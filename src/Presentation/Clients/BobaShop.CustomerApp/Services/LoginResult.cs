namespace BobaShop.CustomerApp.Services;

/// <summary>
/// Kết quả đăng nhập trả về cho Form.
/// TODO: khi Vy có ApiResult và DTO đăng nhập trong Application thì bản cài đặt thật
/// chuyển đổi sang kiểu này, Form không phải sửa.
/// </summary>
public sealed record LoginResult
{
    public bool Success { get; init; }
    public string? ErrorMessage { get; init; }
    public string? AccessToken { get; init; }
    public string? FullName { get; init; }
    public string? Role { get; init; }

    public static LoginResult Ok(string accessToken, string fullName, string role)
    {
        return new LoginResult { Success = true, AccessToken = accessToken, FullName = fullName, Role = role };
    }

    public static LoginResult Fail(string errorMessage)
    {
        return new LoginResult { Success = false, ErrorMessage = errorMessage };
    }
}
