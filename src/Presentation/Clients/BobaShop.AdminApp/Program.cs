using BobaShop.AdminApp.Forms.Auth;
using BobaShop.AdminApp.Forms.Main;
using BobaShop.AdminApp.Services;
using BobaShop.ClientNetworkLogic;
using BobaShop.ClientNetworkLogic.Helpers;

namespace BobaShop.AdminApp;

static class Program
{
    private const string ApiArgumentName = "--api";
    private const string DefaultApiBaseUrl = "http://localhost:5227";

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
            // Lỗi không lường trước: báo cho người dùng rồi thoát, thay vì để ứng dụng tắt đột ngột.
            DialogHelper.ShowError(null, $"Ứng dụng gặp lỗi và cần đóng lại.\n\nChi tiết: {ex.Message}");
        }
    }

    /// <summary>
    /// Đọc địa chỉ Web API từ tham số dòng lệnh "--api <địa chỉ>". Không có thì dùng địa chỉ mặc định
    /// của Web API khi chạy trên máy phát triển.
    /// </summary>
    private static string GetApiBaseUrl(string[] args)
    {
        for (var i = 0; i < args.Length - 1; i++)
        {
            if (string.Equals(args[i], ApiArgumentName, StringComparison.OrdinalIgnoreCase))
            {
                return args[i + 1];
            }
        }

        return DefaultApiBaseUrl;
    }

    private static void Run(string apiBaseUrl)
    {
        // Địa chỉ gốc phải kết thúc bằng dấu "/" thì các đường dẫn "api/..." mới nối đúng.
        // ApiClient sống suốt vòng đời ứng dụng nên không cần Dispose.
        var apiClient = new ApiClient(apiBaseUrl.TrimEnd('/') + "/");
        IAdminAuthService authService = new AdminAuthService(apiClient);

        // Đăng nhập rồi vào khung chính. Đăng xuất thì quay lại màn đăng nhập,
        // còn đóng cửa sổ thì thoát ứng dụng.
        while (true)
        {
            AdminLoginResult? session;
            using (var loginForm = new AdminLoginForm(authService))
            {
                System.Windows.Forms.Application.Run(loginForm);
                session = loginForm.Result;
            }

            if (session is null)
            {
                return;
            }

            using var mainForm = new AdminMainForm(session);
            System.Windows.Forms.Application.Run(mainForm);
            if (!mainForm.LoggedOut)
            {
                return;
            }
        }
    }
}
