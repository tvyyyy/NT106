namespace BobaShop.CustomerApp.Services;

/// <summary>Service hồ sơ phía client. Form chỉ gọi interface này, không gọi ApiClient hay HttpClient.</summary>
public interface IProfileService
{
    /// <summary>Hồ sơ của tài khoản đang đăng nhập.</summary>
    Task<CustomerProfile> GetMyProfileAsync(CancellationToken cancellationToken = default);
}
