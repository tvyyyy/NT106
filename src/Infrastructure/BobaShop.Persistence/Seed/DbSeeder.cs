using BobaShop.Domain.Entities;
using BobaShop.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using BobaShop.Application.Auth;
using Microsoft.Extensions.Configuration;

namespace BobaShop.Persistence.Seed;

public static class DbSeeder
{
    public static async Task SeedAsync(ApplicationDbContext db, IConfiguration configuration, IPasswordHasher passwordHasher, CancellationToken ct = default)
    {
        var adminSection = configuration.GetSection("AdminSeed");
        var admin = new AdminSeedOptions
        {
            Enabled = bool.TryParse(adminSection["Enabled"], out var enabled) && enabled,
            FullName = adminSection["FullName"] ?? "Administrator",
            Phone = adminSection["Phone"] ?? string.Empty,
            Email = adminSection["Email"] ?? string.Empty,
            Password = adminSection["Password"] ?? string.Empty
        };
        if (admin is { Enabled: true })
        {
            if (string.IsNullOrWhiteSpace(admin.Email) || string.IsNullOrWhiteSpace(admin.Phone) || string.IsNullOrWhiteSpace(admin.Password))
                throw new InvalidOperationException("AdminSeed cần Email, Phone và Password trong User Secrets.");
            if (!await db.Users.AnyAsync(u => u.RoleID == 2, ct))
                db.Users.Add(new User { RoleID = 2, FullName = admin.FullName, Phone = admin.Phone, Email = admin.Email.Trim().ToLowerInvariant(), PasswordHash = passwordHasher.Hash(admin.Password) });
        }
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
