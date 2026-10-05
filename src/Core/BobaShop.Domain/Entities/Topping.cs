namespace BobaShop.Domain.Entities;

public class Topping
{
    public int ToppingID { get; set; }
    public string ToppingName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public bool IsAvailable { get; set; } = true;

    public ICollection<OrderDetailTopping> OrderDetailToppings { get; set; } = new List<OrderDetailTopping>();
}