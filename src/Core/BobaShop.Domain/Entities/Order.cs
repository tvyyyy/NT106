using BobaShop.Domain.Enums;

namespace BobaShop.Domain.Entities;

public class Order
{
    public int OrderID { get; set; }

    public int UserID { get; set; }
    public User User { get; set; } = null!;

    public int? VoucherID { get; set; }             // nullable: đơn không bắt buộc có voucher
    public Voucher? Voucher { get; set; }

    public OrderType OrderType { get; set; }
    public decimal TotalAmount { get; set; }        // tổng tiền trước giảm
    public decimal DiscountAmount { get; set; }     // số tiền được giảm
    public decimal FinalAmount { get; set; }        // = Total - Discount
    public PaymentMethod PaymentMethod { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
    public ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();
}