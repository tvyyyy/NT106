namespace BobaShop.Domain.Entities;

public class OrderDetailTopping
{
    public int ID { get; set; }

    public int OrderDetailID { get; set; }
    public OrderDetail OrderDetail { get; set; } = null!;

    public int ToppingID { get; set; }
    public Topping Topping { get; set; } = null!;

    public decimal Price { get; set; }              // giá topping chốt lúc đặt
}