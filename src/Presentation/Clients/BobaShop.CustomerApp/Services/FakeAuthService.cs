namespace BobaShop.CustomerApp.Services;

/// <summary>
/// Bản giả để dựng giao diện khi API chưa xong (theo mục 4 của Quy chuẩn). Dữ liệu chỉ nằm trong bộ nhớ,
/// tắt ứng dụng là mất. Bản thật là AuthService, Program.cs chọn bản nào theo tham số dòng lệnh.
/// Lớp này đóng luôn vai service hồ sơ vì nó là nơi giữ danh sách tài khoản giả và biết ai đang đăng nhập.
/// </summary>
public sealed class FakeAuthService : IAuthService, IProfileService
{
    public const string DemoEmail = "demo@bobashop.vn";
    public const string DemoPassword = "123456";
    public const string LockedEmail = "locked@bobashop.vn";

    /// <summary>Mã xác nhận cố định của bản giả, thay cho mã gửi qua email.</summary>
    public const string DemoResetCode = "123456";

    // Khớp tên vai trò server trả về trong phản hồi đăng nhập.
    private const string CustomerRole = "Customer";

    private static readonly TimeSpan FakeNetworkDelay = TimeSpan.FromMilliseconds(600);

    private readonly Dictionary<string, FakeUser> _users = new(StringComparer.OrdinalIgnoreCase)
    {
        [DemoEmail] = new FakeUser("Khách Demo", DemoEmail, DemoPassword, false),
        [LockedEmail] = new FakeUser("Khách Bị Khóa", LockedEmail, DemoPassword, true),
    };

    private readonly HashSet<string> _pendingResets = new(StringComparer.OrdinalIgnoreCase);

    // Email của tài khoản đăng nhập gần nhất, dùng để trả hồ sơ.
    private string? _currentEmail;

    public async Task<LoginResult> LoginAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        await Task.Delay(FakeNetworkDelay, cancellationToken);

        if (!_users.TryGetValue(email, out var user) || user.Password != password)
        {
            return LoginResult.Fail("Email hoặc mật khẩu không đúng.");
        }

        if (user.IsLocked)
        {
            return LoginResult.Fail("Tài khoản đã bị khóa. Vui lòng liên hệ cửa hàng.");
        }

        _currentEmail = user.Email;
        return LoginResult.Ok("fake-jwt-token", user.FullName, CustomerRole);
    }

    public async Task<AuthActionResult> RegisterAsync(
        string fullName,
        string email,
        string phone,
        string password,
        CancellationToken cancellationToken = default)
    {
        await Task.Delay(FakeNetworkDelay, cancellationToken);

        if (_users.ContainsKey(email))
        {
            return AuthActionResult.Fail("Email này đã được đăng ký.");
        }

        _users[email] = new FakeUser(fullName, email, password, false);
        return AuthActionResult.Ok();
    }

    public async Task<AuthActionResult> SendPasswordResetCodeAsync(string email, CancellationToken cancellationToken = default)
    {
        await Task.Delay(FakeNetworkDelay, cancellationToken);

        if (_users.ContainsKey(email))
        {
            _pendingResets.Add(email);
        }

        return AuthActionResult.Ok();
    }

    public async Task<AuthActionResult> ResetPasswordAsync(
        string email,
        string code,
        string newPassword,
        CancellationToken cancellationToken = default)
    {
        await Task.Delay(FakeNetworkDelay, cancellationToken);

        if (!_pendingResets.Contains(email) || code != DemoResetCode || !_users.TryGetValue(email, out var user))
        {
            return AuthActionResult.Fail("Mã xác nhận không đúng hoặc đã hết hạn.");
        }

        _users[email] = user with { Password = newPassword };
        _pendingResets.Remove(email);
        return AuthActionResult.Ok();
    }

    public async Task<CustomerProfile> GetMyProfileAsync(CancellationToken cancellationToken = default)
    {
        await Task.Delay(FakeNetworkDelay, cancellationToken);

        if (_currentEmail is null || !_users.TryGetValue(_currentEmail, out var user))
        {
            throw new InvalidOperationException("Chưa có tài khoản nào đăng nhập.");
        }

        return new CustomerProfile(user.FullName, user.Email, CustomerRole);
    }

    private sealed record FakeUser(string FullName, string Email, string Password, bool IsLocked);
}
