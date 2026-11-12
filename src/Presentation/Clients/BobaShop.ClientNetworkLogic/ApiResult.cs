using System.Net;

namespace BobaShop.ClientNetworkLogic;

public sealed record ApiResult<T>(bool Success, string Message, T? Data, HttpStatusCode StatusCode)
{
    public static ApiResult<T> Ok(string message, T? data, HttpStatusCode statusCode = HttpStatusCode.OK) =>
        new(true, message, data, statusCode);

    public static ApiResult<T> Fail(string message, HttpStatusCode statusCode = 0) =>
        new(false, message, default, statusCode);
}
