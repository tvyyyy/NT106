namespace BobaShop.AdminApp.Services;

/// <summary>Kết quả đăng nhập trả về cho form của AdminApp.</summary>
public sealed record AdminLoginResult
{
    public bool Success { get; init; }
    public string? ErrorMessage { get; init; }
    public string? AccessToken { get; init; }
    public string? FullName { get; init; }
    public string? Email { get; init; }

    public static AdminLoginResult Ok(string accessToken, string fullName, string email)
    {
        return new AdminLoginResult { Success = true, AccessToken = accessToken, FullName = fullName, Email = email };
    }

    public static AdminLoginResult Fail(string errorMessage)
    {
        return new AdminLoginResult { Success = false, ErrorMessage = errorMessage };
    }
}
