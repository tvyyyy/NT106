using System.ComponentModel;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace BobaShop.ClientNetworkLogic.Controls;

/// <summary>
/// Khung ảnh tự chuyển: mỗi ảnh phủ kín khung, phóng to chậm rồi mờ dần sang ảnh kế tiếp.
/// Dùng cho mảng thương hiệu ở màn đăng nhập và có thể dùng lại cho image slider của trang chủ.
/// Lớp con vẽ thêm nội dung phía trên ảnh bằng cách override OnPaintContent.
/// </summary>
[DesignerCategory("Code")]
public class ImageSlideshow : Control
{
    // Một lượt phóng to rồi thu về kéo dài 36 giây.
    private const double ZoomPeriodSeconds = 36.0;

    private readonly List<Image> _images = new();
    private readonly Stopwatch _clock = Stopwatch.StartNew();
    private readonly System.Windows.Forms.Timer _timer;

    public ImageSlideshow()
    {
        SetStyle(
            ControlStyles.AllPaintingInWmPaint
            | ControlStyles.UserPaint
            | ControlStyles.OptimizedDoubleBuffer
            | ControlStyles.ResizeRedraw,
            true);

        _timer = new System.Windows.Forms.Timer { Interval = 40 };
        _timer.Tick += (_, _) => Invalidate();
    }

    /// <summary>Số giây mỗi ảnh hiển thị, đã gồm thời gian chuyển.</summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public double SlideSeconds { get; set; } = 6.0;

    /// <summary>Số giây mờ dần sang ảnh kế tiếp.</summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public double FadeSeconds { get; set; } = 0.8;

    /// <summary>Mức phóng to tối đa, 0.08 nghĩa là to thêm 8%. Đặt 0 để tắt.</summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public float ZoomAmount { get; set; } = 0.08F;

    /// <summary>Lớp màu phủ lên ảnh để chữ phía trên dễ đọc. Độ trong suốt lấy từ kênh alpha.</summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Color OverlayColor { get; set; } = Color.Empty;

    /// <summary>Đặt danh sách ảnh. Control không Dispose các ảnh này.</summary>
    public void SetImages(IEnumerable<Image?> images)
    {
        _images.Clear();
        foreach (var image in images)
        {
            if (image is not null)
            {
                _images.Add(image);
            }
        }

        _clock.Restart();
        Invalidate();
    }

    /// <summary>Vẽ nội dung phía trên ảnh và lớp phủ.</summary>
    protected virtual void OnPaintContent(Graphics graphics)
    {
    }

    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);
        if (!DesignMode)
        {
            _timer.Start();
        }
    }

    protected override void OnHandleDestroyed(EventArgs e)
    {
        _timer.Stop();
        base.OnHandleDestroyed(e);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var graphics = e.Graphics;
        graphics.Clear(BackColor);

        if (ClientSize.Width <= 0 || ClientSize.Height <= 0)
        {
            return;
        }

        if (_images.Count > 0)
        {
            graphics.InterpolationMode = InterpolationMode.Bilinear;

            var elapsed = _clock.Elapsed.TotalSeconds;
            var slide = Math.Max(1.0, SlideSeconds);
            var fade = Math.Clamp(FadeSeconds, 0.0, slide);
            var position = elapsed % (slide * _images.Count);
            var index = Math.Min(_images.Count - 1, (int)(position / slide));
            var local = position - (index * slide);
            var zoom = 1F + (ZoomAmount * (float)(0.5 - (0.5 * Math.Cos(2.0 * Math.PI * elapsed / ZoomPeriodSeconds))));

            DrawCover(graphics, _images[index], zoom, 1F);

            var fadeStart = slide - fade;
            if (_images.Count > 1 && fade > 0.0 && local > fadeStart)
            {
                var alpha = (float)Math.Clamp((local - fadeStart) / fade, 0.0, 1.0);
                DrawCover(graphics, _images[(index + 1) % _images.Count], zoom, alpha);
            }
        }

        if (OverlayColor.A > 0)
        {
            using var overlay = new SolidBrush(OverlayColor);
            graphics.FillRectangle(overlay, ClientRectangle);
        }

        OnPaintContent(graphics);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _timer.Dispose();
        }

        base.Dispose(disposing);
    }

    private void DrawCover(Graphics graphics, Image image, float zoom, float alpha)
    {
        if (image.Width <= 0 || image.Height <= 0)
        {
            return;
        }

        var scale = Math.Max(ClientSize.Width / (float)image.Width, ClientSize.Height / (float)image.Height) * zoom;
        var width = image.Width * scale;
        var height = image.Height * scale;
        var destination = new RectangleF((ClientSize.Width - width) / 2F, (ClientSize.Height - height) / 2F, width, height);

        if (alpha >= 1F)
        {
            graphics.DrawImage(image, destination);
            return;
        }

        using var attributes = new ImageAttributes();
        attributes.SetColorMatrix(new ColorMatrix { Matrix33 = alpha });
        graphics.DrawImage(
            image,
            Rectangle.Round(destination),
            0,
            0,
            image.Width,
            image.Height,
            GraphicsUnit.Pixel,
            attributes);
    }
}
