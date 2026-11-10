using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using BobaShop.ClientNetworkLogic.Theme;

namespace BobaShop.ClientNetworkLogic.Controls;

/// <summary>
/// Ô nhập một dòng cao 44px có viền theo Theme: viền xanh khi đang nhập, viền đỏ khi lỗi.
/// Bọc một TextBox không viền để chỉnh được chiều cao và khoảng đệm.
/// </summary>
[DesignerCategory("Code")]
public class TextField : Panel
{
    private readonly TextBox _textBox;
    private bool _hasError;

    public TextField()
    {
        // Tạo TextBox trước khi đổi kích thước: đổi Height sẽ gọi OnLayout ngay trong constructor.
        _textBox = new TextBox
        {
            BorderStyle = BorderStyle.None,
            BackColor = AppColors.Surface,
            ForeColor = AppColors.TextPrimary,
            Font = AppFonts.Body,

            // Tự đặt chiều cao: chiều cao mặc định của TextBox không viền cắt mất dấu tiếng Việt.
            AutoSize = false,
        };
        UpdateInputHeight();

        DoubleBuffered = true;
        ResizeRedraw = true;
        BackColor = AppColors.Surface;
        Height = AppSpacing.InputHeight;
        Cursor = Cursors.IBeam;

        _textBox.TextChanged += (_, e) => OnTextChanged(e);
        _textBox.GotFocus += (_, _) => Invalidate();
        _textBox.LostFocus += (_, _) => Invalidate();
        Controls.Add(_textBox);
    }

    [AllowNull]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public override string Text
    {
        get => _textBox?.Text ?? string.Empty;
        set
        {
            if (_textBox is not null)
            {
                _textBox.Text = value ?? string.Empty;
            }
        }
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string PlaceholderText
    {
        get => _textBox.PlaceholderText;
        set => _textBox.PlaceholderText = value;
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool UseSystemPasswordChar
    {
        get => _textBox.UseSystemPasswordChar;
        set => _textBox.UseSystemPasswordChar = value;
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public int MaxLength
    {
        get => _textBox.MaxLength;
        set => _textBox.MaxLength = value;
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public HorizontalAlignment TextAlign
    {
        get => _textBox.TextAlign;
        set => _textBox.TextAlign = value;
    }

    /// <summary>Font của phần chữ nhập vào.</summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Font InputFont
    {
        get => _textBox.Font;
        set
        {
            _textBox.Font = value;
            UpdateInputHeight();
            PerformLayout();
        }
    }

    /// <summary>Bật thì viền chuyển sang màu lỗi.</summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool HasError
    {
        get => _hasError;
        set
        {
            if (_hasError == value)
            {
                return;
            }

            _hasError = value;
            Invalidate();
        }
    }

    /// <summary>Đưa con trỏ vào ô nhập và bôi đen nội dung hiện có.</summary>
    public void FocusInput()
    {
        _textBox.Focus();
        _textBox.SelectAll();
    }

    private void UpdateInputHeight()
    {
        // Chiều cao một dòng chữ của font, cộng thêm chút đệm cho dấu ở trên và dưới.
        _textBox.Height = (int)Math.Ceiling(_textBox.Font.GetHeight()) + 4;
    }

    protected override void OnLayout(LayoutEventArgs levent)
    {
        base.OnLayout(levent);
        if (_textBox is null)
        {
            // Lớp cha có thể gọi OnLayout trước khi constructor của lớp này chạy xong.
            return;
        }

        var horizontalPadding = AppSpacing.Spacing12 * DeviceDpi / 96;
        _textBox.Width = Math.Max(0, ClientSize.Width - (horizontalPadding * 2));
        _textBox.Location = new Point(horizontalPadding, Math.Max(0, (ClientSize.Height - _textBox.Height) / 2));
    }

    protected override void OnClick(EventArgs e)
    {
        base.OnClick(e);
        _textBox.Focus();
    }

    protected override void OnEnabledChanged(EventArgs e)
    {
        base.OnEnabledChanged(e);
        var background = Enabled ? AppColors.Surface : AppColors.Background;
        BackColor = background;
        _textBox.BackColor = background;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        var borderColor = AppColors.InputBorder;
        if (_hasError)
        {
            borderColor = AppColors.Error;
        }
        else if (_textBox.Focused)
        {
            borderColor = AppColors.Primary;
        }

        using var pen = new Pen(borderColor);
        e.Graphics.DrawRectangle(pen, 0, 0, ClientSize.Width - 1, ClientSize.Height - 1);
    }
}
