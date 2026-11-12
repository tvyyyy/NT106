using BobaShop.ClientNetworkLogic;
using AuthDtos = BobaShop.Application.Auth;

namespace BobaShop.CustomerApp.Services;

/// <summary>
/// Service xác thực gọi Web API thật theo docs/api-contract-2026-11-09.md, dùng DTO của Application.
/// Kiêm service hồ sơ ở mức tạm: hợp đồng API chưa có endpoint hồ sơ, nên hồ sơ chỉ gồm
/// những gì phản hồi đăng nhập trả về (họ tên, email).
/// TODO: khi Ngân có SessionManager thì lưu token và thông tin người dùng vào đó thay vì giữ trong lớp này.
/// </summary>
public sealed class AuthService : IAuthService, IProfileService
{
    private const string DefaultTier = "Thành viên";

    private readonly ApiClient _apiClient;
    private AuthDtos.LoginUserResponse? _currentUser;

    public AuthService(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    /// <summary>JWT của phiên đăng nhập hiện tại, null nếu chưa đăng nhập. Các service khác dùng để gọi API cần xác thực.</summary>
    public string? AccessToken { get; private set; }

    public async Task<LoginResult> LoginAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        var result = await _apiClient.PostAsync<AuthDtos.LoginRequest, AuthDtos.LoginResponse>(
            "api/auth/login",
            new AuthDtos.LoginRequest(email, password),
            ct: cancellationToken);

        if (!result.Success || result.Data is null || string.IsNullOrEmpty(result.Data.AccessToken))
        {
            return LoginResult.Fail(result.Message);
        }

        AccessToken = result.Data.AccessToken;
        _currentUser = result.Data.User;
        return LoginResult.Ok(
            result.Data.AccessToken,
            result.Data.User?.FullName ?? string.Empty,
            result.Data.User?.Role ?? string.Empty);
    }

    public async Task<AuthActionResult> RegisterAsync(
        string fullName,
        string email,
        string phone,
        string password,
        CancellationToken cancellationToken = default)
    {
        var result = await _apiClient.PostAsync<AuthDtos.RegisterRequest, AuthDtos.RegisterResponse>(
            "api/auth/register",
            new AuthDtos.RegisterRequest(fullName, phone, email, password),
            ct: cancellationToken);
        return result.Success ? AuthActionResult.Ok() : AuthActionResult.Fail(result.Message);
    }

    public async Task<AuthActionResult> SendPasswordResetCodeAsync(string email, CancellationToken cancellationToken = default)
    {
        var result = await _apiClient.PostAsync<AuthDtos.ForgotPasswordRequest, object>(
            "api/auth/forgot-password",
            new AuthDtos.ForgotPasswordRequest(email),
            ct: cancellationToken);
        return result.Success ? AuthActionResult.Ok() : AuthActionResult.Fail(result.Message);
    }

    public async Task<AuthActionResult> ResetPasswordAsync(
        string email,
        string code,
        string newPassword,
        CancellationToken cancellationToken = default)
    {
        var result = await _apiClient.PostAsync<AuthDtos.ResetPasswordRequest, object>(
            "api/auth/reset-password",
            new AuthDtos.ResetPasswordRequest(email, code, newPassword),
            ct: cancellationToken);
        return result.Success ? AuthActionResult.Ok() : AuthActionResult.Fail(result.Message);
    }

    public Task<CustomerProfile> GetMyProfileAsync(CancellationToken cancellationToken = default)
    {
        if (_currentUser is null)
        {
            throw new InvalidOperationException("Chưa có tài khoản nào đăng nhập.");
        }

        // Số điện thoại, ngày sinh và ngày đăng ký chưa có API trả về nên để trống.
        return Task.FromResult(new CustomerProfile(
            _currentUser.FullName,
            _currentUser.Email,
            string.Empty,
            null,
            null,
            DefaultTier));
    }
}
