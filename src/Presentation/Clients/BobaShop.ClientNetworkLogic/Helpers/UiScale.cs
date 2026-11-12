namespace BobaShop.ClientNetworkLogic.Helpers;

/// <summary>
/// Tỉ lệ hiển thị của màn hình (100%, 125%, 150%...). Chữ tính bằng point tự to theo tỉ lệ này,
/// còn kích thước control tính bằng pixel thì không, nên form dựng bằng code phải nhân qua Px.
/// </summary>
public static class UiScale
{
    /// <summary>1.0 ở 100%, 1.25 ở 125%...</summary>
    public static readonly float Factor = GetFactor();

    /// <summary>Đổi số pixel ở 100% sang pixel thật trên màn hình hiện tại.</summary>
    public static int Px(int value)
    {
        return (int)Math.Round(value * Factor);
    }

    private static float GetFactor()
    {
        try
        {
            using var graphics = Graphics.FromHwnd(IntPtr.Zero);
            return Math.Max(1F, graphics.DpiX / 96F);
        }
        catch (Exception)
        {
            return 1F;
        }
    }
}
