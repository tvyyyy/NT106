using System.ComponentModel;
using System.Drawing.Text;
using BobaShop.ClientNetworkLogic.Helpers;
using BobaShop.ClientNetworkLogic.Theme;

namespace BobaShop.CustomerApp.Controls;

/// <summary>
/// Chân trang của trang chủ: địa chỉ liên hệ và các nhóm mục thông tin xếp thành cột trên nền xanh.
/// Toàn bộ nội dung là thông tin minh họa cho đồ án, không phải thông tin của một cửa hàng thật,
/// và các mục chỉ để hiển thị, chưa bấm được.
/// </summary>
[DesignerCategory("Code")]
public class PageFooter : Control
{
    /// <summary>Chiều cao chân trang, tính bằng pixel ở 100%.</summary>
    public const int FooterHeight = 320;

    private const int SidePadding = 32;
    private const int TopPadding = 32;
    private const int ColumnGap = 24;
    private const int HeadingGap = 10;
    private const int ItemGap = 8;
    private const int NoteAreaHeight = 48;

    // Phần bề rộng dành cho cột địa chỉ, các cột còn lại chia đều phần còn lại.
    private const float AddressColumnRatio = 0.34F;

    // Độ đậm của đường kẻ trắng phía trên dòng ghi chú, 0 đến 255.
    private const int DividerAlpha = 70;

    private const string Note = "© 2026 BobaShop. Đồ án môn NT106, thông tin trên trang chỉ mang tính minh họa.";

    private static readonly FooterColumn AddressColumn = new(
        "ĐỊA CHỈ",
        new[]
        {
            "Cửa hàng: Khu phố 6, phường Linh Trung, TP. Thủ Đức, TP. Hồ Chí Minh",
            "Giờ mở cửa: 07:00 - 22:00 mỗi ngày",
            "Hotline đặt hàng: 1900 0000",
            "Email: hotro@bobashop.example",
        });

    private static readonly FooterColumn[] LinkColumns =
    {
        new("CÔNG TY", new[] { "Giới thiệu", "Thư viện hình ảnh", "Hình ảnh thực đơn", "Tuyển dụng" }),
        new("HỘI VIÊN", new[] { "Câu hỏi thường gặp", "Điều khoản hội viên", "Điều kiện dùng voucher" }),
        new("CỬA HÀNG", new[] { "Danh sách cửa hàng", "Tin khuyến mãi", "Liên hệ" }),
        new("ĐIỀU KHOẢN", new[] { "Chính sách bảo mật", "Chính sách đặt hàng" }),
    };

    public PageFooter()
    {
        SetStyle(
            ControlStyles.AllPaintingInWmPaint
            | ControlStyles.UserPaint
            | ControlStyles.OptimizedDoubleBuffer
            | ControlStyles.ResizeRedraw,
            true);
        BackColor = AppColors.Primary;
        TabStop = false;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var graphics = e.Graphics;
        graphics.Clear(AppColors.Primary);
        graphics.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;

        var padding = UiScale.Px(SidePadding);
        var gap = UiScale.Px(ColumnGap);
        var contentWidth = Width - (padding * 2);
        if (contentWidth <= 0)
        {
            return;
        }

        using var textBrush = new SolidBrush(AppColors.OnPrimary);

        var addressWidth = (int)(contentWidth * AddressColumnRatio);
        var linkWidth = (contentWidth - addressWidth - (gap * LinkColumns.Length)) / LinkColumns.Length;
        var top = UiScale.Px(TopPadding);

        var x = padding;
        DrawColumn(graphics, textBrush, AddressColumn, x, top, addressWidth);
        x += addressWidth + gap;
        foreach (var column in LinkColumns)
        {
            DrawColumn(graphics, textBrush, column, x, top, linkWidth);
            x += linkWidth + gap;
        }

        // Dòng ghi chú ở đáy, ngăn với phần trên bằng một đường kẻ mờ.
        var noteTop = Height - UiScale.Px(NoteAreaHeight);
        using var divider = new Pen(Color.FromArgb(DividerAlpha, AppColors.OnPrimary));
        graphics.DrawLine(divider, padding, noteTop, Width - padding, noteTop);

        using var noteFormat = new StringFormat
        {
            LineAlignment = StringAlignment.Center,
            Trimming = StringTrimming.EllipsisCharacter,
            FormatFlags = StringFormatFlags.NoWrap,
        };
        graphics.DrawString(
            Note,
            AppFonts.Caption,
            textBrush,
            new RectangleF(padding, noteTop, contentWidth, UiScale.Px(NoteAreaHeight)),
            noteFormat);
    }

    /// <summary>Vẽ một cột: tiêu đề in đậm rồi các dòng bên dưới, dòng dài tự xuống hàng trong bề rộng cột.</summary>
    private static void DrawColumn(Graphics graphics, Brush brush, FooterColumn column, int x, int top, int width)
    {
        if (width <= 0)
        {
            return;
        }

        float y = top;
        var headingSize = graphics.MeasureString(column.Heading, AppFonts.BodyBold, width);
        graphics.DrawString(column.Heading, AppFonts.BodyBold, brush, new RectangleF(x, y, width, headingSize.Height));
        y += headingSize.Height + UiScale.Px(HeadingGap);

        foreach (var item in column.Items)
        {
            var itemSize = graphics.MeasureString(item, AppFonts.Body, width);
            graphics.DrawString(item, AppFonts.Body, brush, new RectangleF(x, y, width, itemSize.Height + 1F));
            y += itemSize.Height + UiScale.Px(ItemGap);
        }
    }

    private sealed record FooterColumn(string Heading, string[] Items);
}
