namespace BobaShop.Application.Home;

public record CategoryDto(int CategoryId, string CategoryName);
public record ProductDto(int ProductId, int CategoryId, string ProductName, decimal Price, string? ImageUrl);
