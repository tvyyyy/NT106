using BobaShop.Domain.Entities;
using BobaShop.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace BobaShop.Persistence.Seed;

public static class DbSeeder
{
    public static async Task SeedAsync(ApplicationDbContext db, CancellationToken ct = default)
    {
        if (!await db.Categories.AnyAsync(ct))
        {
            var milkTea = new Category { CategoryName = "Trà sữa" };
            var fruitTea = new Category { CategoryName = "Trà trái cây" };

            db.Products.AddRange(
                new Product { Category = milkTea, ProductName = "Trà sữa trân châu", Price = 35000 },
                new Product { Category = milkTea, ProductName = "Trà sữa matcha", Price = 40000 },
                new Product { Category = fruitTea, ProductName = "Trà đào cam sả", Price = 38000 },
                new Product { Category = fruitTea, ProductName = "Trà vải", Price = 36000 });
        }

        if (!await db.Toppings.AnyAsync(ct))
        {
            db.Toppings.AddRange(
                new Topping { ToppingName = "Trân châu đen", Price = 5000 },
                new Topping { ToppingName = "Thạch dừa", Price = 5000 },
                new Topping { ToppingName = "Pudding", Price = 7000 });
        }

        await db.SaveChangesAsync(ct);
    }
}