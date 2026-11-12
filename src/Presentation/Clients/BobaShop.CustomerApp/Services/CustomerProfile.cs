namespace BobaShop.CustomerApp.Services;

/// <summary>
/// Hồ sơ khách hàng hiển thị ở cửa sổ hồ sơ. Chỉ gồm những gì Web API hiện trả về cho client
/// (thông tin người dùng trong phản hồi đăng nhập): họ tên, email và vai trò.
/// </summary>
public sealed record CustomerProfile(string FullName, string Email, string Role);
