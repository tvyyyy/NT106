namespace BobaShop.CustomerApp.Services;

/// <summary>Kết quả của thao tác xác thực không trả dữ liệu: đăng ký, gửi mã, đặt lại mật khẩu.</summary>
public sealed record AuthActionResult
{
    public bool Success { get; init; }
    public string? ErrorMessage { get; init; }

    public static AuthActionResult Ok()
    {
        return new AuthActionResult { Success = true };
    }

    public static AuthActionResult Fail(string errorMessage)
    {
        return new AuthActionResult { Success = false, ErrorMessage = errorMessage };
    }
}
