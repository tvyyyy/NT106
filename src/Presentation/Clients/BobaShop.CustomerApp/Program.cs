using BobaShop.ClientNetworkLogic;
using BobaShop.CustomerApp.Forms.Auth;
using BobaShop.CustomerApp.Forms.Main;
using BobaShop.CustomerApp.Services;

namespace BobaShop.CustomerApp;

static class Program
{
    private const string ApiArgumentName = "--api";

    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main(string[] args)
    {
        // To customize application configuration such as set high DPI settings or default font,
        // see https://aka.ms/applicationconfiguration.
        ApplicationConfiguration.Initialize();

        try
        {
            Run(GetApiBaseUrl(args));
        }
        catch (Exception ex)
        {
            // Tạm thời: hiện nguyên lỗi lúc khởi động để dễ tìm nguyên nhân. Gỡ khi giao diện đã ổn định.
            MessageBox.Show(ex.ToString(), "BobaShop - Lỗi khởi động", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    /// <summary>
    /// Đọc địa chỉ Web API từ tham số dòng lệnh "--api <địa chỉ>". Không có tham số này thì trả về null
    /// và ứng dụng chạy bằng dữ liệu giả.
    /// </summary>
    private static string? GetApiBaseUrl(string[] args)
    {
        for (var i = 0; i < args.Length - 1; i++)
        {
            if (string.Equals(args[i], ApiArgumentName, StringComparison.OrdinalIgnoreCase))
            {
                return args[i + 1];
            }
        }

        return null;
    }

    private static void Run(string? apiBaseUrl)
    {
        IAuthService authService;
        IProfileService profileService;
        IMenuService menuService;

        IPromotionService promotionService;

        if (apiBaseUrl is null)
        {
            // Bản giả của xác thực giữ luôn danh sách tài khoản nên kiêm cả service hồ sơ.
            var fakeAccounts = new FakeAuthService();
            authService = fakeAccounts;
            profileService = fakeAccounts;
            menuService = new FakeMenuService();
            promotionService = new FakePromotionService();
        }
        else
        {
            // ApiClient sống suốt vòng đời ứng dụng nên không cần Dispose.
            // Địa chỉ gốc phải kết thúc bằng dấu "/" thì các đường dẫn "api/..." mới nối đúng.
            var apiClient = new ApiClient(apiBaseUrl.TrimEnd('/') + "/");
            var realAccounts = new AuthService(apiClient);
            authService = realAccounts;
            profileService = realAccounts;
            menuService = new MenuService(apiClient);
            promotionService = new PromotionService(apiClient);
        }

        // Đăng nhập rồi vào trang chủ. Đăng xuất thì quay lại màn đăng nhập,
        // còn đóng cửa sổ thì thoát ứng dụng.
        while (true)
        {
            LoginResult? session;
            using (var loginForm = new LoginForm(authService))
            {
                System.Windows.Forms.Application.Run(loginForm);
                session = loginForm.Result;
            }

            if (session is null)
            {
                return;
            }

            using var mainForm = new CustomerMainForm(
                session,
                menuService,
                promotionService,
                profileService);
            System.Windows.Forms.Application.Run(mainForm);
            if (!mainForm.LoggedOut)
            {
                return;
            }
        }
    }
}
