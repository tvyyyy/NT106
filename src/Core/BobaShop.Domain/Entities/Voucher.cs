namespace BobaShop.Domain.Entities;

public class Voucher
{
    public int VoucherID { get; set; }
    public string Code { get; set; } = string.Empty;   // sẽ đặt UNIQUE index
    public decimal DiscountValue { get; set; }
    public decimal MinOrderValue { get; set; }
    public decimal MaxDiscountAmount { get; set; }
    public int Quantity { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime ExpiryDate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Order> Orders { get; set; } = new List<Order>();
}