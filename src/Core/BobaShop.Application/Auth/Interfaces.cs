using BobaShop.Domain.Entities;

namespace BobaShop.Application.Auth;

public interface IAuthService
{
    Task<RegisterResponse?> RegisterAsync(RegisterRequest request, CancellationToken ct);
    Task<LoginResponse?> LoginAsync(LoginRequest request, CancellationToken ct);
    Task ForgotPasswordAsync(string email, CancellationToken ct);
    Task<bool> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken ct);
}

public interface IPasswordHasher
{
    string Hash(string value);
    bool Verify(string value, string hash);
}

public interface IJwtTokenService
{
    (string Token, DateTime ExpiresAt) CreateToken(User user, string role);
}

public interface IEmailSender
{
    Task SendPasswordResetCodeAsync(string email, string otp, CancellationToken ct);
}

public interface IOtpGenerator
{
    string Generate();
}
