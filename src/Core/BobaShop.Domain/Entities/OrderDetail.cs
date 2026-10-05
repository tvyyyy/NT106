using BobaShop.Domain.Enums;

namespace BobaShop.Domain.Entities;

public class OrderDetail
{
    public int OrderDetailID { get; set; }

    public int OrderID { get; set; }
    public Order Order { get; set; } = null!;

    public int ProductID { get; set; }
    public Product Product { get; set; } = null!;

    public int Quantity { get; set; }
    public DrinkSize Size { get; set; }
    public SugarLevel SugarLevel { get; set; }
    public IceLevel IceLevel { get; set; }
    public string? Note { get; set; }
    public decimal UnitPrice { get; set; }          // giá chốt lúc đặt (đã gồm size)

    public ICollection<OrderDetailTopping> OrderDetailToppings { get; set; } = new List<OrderDetailTopping>();
}