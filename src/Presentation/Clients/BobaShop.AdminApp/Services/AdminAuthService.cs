using BobaShop.ClientNetworkLogic;
using AuthDtos = BobaShop.Application.Auth;

namespace BobaShop.AdminApp.Services;

/// <summary>
/// Đăng nhập AdminApp qua POST /api/auth/login (dùng chung endpoint với CustomerApp).
/// Server trả vai trò trong phản hồi; tài khoản không phải Admin bị từ chối ngay tại đây
/// và token của tài khoản đó không được giữ lại.
/// </summary>
public sealed class AdminAuthService : IAdminAuthService
{
    // Khớp tên vai trò quản lý mà server trả về.
    private const string AdminRole = "Admin";

    private readonly ApiClient _apiClient;

    public AdminAuthService(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<AdminLoginResult> LoginAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        var result = await _apiClient.PostAsync<AuthDtos.LoginRequest, AuthDtos.LoginResponse>(
            "api/auth/login",
            new AuthDtos.LoginRequest(email, password),
            ct: cancellationToken);

        if (!result.Success || result.Data is null || string.IsNullOrEmpty(result.Data.AccessToken))
        {
            return AdminLoginResult.Fail(result.Message);
        }

        var user = result.Data.User;
        if (user is null || !string.Equals(user.Role, AdminRole, StringComparison.Ordinal))
        {
            return AdminLoginResult.Fail("Tài khoản này không có quyền quản trị.");
        }

        return AdminLoginResult.Ok(result.Data.AccessToken, user.FullName, user.Email);
    }
}
