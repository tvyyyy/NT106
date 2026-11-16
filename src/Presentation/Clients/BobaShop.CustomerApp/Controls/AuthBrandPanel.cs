using System.ComponentModel;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using BobaShop.ClientNetworkLogic.Controls;
using BobaShop.ClientNetworkLogic.Theme;
using System.IO;

namespace BobaShop.CustomerApp.Controls;

[DesignerCategory("Code")]
public class AuthBrandPanel : ImageSlideshow
{
    private const string BrandName = "BobaShop";
    private const string Tagline = "Trà sữa pha theo ý bạn";
    private const string Subtitle = "Chọn mức đường, mức đá, topping và theo dõi đơn hàng ngay trên máy.";

    private const string BrandCupsResourceName = "brand_cups";
    private const int OverlayAlpha = 148;
    private const int LogoSize = 52;
    private const int LogoRadius = 10;
    private const int LogoTextGap = 14;

    private readonly Image? _logo;

    public AuthBrandPanel()
    {
        BackColor = AppColors.BrandDark;
        OverlayColor = Color.FromArgb(OverlayAlpha, AppColors.BrandDark);

        // Đã thêm dấu ; ở cuối
        _logo = Image.FromFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "download1.jpg"));

        // Đã thay Properties.Resources.download1 bằng Image.FromFile để hết lỗi
        SetImages(new[]
        {
            Image.FromFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "download1.jpg")),
            Properties.Resources.Cach_pha_tra_sua_hoa_nhai_ngon_la_mieng,
            Properties.Resources.ResourceManager.GetObject(BrandCupsResourceName) as Image,
        });
    }

    protected override void OnPaintContent(Graphics graphics)
    {
        graphics.SmoothingMode = SmoothingMode.AntiAlias;
        graphics.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;

        var scale = DeviceDpi / 96F;
        var padding = AppSpacing.Spacing48 * scale;
        using var textBrush = new SolidBrush(AppColors.OnPrimary);

        var logoBounds = new RectangleF(padding, padding, LogoSize * scale, LogoSize * scale);
        if (_logo is not null)
        {
            using var clip = CreateRoundedRectangle(logoBounds, LogoRadius * scale);
            var state = graphics.Save();
            graphics.SetClip(clip);
            graphics.DrawImage(_logo, logoBounds);
            graphics.Restore(state);
        }

        var brandSize = graphics.MeasureString(BrandName, AppFonts.Brand);
        graphics.DrawString(
            BrandName,
            AppFonts.Brand,
            textBrush,
            logoBounds.Right + (LogoTextGap * scale),
            logoBounds.Top + ((logoBounds.Height - brandSize.Height) / 2F));

        var maxWidth = ClientSize.Width - (padding * 2F);
        if (maxWidth <= 0F)
        {
            return;
        }

        var layoutWidth = (int)maxWidth;
        var subtitleSize = graphics.MeasureString(Subtitle, AppFonts.BodyLarge, layoutWidth);
        var taglineSize = graphics.MeasureString(Tagline, AppFonts.Hero, layoutWidth);

        var subtitleTop = ClientSize.Height - padding - subtitleSize.Height;
        var taglineTop = subtitleTop - (taglineSize.Height * 0.88F);

        graphics.DrawString(
            Tagline,
            AppFonts.Hero,
            textBrush,
            new RectangleF(padding, taglineTop, maxWidth, taglineSize.Height + 1F));
        graphics.DrawString(
            Subtitle,
            AppFonts.BodyLarge,
            textBrush,
            new RectangleF(padding, subtitleTop, maxWidth, subtitleSize.Height + 1F));
    }

    private static GraphicsPath CreateRoundedRectangle(RectangleF bounds, float radius)
    {
        var diameter = radius * 2F;
        var path = new GraphicsPath();
        path.AddArc(bounds.Left, bounds.Top, diameter, diameter, 180F, 90F);
        path.AddArc(bounds.Right - diameter, bounds.Top, diameter, diameter, 270F, 90F);
        path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0F, 90F);
        path.AddArc(bounds.Left, bounds.Bottom - diameter, diameter, diameter, 90F, 90F);
        path.CloseFigure();
        return path;
    }
}