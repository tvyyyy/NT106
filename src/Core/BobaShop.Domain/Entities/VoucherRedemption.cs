namespace BobaShop.Domain.Entities;

public class VoucherRedemption
{
    public int RedemptionID { get; set; }
    public int VoucherID { get; set; }
    public Voucher Voucher { get; set; } = null!;
    public int UserID { get; set; }
    public User User { get; set; } = null!;
    public int OrderID { get; set; }
    public Order Order { get; set; } = null!;
    public DateTime RedeemedAt { get; set; } = DateTime.UtcNow;
}
