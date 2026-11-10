namespace BobaShop.ClientNetworkLogic.Theme;

/// <summary>
/// Bảng màu chung cho CustomerApp và AdminApp.
/// Form không hardcode Color.FromArgb(...), chỉ lấy màu từ đây.
/// </summary>
public static class AppColors
{
    /// <summary>Màu chủ đạo: nút chính, tiêu đề, liên kết.</summary>
    public static readonly Color Primary = Color.FromArgb(38, 99, 59);

    /// <summary>Màu chủ đạo đậm: trạng thái hover / nhấn của nút chính.</summary>
    public static readonly Color PrimaryDark = Color.FromArgb(27, 74, 43);

    /// <summary>Xanh đậm của logo: nền và lớp phủ của mảng ảnh thương hiệu.</summary>
    public static readonly Color BrandDark = Color.FromArgb(18, 46, 24);

    /// <summary>Chữ và icon đặt trên nền Primary hoặc BrandDark.</summary>
    public static readonly Color OnPrimary = Color.White;

    /// <summary>Nền của form.</summary>
    public static readonly Color Background = Color.FromArgb(247, 244, 238);

    /// <summary>Nền của thẻ, panel, ô nhập.</summary>
    public static readonly Color Surface = Color.White;

    /// <summary>Chữ chính.</summary>
    public static readonly Color TextPrimary = Color.FromArgb(43, 33, 24);

    /// <summary>Chữ phụ: mô tả, gợi ý.</summary>
    public static readonly Color TextSecondary = Color.FromArgb(107, 93, 82);

    /// <summary>Thông báo lỗi, viền ô nhập đang lỗi.</summary>
    public static readonly Color Error = Color.FromArgb(179, 40, 45);

    /// <summary>Viền ô nhập ở trạng thái thường.</summary>
    public static readonly Color InputBorder = Color.FromArgb(184, 172, 158);

    /// <summary>Đường kẻ, nền nút bị vô hiệu hóa.</summary>
    public static readonly Color Divider = Color.FromArgb(230, 222, 212);
}
