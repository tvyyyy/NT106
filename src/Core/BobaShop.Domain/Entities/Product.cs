namespace BobaShop.Domain.Entities;

public class Product
{
    public int ProductID { get; set; }

    public int CategoryID { get; set; }
    public Category Category { get; set; } = null!;

    public string ProductName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string? ImageURL { get; set; }           // có thể chưa có ảnh
    public bool IsAvailable { get; set; } = true;  // false = ẩn món khi hết hàng

    public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
    public ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();
}