namespace BobaShop.Domain.Entities;

public class Feedback
{
    public int FeedbackID { get; set; }

    public int UserID { get; set; }
    public User User { get; set; } = null!;

    public int OrderID { get; set; }
    public Order Order { get; set; } = null!;

    public int ProductID { get; set; }              // MỚI: đánh giá theo từng món
    public Product Product { get; set; } = null!;

    public int Rating { get; set; }                 // 1 đến 5
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}