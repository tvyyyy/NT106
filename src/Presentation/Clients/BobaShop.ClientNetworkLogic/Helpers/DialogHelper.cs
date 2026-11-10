namespace BobaShop.ClientNetworkLogic.Helpers;

/// <summary>Hộp thoại thông báo dùng chung, để tiêu đề và biểu tượng thống nhất.</summary>
public static class DialogHelper
{
    public const string AppTitle = "BobaShop";

    public static void ShowInfo(IWin32Window? owner, string message)
    {
        MessageBox.Show(owner, message, AppTitle, MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    public static void ShowError(IWin32Window? owner, string message)
    {
        MessageBox.Show(owner, message, AppTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
}
