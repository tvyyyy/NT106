namespace BobaShop.Application.Contracts;

public record ApiResponse<T>(bool Success, string Message, T? Data)
{
    public static ApiResponse<T> Ok(string message, T? data) => new(true, message, data);
    public static ApiResponse<T> Fail(string message) => new(false, message, default);
}
