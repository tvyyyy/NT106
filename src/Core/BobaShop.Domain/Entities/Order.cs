using BobaShop.Domain.Enums;

namespace BobaShop.Domain.Entities;

public class Order
{
    public int OrderID { get; set; }

    public int UserID { get; set; }
    public User User { get; set; } = null!;

    public int? VoucherID { get; set; }             // nullable: đơn không bắt buộc có voucher
    public Voucher? Voucher { get; set; }
    public int? TableID { get; set; }
    public DiningTable? DiningTable { get; set; }
    public long? PayOSOrderCode { get; set; }
    public DateTime? PaidAt { get; set; }
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Unpaid;

    public OrderType OrderType { get; set; }
    public decimal TotalAmount { get; set; }        // tổng tiền trước giảm
    public decimal DiscountAmount { get; set; }     // số tiền được giảm
    public decimal FinalAmount { get; set; }        // = Total - Discount
    public PaymentMethod PaymentMethod { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
    public ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();
    public ICollection<VoucherRedemption> VoucherRedemptions { get; set; } = new List<VoucherRedemption>();
}
