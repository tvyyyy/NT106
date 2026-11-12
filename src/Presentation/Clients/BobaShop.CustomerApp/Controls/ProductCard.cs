using System.ComponentModel;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using BobaShop.ClientNetworkLogic.Helpers;
using BobaShop.ClientNetworkLogic.Theme;
using BobaShop.CustomerApp.Services;

namespace BobaShop.CustomerApp.Controls;

/// <summary>Thẻ một món trên trang chủ: ảnh ở trên, tên món và giá ở dưới.</summary>
[DesignerCategory("Code")]
public class ProductCard : Control
{
    /// <summary>Chiều cao phần ảnh so với bề rộng thẻ.</summary>
    public const float ImageRatio = 0.62F;

    /// <summary>Chiều cao phần chữ dưới ảnh, tính bằng pixel ở 100%.</summary>
    public const int TextAreaHeight = 96;

    private const int TextPadding = 12;

    private readonly Image? _image;
    private Bitmap? _imageCache;

    public ProductCard(MenuProduct product, Image? image)
    {
        Product = product;
        _image = image;

        SetStyle(
            ControlStyles.AllPaintingInWmPaint
            | ControlStyles.UserPaint
            | ControlStyles.OptimizedDoubleBuffer
            | ControlStyles.ResizeRedraw,
            true);
        BackColor = AppColors.Surface;
        TabStop = false;
    }

    public MenuProduct Product { get; }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);

        // Kích thước đổi thì ảnh thu nhỏ cũ không còn đúng, lần vẽ sau sẽ tạo lại.
        _imageCache?.Dispose();
        _imageCache = null;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var graphics = e.Graphics;
        graphics.Clear(AppColors.Surface);

        if (Width <= 2 || Height <= 2)
        {
            return;
        }

        var imageHeight = (int)(Width * ImageRatio);
        var imageBounds = new Rectangle(0, 0, Width, imageHeight);
        var cached = GetImageCache(imageBounds.Size);
        if (cached is not null)
        {
            // Chỉ rõ khung đích để GDI+ vẽ đúng từng pixel, không tự co giãn theo DPI của ảnh.
            graphics.DrawImage(cached, imageBounds);
        }
        else
        {
            using var placeholder = new SolidBrush(AppColors.Divider);
            graphics.FillRectangle(placeholder, imageBounds);
        }

        graphics.SmoothingMode = SmoothingMode.AntiAlias;
        graphics.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;

        var padding = UiScale.Px(TextPadding);
        var priceHeight = (float)Math.Ceiling(AppFonts.Heading.GetHeight(graphics));
        var priceTop = Height - padding - priceHeight;
        var nameBounds = new RectangleF(
            padding,
            imageHeight + padding,
            Width - (padding * 2),
            Math.Max(0F, priceTop - imageHeight - padding));

        using var nameBrush = new SolidBrush(AppColors.TextPrimary);
        using var priceBrush = new SolidBrush(AppColors.Primary);
        using var nameFormat = new StringFormat
        {
            Trimming = StringTrimming.EllipsisWord,
            FormatFlags = StringFormatFlags.LineLimit,
        };

        graphics.DrawString(Product.ProductName, AppFonts.BodyBold, nameBrush, nameBounds, nameFormat);
        graphics.DrawString(Formatter.Currency(Product.Price), AppFonts.Heading, priceBrush, padding, priceTop);

        using var border = new Pen(AppColors.Divider);
        graphics.DrawRectangle(border, 0, 0, Width - 1, Height - 1);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _imageCache?.Dispose();
            _imageCache = null;
        }

        base.Dispose(disposing);
    }

    /// <summary>Thu nhỏ và cắt ảnh cho vừa khung một lần, các lần vẽ sau dùng lại.</summary>
    private Bitmap? GetImageCache(Size size)
    {
        if (_image is null || size.Width <= 0 || size.Height <= 0 || _image.Width <= 0 || _image.Height <= 0)
        {
            return null;
        }

        if (_imageCache is not null && _imageCache.Size == size)
        {
            return _imageCache;
        }

        _imageCache?.Dispose();
        _imageCache = new Bitmap(size.Width, size.Height);

        // Lấy phần giữa của ảnh gốc sao cho phủ kín khung mà không méo.
        var scale = Math.Max(size.Width / (float)_image.Width, size.Height / (float)_image.Height);
        var sourceWidth = size.Width / scale;
        var sourceHeight = size.Height / scale;
        var source = new RectangleF(
            (_image.Width - sourceWidth) / 2F,
            (_image.Height - sourceHeight) / 2F,
            sourceWidth,
            sourceHeight);

        using var graphics = Graphics.FromImage(_imageCache);
        graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
        graphics.DrawImage(_image, new RectangleF(0F, 0F, size.Width, size.Height), source, GraphicsUnit.Pixel);
        return _imageCache;
    }
}
