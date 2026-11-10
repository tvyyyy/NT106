namespace BobaShop.CustomerApp.Services;

/// <summary>
/// Bản giả để dựng giao diện khi API chưa xong (theo mục 4 của Quy chuẩn). Dữ liệu chỉ nằm trong bộ nhớ,
/// tắt ứng dụng là mất. Khi có AuthController thật thì viết AuthService gọi ApiClient
/// và đổi dòng khởi tạo trong Program.cs.
/// </summary>
public sealed class FakeAuthService : IAuthService
{
    public const string DemoEmail = "demo@bobashop.vn";
    public const string DemoPassword = "123456";
    public const string LockedEmail = "locked@bobashop.vn";

    /// <summary>Mã xác nhận cố định của bản giả, thay cho mã gửi qua email.</summary>
    public const string DemoResetCode = "123456";

    private static readonly TimeSpan FakeNetworkDelay = TimeSpan.FromMilliseconds(600);

    private readonly Dictionary<string, FakeUser> _users = new(StringComparer.OrdinalIgnoreCase)
    {
        [DemoEmail] = new FakeUser("Khách Demo", DemoPassword, false),
        [LockedEmail] = new FakeUser("Khách Bị Khóa", DemoPassword, true),
    };

    private readonly HashSet<string> _pendingResets = new(StringComparer.OrdinalIgnoreCase);

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

        return LoginResult.Ok("fake-jwt-token", user.FullName, "Customer");
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

        _users[email] = new FakeUser(fullName, password, false);
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

    private sealed record FakeUser(string FullName, string Password, bool IsLocked);
}
