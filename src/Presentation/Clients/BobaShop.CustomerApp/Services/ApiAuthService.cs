using BobaShop.ClientNetworkLogic;

namespace BobaShop.CustomerApp.Services;

public sealed class ApiAuthService : IAuthService, IDisposable
{
    private readonly ApiClient _api;

    public ApiAuthService(ApiClient? apiClient = null)
    {
        _api = apiClient ?? new ApiClient();
    }

    public async Task<LoginResult> LoginAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        var result = await _api.PostAsync<LoginRequestDto, LoginResponseDto>(
            "api/auth/login",
            new LoginRequestDto(email, password),
            ct: cancellationToken);

        if (!result.Success || result.Data is null)
            return LoginResult.Fail(result.Message);

        return LoginResult.Ok(result.Data.AccessToken, result.Data.User.FullName, result.Data.User.Role);
    }

    public async Task<AuthActionResult> RegisterAsync(string fullName, string email, string phone, string password, CancellationToken cancellationToken = default)
    {
        var result = await _api.PostAsync<RegisterRequestDto, object>(
            "api/auth/register",
            new RegisterRequestDto(fullName, phone, email, password),
            ct: cancellationToken);

        return result.Success ? AuthActionResult.Ok() : AuthActionResult.Fail(result.Message);
    }

    public async Task<AuthActionResult> SendPasswordResetCodeAsync(string email, CancellationToken cancellationToken = default)
    {
        var result = await _api.PostAsync<ForgotPasswordRequestDto, object>(
            "api/auth/forgot-password",
            new ForgotPasswordRequestDto(email),
            ct: cancellationToken);

        return result.Success ? AuthActionResult.Ok() : AuthActionResult.Fail(result.Message);
    }

    public async Task<AuthActionResult> ResetPasswordAsync(string email, string code, string newPassword, CancellationToken cancellationToken = default)
    {
        var result = await _api.PostAsync<ResetPasswordRequestDto, object>(
            "api/auth/reset-password",
            new ResetPasswordRequestDto(email, code, newPassword),
            ct: cancellationToken);

        return result.Success ? AuthActionResult.Ok() : AuthActionResult.Fail(result.Message);
    }

    public void Dispose() => _api.Dispose();

    private sealed record RegisterRequestDto(string FullName, string Phone, string Email, string Password);
    private sealed record LoginRequestDto(string Email, string Password);
    private sealed record LoginUserDto(int UserId, string FullName, string Email, string Role);
    private sealed record LoginResponseDto(string AccessToken, string TokenType, DateTime ExpiresAt, LoginUserDto User);
    private sealed record ForgotPasswordRequestDto(string Email);
    private sealed record ResetPasswordRequestDto(string Email, string Otp, string NewPassword);
}
