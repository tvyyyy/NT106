namespace BobaShop.Domain.Entities;

public class DiningTable
{
    public int TableID { get; set; }
    public string TableNumber { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public bool IsActive { get; set; } = true;
    public ICollection<Order> Orders { get; set; } = new List<Order>();
}
