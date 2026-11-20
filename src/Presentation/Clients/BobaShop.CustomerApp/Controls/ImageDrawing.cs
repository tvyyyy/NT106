using System.Drawing.Drawing2D;

namespace BobaShop.CustomerApp.Controls;

/// <summary>Vẽ ảnh phủ kín một khung bo góc, cắt phần thừa ở giữa ảnh để không bị méo.</summary>
public static class ImageDrawing
{
    public static void DrawCover(Graphics graphics, Image? image, Rectangle bounds, int radius, Color placeholder)
    {
        if (bounds.Width <= 0 || bounds.Height <= 0)
        {
            return;
        }

        using var path = RoundedRectangle(bounds, radius);
        var oldClip = graphics.Clip;
        graphics.SetClip(path, CombineMode.Intersect);
        try
        {
            if (image is null || image.Width <= 0 || image.Height <= 0)
            {
                using var brush = new SolidBrush(placeholder);
                graphics.FillRectangle(brush, bounds);
                return;
            }

            var scale = Math.Max(bounds.Width / (float)image.Width, bounds.Height / (float)image.Height);
            var sourceWidth = bounds.Width / scale;
            var sourceHeight = bounds.Height / scale;
            var source = new RectangleF(
                (image.Width - sourceWidth) / 2F,
                (image.Height - sourceHeight) / 2F,
                sourceWidth,
                sourceHeight);

            var oldMode = graphics.InterpolationMode;
            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            graphics.DrawImage(image, bounds, source, GraphicsUnit.Pixel);
            graphics.InterpolationMode = oldMode;
        }
        finally
        {
            graphics.Clip = oldClip;
            oldClip.Dispose();
        }
    }

    public static GraphicsPath RoundedRectangle(Rectangle bounds, int radius)
    {
        var path = new GraphicsPath();
        var diameter = Math.Min(radius * 2, Math.Min(bounds.Width, bounds.Height));
        if (diameter <= 0)
        {
            path.AddRectangle(bounds);
            return path;
        }

        path.AddArc(bounds.Left, bounds.Top, diameter, diameter, 180, 90);
        path.AddArc(bounds.Right - diameter, bounds.Top, diameter, diameter, 270, 90);
        path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0, 90);
        path.AddArc(bounds.Left, bounds.Bottom - diameter, diameter, diameter, 90, 90);
        path.CloseFigure();
        return path;
    }
}
