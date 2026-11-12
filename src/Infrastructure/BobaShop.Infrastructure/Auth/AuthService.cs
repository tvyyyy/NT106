using System.Security.Cryptography;
using System.Text;
using BobaShop.Application.Auth;
using BobaShop.Domain.Entities;
using BobaShop.Domain.Enums;
using BobaShop.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace BobaShop.Infrastructure.Auth;

public class AuthService(ApplicationDbContext db, IPasswordHasher hasher, IJwtTokenService jwt,
    IEmailSender emailSender, IOtpGenerator otpGenerator) : IAuthService
{
    public async Task<RegisterResponse?> RegisterAsync(RegisterRequest request, CancellationToken ct)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var phone = request.Phone.Trim();
        if (await db.Users.AnyAsync(u => u.Email == email || u.Phone == phone, ct)) return null;
        var user = new User { RoleID = (int)UserRole.Customer, FullName = request.FullName.Trim(), Phone = phone,
            Email = email, PasswordHash = hasher.Hash(request.Password) };
        db.Users.Add(user);
        await db.SaveChangesAsync(ct);
        return new(user.UserID, user.FullName, user.Phone, user.Email);
    }

    public async Task<LoginResponse?> LoginAsync(LoginRequest request, CancellationToken ct)
    {
        var user = await db.Users.Include(u => u.Role).SingleOrDefaultAsync(u => u.Email == request.Email.Trim().ToLowerInvariant(), ct);
        if (user is null || user.Status == UserStatus.Locked || !hasher.Verify(request.Password, user.PasswordHash)) return null;
        var token = jwt.CreateToken(user, user.Role.RoleName);
        return new(token.Token, "Bearer", token.ExpiresAt, new(user.UserID, user.FullName, user.Email, user.Role.RoleName));
    }

    public async Task ForgotPasswordAsync(string email, CancellationToken ct)
    {
        var normalized = email.Trim().ToLowerInvariant();
        var user = await db.Users.SingleOrDefaultAsync(u => u.Email == normalized, ct);
        if (user is null) return;
        var now = DateTime.UtcNow;
        var latest = await db.PasswordResetTokens.Where(t => t.UserID == user.UserID).OrderByDescending(t => t.CreatedAt).FirstOrDefaultAsync(ct);
        if (latest is not null && latest.CreatedAt > now.AddSeconds(-60)) return;
        var otp = otpGenerator.Generate();
        db.PasswordResetTokens.Add(new PasswordResetToken { UserID = user.UserID, TokenHash = HashOtp(otp), CreatedAt = now, ExpiresAt = now.AddMinutes(10) });
        await db.SaveChangesAsync(ct);
        await emailSender.SendPasswordResetCodeAsync(user.Email, otp, ct);
    }

    public async Task<bool> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken ct)
    {
        var user = await db.Users.SingleOrDefaultAsync(u => u.Email == request.Email.Trim().ToLowerInvariant(), ct);
        if (user is null) return false;
        var now = DateTime.UtcNow;
        var token = await db.PasswordResetTokens.Where(t => t.UserID == user.UserID && t.UsedAt == null && t.ExpiresAt > now)
            .OrderByDescending(t => t.CreatedAt).FirstOrDefaultAsync(ct);
        if (token is null || !CryptographicOperations.FixedTimeEquals(Convert.FromHexString(token.TokenHash), Convert.FromHexString(HashOtp(request.Otp)))) return false;
        token.UsedAt = now;
        user.PasswordHash = hasher.Hash(request.NewPassword);
        await db.SaveChangesAsync(ct);
        return true;
    }

    private static string HashOtp(string otp) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(otp)));
}
