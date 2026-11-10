namespace BobaShop.CustomerApp.Services;

/// <summary>
/// Service xác thực phía client. Form chỉ gọi interface này, không gọi ApiClient hay HttpClient.
/// </summary>
public interface IAuthService
{
    Task<LoginResult> LoginAsync(string email, string password, CancellationToken cancellationToken = default);

    Task<AuthActionResult> RegisterAsync(
        string fullName,
        string email,
        string phone,
        string password,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Yêu cầu gửi mã xác nhận 6 số tới email. Luôn trả về thành công dù email có tồn tại hay không,
    /// để không lộ email nào đã đăng ký.
    /// </summary>
    Task<AuthActionResult> SendPasswordResetCodeAsync(string email, CancellationToken cancellationToken = default);

    Task<AuthActionResult> ResetPasswordAsync(
        string email,
        string code,
        string newPassword,
        CancellationToken cancellationToken = default);
}
