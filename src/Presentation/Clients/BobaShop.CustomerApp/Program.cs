using BobaShop.CustomerApp.Forms.Auth;
using BobaShop.CustomerApp.Services;

namespace BobaShop.CustomerApp;

static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        // To customize application configuration such as set high DPI settings or default font,
        // see https://aka.ms/applicationconfiguration.
        ApplicationConfiguration.Initialize();

        try
        {
            Run();
        }
        catch (Exception ex)
        {
            // Tạm thời: hiện nguyên lỗi lúc khởi động để dễ tìm nguyên nhân. Gỡ khi giao diện đã ổn định.
            MessageBox.Show(ex.ToString(), "BobaShop - Lỗi khởi động", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private static void Run()
    {
        // TODO: đổi sang AuthService thật (gọi ApiClient) khi Vy xong AuthController.
        IAuthService authService = new FakeAuthService();

        // Chạy màn đăng nhập trước. Đóng cửa sổ mà chưa đăng nhập thì thoát ứng dụng.
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

        // TODO: thay Form1 bằng CustomerMainForm khi làm xong trang chủ.
        System.Windows.Forms.Application.Run(new Form1());
    }
}
