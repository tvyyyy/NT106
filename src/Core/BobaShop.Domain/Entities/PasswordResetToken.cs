namespace BobaShop.Domain.Entities;

public class PasswordResetToken
{
    public int TokenID { get; set; }

    public int UserID { get; set; }
    public User User { get; set; } = null!;

    public string TokenHash { get; set; } = string.Empty;   // hash của OTP 6 số, không lưu OTP gốc
    public DateTime ExpiresAt { get; set; }
    public DateTime? UsedAt { get; set; }                   // null = chưa dùng
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}