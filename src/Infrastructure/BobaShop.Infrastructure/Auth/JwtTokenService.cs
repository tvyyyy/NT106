using System.Security.Claims;
using System.Text;
using BobaShop.Application.Auth;
using BobaShop.Domain.Entities;
using System.Security.Cryptography;

namespace BobaShop.Infrastructure.Auth;

public class JwtTokenService(AuthOptions settings) : IJwtTokenService
{
    public (string Token, DateTime ExpiresAt) CreateToken(User user, string role)
    {
        var expires = DateTime.UtcNow.AddMinutes(settings.ExpiryMinutes);
        var claims = new[]
        {
            new Claim("sub", user.UserID.ToString()),
            new Claim("userId", user.UserID.ToString()),
            new Claim("email", user.Email),
            new Claim(ClaimTypes.Role, role)
        };
        var header = Base64UrlEncoder.Encode("{\"alg\":\"HS256\",\"typ\":\"JWT\"}");
        var payload = Base64UrlEncoder.Encode(System.Text.Json.JsonSerializer.Serialize(new
        {
            iss = settings.Issuer, aud = settings.Audience, sub = user.UserID.ToString(),
            userId = user.UserID.ToString(), email = user.Email, role, iat = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            exp = new DateTimeOffset(expires).ToUnixTimeSeconds()
        }));
        var unsigned = $"{header}.{payload}";
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(settings.Key));
        return ($"{unsigned}.{Base64UrlEncoder.Encode(hmac.ComputeHash(Encoding.UTF8.GetBytes(unsigned)))}", expires);
    }

    private static class Base64UrlEncoder
    {
        public static string Encode(string value) => Encode(Encoding.UTF8.GetBytes(value));
        public static string Encode(byte[] bytes) => Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }
}
