namespace BobaShop.Application.Home;

public record CategoryDto(int CategoryId, string CategoryName);
public record ProductDto(int ProductId, int CategoryId, string ProductName, decimal Price, string? ImageUrl);
public record PromotionDto(int PromotionId, string Title, string Description, string Code, DateTime ExpiresAt);
