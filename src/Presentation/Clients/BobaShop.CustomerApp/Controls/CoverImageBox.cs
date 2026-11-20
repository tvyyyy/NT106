using System.ComponentModel;
using System.Drawing.Drawing2D;
using BobaShop.ClientNetworkLogic.Helpers;
using BobaShop.ClientNetworkLogic.Theme;

namespace BobaShop.CustomerApp.Controls;

/// <summary>Khung ảnh bo góc, ảnh phủ kín khung. Dùng cho ảnh lớn ở màn chi tiết món.</summary>
[DesignerCategory("Code")]
public class CoverImageBox : Control
{
    private Image? _image;

    public CoverImageBox()
    {
        SetStyle(
            ControlStyles.AllPaintingInWmPaint
            | ControlStyles.UserPaint
            | ControlStyles.OptimizedDoubleBuffer
            | ControlStyles.ResizeRedraw,
            true);
        BackColor = AppColors.Background;
        TabStop = false;
    }

    /// <summary>Ảnh hiển thị. Khung không giữ quyền sở hữu ảnh nên không Dispose ảnh này.</summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Image? Picture
    {
        get => _image;
        set
        {
            _image = value;
            Invalidate();
        }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.Clear(BackColor);
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        ImageDrawing.DrawCover(
            e.Graphics,
            _image,
            new Rectangle(0, 0, Width, Height),
            UiScale.Px(AppSpacing.Radius12),
            AppColors.Divider);
    }
}
