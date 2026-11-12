using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace BobaShop.ClientNetworkLogic;

/// <summary>Small JSON client for the Boba Shop Web API.</summary>
public sealed class ApiClient : IDisposable
{
    private readonly HttpClient _http;
    private readonly bool _ownsClient;
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public ApiClient(string baseAddress = "http://localhost:5227/")
        : this(new HttpClient { BaseAddress = new Uri(baseAddress, UriKind.Absolute), Timeout = TimeSpan.FromSeconds(20) }, ownsClient: true)
    {
    }

    public ApiClient(HttpClient httpClient, bool ownsClient = false)
    {
        _http = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _ownsClient = ownsClient;
    }

    public Task<ApiResult<TResponse>> GetAsync<TResponse>(string route, string? accessToken = null, CancellationToken ct = default) =>
        SendAsync<TResponse>(new HttpRequestMessage(HttpMethod.Get, route), accessToken, ct);

    public Task<ApiResult<TResponse>> PostAsync<TRequest, TResponse>(string route, TRequest body, string? accessToken = null, CancellationToken ct = default)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, route)
        {
            Content = JsonContent.Create(body, options: JsonOptions)
        };
        return SendAsync<TResponse>(request, accessToken, ct);
    }

    private async Task<ApiResult<TResponse>> SendAsync<TResponse>(HttpRequestMessage request, string? accessToken, CancellationToken ct)
    {
        using (request)
        {
            request.Headers.Accept.ParseAdd("application/json");
            if (!string.IsNullOrWhiteSpace(accessToken))
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            try
            {
                using var response = await _http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
                ApiEnvelope<TResponse>? payload;
                try
                {
                    payload = await response.Content.ReadFromJsonAsync<ApiEnvelope<TResponse>>(JsonOptions, ct);
                }
                catch (JsonException)
                {
                    var message = response.IsSuccessStatusCode
                        ? "Máy chủ trả về dữ liệu không đúng định dạng."
                        : $"Yêu cầu thất bại (HTTP {(int)response.StatusCode}).";
                    return ApiResult<TResponse>.Fail(message, response.StatusCode);
                }
                if (payload is null)
                {
                    var message = response.IsSuccessStatusCode ? "Máy chủ trả về dữ liệu rỗng." : $"Yêu cầu thất bại (HTTP {(int)response.StatusCode}).";
                    return ApiResult<TResponse>.Fail(message, response.StatusCode);
                }
                if (!response.IsSuccessStatusCode || !payload.Success)
                    return ApiResult<TResponse>.Fail(string.IsNullOrWhiteSpace(payload.Message) ? "Yêu cầu không thành công." : payload.Message, response.StatusCode);
                return ApiResult<TResponse>.Ok(payload.Message, payload.Data, response.StatusCode);
            }
            catch (HttpRequestException)
            {
                return ApiResult<TResponse>.Fail("Không kết nối được máy chủ. Hãy kiểm tra Web API.");
            }
            catch (TaskCanceledException) when (!ct.IsCancellationRequested)
            {
                return ApiResult<TResponse>.Fail("Máy chủ phản hồi quá lâu.");
            }
            catch (JsonException)
            {
                return ApiResult<TResponse>.Fail("Dữ liệu máy chủ trả về không hợp lệ.");
            }
        }
    }

    public void Dispose()
    {
        if (_ownsClient) _http.Dispose();
    }

    private sealed record ApiEnvelope<T>(bool Success, string Message, T? Data);
}
