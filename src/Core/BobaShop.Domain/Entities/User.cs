using BobaShop.Domain.Enums;

namespace BobaShop.Domain.Entities;

public class User
{
    public int UserID { get; set; }

    public int RoleID { get; set; }                 // khóa ngoại
    public Role Role { get; set; } = null!;         // navigation tới Role

    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserStatus Status { get; set; } = UserStatus.Active;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Order> Orders { get; set; } = new List<Order>();
    public ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();
}