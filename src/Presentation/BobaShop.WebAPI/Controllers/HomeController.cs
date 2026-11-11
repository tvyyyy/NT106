using BobaShop.Application.Contracts;
using BobaShop.Application.Home;
using BobaShop.Domain.Enums;
using BobaShop.Persistence.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BobaShop.WebAPI.Controllers;

[ApiController]
[Route("api")]
public class HomeController(ApplicationDbContext db) : ControllerBase
{
    [HttpGet("categories")]
    public async Task<IActionResult> Categories(CancellationToken ct)
    {
        var data = await db.Categories.AsNoTracking().OrderBy(c => c.CategoryID)
            .Select(c => new CategoryDto(c.CategoryID, c.CategoryName)).ToListAsync(ct);
        return Ok(ApiResponse<IReadOnlyList<CategoryDto>>.Ok("Lấy danh sách danh mục thành công.", data));
    }

    [HttpGet("products")]
    public async Task<IActionResult> Products([FromQuery] bool? featured, CancellationToken ct)
    {
        var data = await db.Products.AsNoTracking().Where(p => p.IsAvailable).OrderBy(p => p.ProductID)
            .Select(p => new ProductDto(p.ProductID, p.CategoryID, p.ProductName, p.Price, p.ImageURL)).ToListAsync(ct);
        return Ok(ApiResponse<IReadOnlyList<ProductDto>>.Ok("Lấy danh sách sản phẩm thành công.", data));
    }

    [HttpGet("promotions")]
    public async Task<IActionResult> Promotions(CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var vouchers = await db.Vouchers.AsNoTracking()
            .Where(v => v.IsActive && v.Quantity > 0 && v.ExpiryDate > now)
            .OrderBy(v => v.ExpiryDate)
            .Select(v => new { v.VoucherID, v.Code, v.DiscountType, v.DiscountValue, v.MinOrderValue, v.ExpiryDate })
            .ToListAsync(ct);
        var data = vouchers.Select(v =>
        {
            var discount = v.DiscountType == DiscountType.Percent
                ? $"Giảm {v.DiscountValue:0.##}%"
                : $"Giảm {v.DiscountValue:N0}đ";
            var title = $"Ưu đãi {v.Code}";
            var description = v.MinOrderValue > 0
                ? $"{discount} cho đơn từ {v.MinOrderValue:N0}đ. Nhập mã {v.Code} khi đặt món."
                : $"{discount}. Nhập mã {v.Code} khi đặt món.";
            return new PromotionDto(v.VoucherID, title, description, v.Code, v.ExpiryDate);
        }).ToList();
        return Ok(ApiResponse<IReadOnlyList<PromotionDto>>.Ok("Lấy danh sách ưu đãi thành công.", data));
    }
}
