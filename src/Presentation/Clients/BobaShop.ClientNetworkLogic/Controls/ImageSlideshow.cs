using System.ComponentModel;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using BobaShop.ClientNetworkLogic.Helpers;
using BobaShop.ClientNetworkLogic.Theme;

namespace BobaShop.ClientNetworkLogic.Controls;

/// <summary>
/// Khung ảnh tự chuyển: mỗi ảnh phủ kín khung, phóng to chậm rồi mờ dần sang ảnh kế tiếp.
/// Dùng cho mảng thương hiệu ở màn đăng nhập và image slider của trang chủ.
/// Bật ShowNavigation để có mũi tên trái/phải và các chấm chọn ảnh.
/// Lớp con vẽ thêm nội dung phía trên ảnh bằng cách override OnPaintContent.
/// </summary>
[DesignerCategory("Code")]
public class ImageSlideshow : Control
{
    // Một lượt phóng to rồi thu về kéo dài 36 giây.
    private const double ZoomPeriodSeconds = 36.0;

    // Kích thước phần điều hướng, tính bằng pixel ở 100%.
    private const int ArrowDiameter = 40;
    private const int ArrowMargin = 16;
    private const int DotDiameter = 6;
    private const int DotGap = 8;
    private const int DotBottomMargin = 14;

    // Độ đậm của nền mũi tên và của chấm chưa chọn, 0 đến 255.
    private const int ArrowBackgroundAlpha = 150;
    private const int ArrowHoverAlpha = 210;
    private const int InactiveDotAlpha = 200;

    private readonly List<Image> _images = new();
    private readonly Stopwatch _clock = Stopwatch.StartNew();
    private readonly System.Windows.Forms.Timer _timer;

    private int _currentIndex;

    // Ảnh đang mờ dần hiện lên, -1 nếu không trong lúc chuyển.
    private int _nextIndex = -1;
    private double _slideStartedAt;
    private double _fadeStartedAt;
    private bool _showNavigation;
    private NavigationTarget _hoverTarget = NavigationTarget.None;

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

    private enum NavigationTarget
    {
        None,
        Previous,
        Next,
        Dot,
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

    /// <summary>Hiện mũi tên trái/phải và các chấm để người dùng tự chuyển ảnh.</summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool ShowNavigation
    {
        get => _showNavigation;
        set
        {
            _showNavigation = value;
            Invalidate();
        }
    }

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

        _currentIndex = 0;
        _nextIndex = -1;
        _slideStartedAt = _clock.Elapsed.TotalSeconds;
        Invalidate();
    }

    /// <summary>Chuyển sang ảnh kế tiếp, hết thì quay về ảnh đầu.</summary>
    public void ShowNext()
    {
        ShowSlide(TargetIndex + 1);
    }

    /// <summary>Chuyển về ảnh trước, đang ở ảnh đầu thì sang ảnh cuối.</summary>
    public void ShowPrevious()
    {
        ShowSlide(TargetIndex - 1);
    }

    /// <summary>Chuyển tới ảnh theo vị trí, có hiệu ứng mờ dần.</summary>
    public void ShowSlide(int index)
    {
        if (_images.Count == 0)
        {
            return;
        }

        var target = ((index % _images.Count) + _images.Count) % _images.Count;
        if (target == TargetIndex)
        {
            return;
        }

        if (_nextIndex >= 0)
        {
            // Đang chuyển dở thì coi như đã tới ảnh đích rồi mới chuyển tiếp.
            _currentIndex = _nextIndex;
        }

        _nextIndex = target;
        _fadeStartedAt = _clock.Elapsed.TotalSeconds;
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

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);

