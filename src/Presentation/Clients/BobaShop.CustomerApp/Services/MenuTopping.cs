namespace BobaShop.CustomerApp.Services;

/// <summary>Topping khách chọn thêm vào đồ uống, theo cấu trúc trả về của GET /api/toppings.</summary>
public sealed record MenuTopping(int ToppingId, string ToppingName, decimal Price);
