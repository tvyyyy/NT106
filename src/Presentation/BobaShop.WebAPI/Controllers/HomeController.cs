using BobaShop.Application.Contracts;
using BobaShop.Application.Home;
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
}
