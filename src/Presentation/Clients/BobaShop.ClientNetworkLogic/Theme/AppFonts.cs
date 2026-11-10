using System.Drawing.Text;
using System.Runtime.InteropServices;

namespace BobaShop.ClientNetworkLogic.Theme;

/// <summary>
/// Font và cỡ chữ chung cho hai client. Các Font ở đây dùng chung suốt vòng đời
/// ứng dụng, không Dispose ở Form.
/// Font thiết kế: "Baloo 2" cho tiêu đề và nút, "Be Vietnam Pro" cho nội dung.
/// Hai font này nằm sẵn trong thư mục Theme/Fonts và được nhúng vào file dll,
/// nên máy chạy ứng dụng không cần cài font. Nạp không được thì tự dùng Segoe UI.
/// </summary>
public static class AppFonts
{
    public const string FallbackFamily = "Segoe UI";
    public const string PreferredDisplayFamily = "Baloo 2";
    public const string PreferredBodyFamily = "Be Vietnam Pro";

    // Cờ FR_PRIVATE của GDI: font chỉ dùng được trong tiến trình này, không cài vào Windows.
    private const uint FontResourcePrivate = 0x10;

    // Phải khai báo trước các Font bên dưới vì field static khởi tạo theo thứ tự trong file.
    private static readonly PrivateFontCollection BundledFonts = LoadBundledFonts();
    private static readonly FontFamily? DisplayFontFamily = FindBundledFamily(PreferredDisplayFamily);
    private static readonly FontFamily? BodyFontFamily = FindBundledFamily(PreferredBodyFamily);

    /// <summary>Khẩu hiệu lớn trên mảng ảnh thương hiệu.</summary>
    public static readonly Font Hero = Create(DisplayFontFamily, PreferredDisplayFamily, 40F, FontStyle.Bold);

    /// <summary>Tên quán cạnh logo.</summary>
    public static readonly Font Brand = Create(DisplayFontFamily, PreferredDisplayFamily, 21F, FontStyle.Bold);

    /// <summary>Tiêu đề màn hình (Đăng nhập, Trang chủ).</summary>
    public static readonly Font Title = Create(DisplayFontFamily, PreferredDisplayFamily, 25F, FontStyle.Bold);

    /// <summary>Tiêu đề khu vực, tên món trên thẻ.</summary>
    public static readonly Font Heading = Create(BodyFontFamily, PreferredBodyFamily, 13F, FontStyle.Bold);

    /// <summary>Đoạn mô tả lớn trên mảng ảnh thương hiệu.</summary>
    public static readonly Font BodyLarge = Create(BodyFontFamily, PreferredBodyFamily, 12.5F, FontStyle.Regular);

    /// <summary>Nội dung thường, ô nhập.</summary>
    public static readonly Font Body = Create(BodyFontFamily, PreferredBodyFamily, 10.5F, FontStyle.Regular);

    /// <summary>Nội dung cần nhấn mạnh, nhãn của ô nhập, liên kết.</summary>
    public static readonly Font BodyBold = Create(BodyFontFamily, PreferredBodyFamily, 10.5F, FontStyle.Bold);

    /// <summary>Chú thích, thông báo lỗi dưới ô nhập.</summary>
    public static readonly Font Caption = Create(BodyFontFamily, PreferredBodyFamily, 9F, FontStyle.Regular);

    /// <summary>Chữ trên nút.</summary>
    public static readonly Font Button = Create(DisplayFontFamily, PreferredDisplayFamily, 14F, FontStyle.Bold);

    [DllImport("gdi32.dll", CharSet = CharSet.Unicode)]
    private static extern int AddFontResourceEx(string fileName, uint flags, IntPtr reserved);

    /// <summary>
    /// Chép các file .ttf nhúng trong dll ra thư mục tạm rồi nạp cho cả GDI+ (Graphics.DrawString)
    /// và GDI (Label, Button, TextBox). Lỗi ở bước nào thì bỏ qua file đó.
    /// </summary>
    private static PrivateFontCollection LoadBundledFonts()
    {
        var collection = new PrivateFontCollection();
        try
        {
            var assembly = typeof(AppFonts).Assembly;
            var folder = Path.Combine(Path.GetTempPath(), "BobaShop", "Fonts");
            Directory.CreateDirectory(folder);

            foreach (var resourceName in assembly.GetManifestResourceNames())
            {
                if (!resourceName.EndsWith(".ttf", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                try
                {
                    using var stream = assembly.GetManifestResourceStream(resourceName);
                    if (stream is null)
                    {
                        continue;
                    }

                    // Tên resource có dạng "BobaShop.ClientNetworkLogic.Theme.Fonts.Baloo2-Bold.ttf".
                    var parts = resourceName.Split('.');
                    var fileName = parts.Length >= 2 ? $"{parts[^2]}.{parts[^1]}" : resourceName;
                    var path = Path.Combine(folder, fileName);

                    if (!File.Exists(path) || new FileInfo(path).Length != stream.Length)
                    {
                        using var file = File.Create(path);
                        stream.CopyTo(file);
                    }

                    collection.AddFontFile(path);
                    AddFontResourceEx(path, FontResourcePrivate, IntPtr.Zero);
                }
                catch (Exception)
                {
                    // Bỏ qua file font hỏng hoặc không ghi được, các file khác vẫn nạp tiếp.
                }
            }
        }
        catch (Exception)
        {
            // Không nạp được font đi kèm thì dùng font đã cài hoặc font mặc định.
        }

        return collection;
    }

    private static FontFamily? FindBundledFamily(string familyName)
    {
        try
        {
            foreach (var family in BundledFonts.Families)
            {
                if (string.Equals(family.Name, familyName, StringComparison.OrdinalIgnoreCase))
                {
                    return family;
                }
            }
        }
        catch (Exception)
        {
            // Coi như không có font đi kèm.
        }

        return null;
    }

    private static Font Create(FontFamily? bundledFamily, string preferredFamily, float size, FontStyle style)
    {
        // Ưu tiên font đi kèm, kế đến font cùng tên đã cài trong Windows, cuối cùng là Segoe UI.
        if (bundledFamily is not null)
        {
            try
            {
                if (bundledFamily.IsStyleAvailable(style))
                {
                    return new Font(bundledFamily, size, style);
                }
            }
            catch (ArgumentException)
            {
                // Thử cách tiếp theo.
            }
        }

        try
        {
            var font = new Font(preferredFamily, size, style);
            if (string.Equals(font.Name, preferredFamily, StringComparison.OrdinalIgnoreCase))
            {
                return font;
            }

            font.Dispose();
        }
        catch (ArgumentException)
        {
            // Thử cách tiếp theo.
        }

        return new Font(FallbackFamily, size, style);
    }
}