        var target = HitTest(e.Location, out _);
        if (target != _hoverTarget)
        {
            _hoverTarget = target;
            Cursor = target == NavigationTarget.None ? Cursors.Default : Cursors.Hand;
            Invalidate();
        }
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        if (_hoverTarget != NavigationTarget.None)
        {
            _hoverTarget = NavigationTarget.None;
            Cursor = Cursors.Default;
            Invalidate();
        }
    }

    protected override void OnMouseClick(MouseEventArgs e)
    {
        base.OnMouseClick(e);
        if (e.Button != MouseButtons.Left)
        {
            return;
        }

        switch (HitTest(e.Location, out var dotIndex))
        {
            case NavigationTarget.Previous:
                ShowPrevious();
                break;
            case NavigationTarget.Next:
                ShowNext();
                break;
            case NavigationTarget.Dot:
                ShowSlide(dotIndex);
                break;
        }
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

            var now = _clock.Elapsed.TotalSeconds;
            var fadeAlpha = AdvanceSlides(now);
            var zoom = 1F + (ZoomAmount * (float)(0.5 - (0.5 * Math.Cos(2.0 * Math.PI * now / ZoomPeriodSeconds))));

            DrawCover(graphics, _images[_currentIndex], zoom, 1F);
            if (_nextIndex >= 0)
            {
                DrawCover(graphics, _images[_nextIndex], zoom, fadeAlpha);
            }
        }

        if (OverlayColor.A > 0)
        {
            using var overlay = new SolidBrush(OverlayColor);
            graphics.FillRectangle(overlay, ClientRectangle);
        }

        OnPaintContent(graphics);

        if (_showNavigation && _images.Count > 1)
        {
            DrawNavigation(graphics);
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _timer.Dispose();
        }

        base.Dispose(disposing);
    }

    /// <summary>Ảnh mà khung đang hướng tới: ảnh đang hiện lên nếu đang chuyển, không thì ảnh hiện tại.</summary>
    private int TargetIndex => _nextIndex >= 0 ? _nextIndex : _currentIndex;

    /// <summary>
    /// Cập nhật trạng thái theo thời gian: tới hạn thì bắt đầu mờ sang ảnh kế, mờ xong thì chốt ảnh mới.
    /// Trả về độ rõ (0 đến 1) của ảnh đang hiện lên.
    /// </summary>
    private float AdvanceSlides(double now)
    {
        var slide = Math.Max(1.0, SlideSeconds);
        var fade = Math.Clamp(FadeSeconds, 0.0, slide);

        if (_currentIndex >= _images.Count)
        {
            _currentIndex = 0;
        }

        if (_nextIndex >= _images.Count)
        {
            _nextIndex = -1;
        }

        if (_nextIndex < 0 && _images.Count > 1 && now - _slideStartedAt >= slide - fade)
        {
            _nextIndex = (_currentIndex + 1) % _images.Count;
            _fadeStartedAt = now;
        }

        if (_nextIndex < 0)
        {
            return 0F;
        }

        var alpha = fade <= 0.0 ? 1.0 : (now - _fadeStartedAt) / fade;
        if (alpha >= 1.0)
        {
            _currentIndex = _nextIndex;
            _nextIndex = -1;
            _slideStartedAt = now;
            return 0F;
        }

        return (float)Math.Max(0.0, alpha);
    }

    private Rectangle GetArrowBounds(bool isNext)
    {
        var diameter = UiScale.Px(ArrowDiameter);
        var margin = UiScale.Px(ArrowMargin);
        var x = isNext ? ClientSize.Width - margin - diameter : margin;
        return new Rectangle(x, (ClientSize.Height - diameter) / 2, diameter, diameter);
    }

    private Rectangle GetDotBounds(int index)
    {
        var diameter = UiScale.Px(DotDiameter);
        var gap = UiScale.Px(DotGap);
        var totalWidth = (_images.Count * diameter) + ((_images.Count - 1) * gap);
        var x = ((ClientSize.Width - totalWidth) / 2) + (index * (diameter + gap));
        var y = ClientSize.Height - UiScale.Px(DotBottomMargin) - diameter;
        return new Rectangle(x, y, diameter, diameter);
    }

    private NavigationTarget HitTest(Point point, out int dotIndex)
    {
        dotIndex = -1;
        if (!_showNavigation || _images.Count <= 1)
        {
            return NavigationTarget.None;
        }

        if (GetArrowBounds(false).Contains(point))
        {
            return NavigationTarget.Previous;
        }

        if (GetArrowBounds(true).Contains(point))
        {
            return NavigationTarget.Next;
        }

        // Nới vùng bấm của chấm ra cho dễ trúng.
        var slack = UiScale.Px(DotGap) / 2;
        for (var i = 0; i < _images.Count; i++)
        {
            var bounds = GetDotBounds(i);
            bounds.Inflate(slack, slack);
            if (bounds.Contains(point))
            {
                dotIndex = i;
                return NavigationTarget.Dot;
            }
        }

        return NavigationTarget.None;
    }

    private void DrawNavigation(Graphics graphics)
    {
        graphics.SmoothingMode = SmoothingMode.AntiAlias;

        DrawArrow(graphics, GetArrowBounds(false), false, _hoverTarget == NavigationTarget.Previous);
        DrawArrow(graphics, GetArrowBounds(true), true, _hoverTarget == NavigationTarget.Next);

        // Chấm của ảnh đang xem tô xanh đậm, các chấm còn lại tô trắng mờ, để nhìn là phân biệt được.
        using var activeBrush = new SolidBrush(AppColors.Primary);
        using var inactiveBrush = new SolidBrush(Color.FromArgb(InactiveDotAlpha, AppColors.OnPrimary));
        using var outline = new Pen(AppColors.OnPrimary);
        var activeIndex = TargetIndex;
        for (var i = 0; i < _images.Count; i++)
        {
            var bounds = GetDotBounds(i);
            graphics.FillEllipse(i == activeIndex ? activeBrush : inactiveBrush, bounds);
            graphics.DrawEllipse(outline, bounds);
        }
    }

    private static void DrawArrow(Graphics graphics, Rectangle bounds, bool isNext, bool isHovered)
    {
        var alpha = isHovered ? ArrowHoverAlpha : ArrowBackgroundAlpha;
        using var background = new SolidBrush(Color.FromArgb(alpha, AppColors.BrandDark));
        graphics.FillEllipse(background, bounds);

        // Mũi tên là hai nét gấp khúc hình chữ V nằm ngang.
        var centerX = bounds.Left + (bounds.Width / 2F);
        var centerY = bounds.Top + (bounds.Height / 2F);
        var size = bounds.Width * 0.16F;
        var direction = isNext ? 1F : -1F;

        using var pen = new Pen(AppColors.OnPrimary, Math.Max(2F, bounds.Width / 16F))
        {
            StartCap = LineCap.Round,
            EndCap = LineCap.Round,
            LineJoin = LineJoin.Round,
        };
        graphics.DrawLines(pen, new[]
        {
            new PointF(centerX - (direction * size * 0.5F), centerY - size),
            new PointF(centerX + (direction * size * 0.5F), centerY),
            new PointF(centerX - (direction * size * 0.5F), centerY + size),
        });
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
