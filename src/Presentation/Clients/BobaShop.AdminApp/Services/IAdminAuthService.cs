namespace BobaShop.AdminApp.Services;

/// <summary>Service xác thực của AdminApp. Form chỉ gọi interface này, không gọi ApiClient hay HttpClient.</summary>
public interface IAdminAuthService
{
    /// <summary>Đăng nhập và chỉ chấp nhận tài khoản có vai trò Admin.</summary>
    Task<AdminLoginResult> LoginAsync(string email, string password, CancellationToken cancellationToken = default);
}
