using System.ComponentModel;
using BobaShop.ClientNetworkLogic.Controls;
using BobaShop.ClientNetworkLogic.Theme;
using BobaShop.CustomerApp.Controls;

namespace BobaShop.CustomerApp.Forms.Auth;

/// <summary>
/// Khung chung của các màn xác thực: cửa sổ 1200 x 700 chia 70/30,
/// bên trái là mảng thương hiệu có ảnh tự chuyển, bên phải là cột form.
/// Giao diện dựng bằng code (không có file Designer) để ba màn dùng chung một bố cục.
/// Mọi kích thước viết theo pixel ở 100% rồi nhân với tỉ lệ màn hình qua hàm Px,
/// vì chữ (tính bằng point) tự to lên theo tỉ lệ còn kích thước control thì không.
/// </summary>
[DesignerCategory("Code")]
public class AuthFormBase : Form
{
    private const int WindowWidth = 1200;
    private const int WindowHeight = 700;
    private const int BrandWidth = 840;
    private const int ContentWidthAt100 = 296;

    // Chữ trong Label lùi vào so với mép control. Hai số này kéo nhãn ra cho thẳng mép ô nhập.
    private const int TitleTextInset = 10;
    private const int LabelTextInset = 4;

    // Phải khai báo trước ContentWidth vì field static khởi tạo theo thứ tự trong file.
    private static readonly float UiScale = GetUiScale();

    /// <summary>Bề rộng cột nội dung bên phải, đã nhân tỉ lệ màn hình.</summary>
    protected static readonly int ContentWidth = Px(ContentWidthAt100);

    private readonly Panel _pnlRight;
    private readonly FlowLayoutPanel _flpStack;

    protected AuthFormBase()
    {
        SuspendLayout();

        AutoScaleMode = AutoScaleMode.None;
        ClientSize = new Size(Px(WindowWidth), Px(WindowHeight));
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = AppColors.Background;
        ForeColor = AppColors.TextPrimary;
        Font = AppFonts.Body;

        var brandPanel = new AuthBrandPanel
        {
            Dock = DockStyle.Left,
            Width = Px(BrandWidth),
            TabStop = false,
        };

        _pnlRight = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = AppColors.Background,
        };

        _flpStack = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            BackColor = AppColors.Background,
            Margin = Padding.Empty,
            Padding = new Padding(Px(TitleTextInset), 0, 0, 0),
        };

        _pnlRight.Controls.Add(_flpStack);
        _pnlRight.Resize += (_, _) => CenterStack();
        _flpStack.SizeChanged += (_, _) => CenterStack();

        // Thêm panel Fill trước, panel Left sau để mảng thương hiệu được dock trước.
        Controls.Add(_pnlRight);
        Controls.Add(brandPanel);

        ResumeLayout(false);
    }

    /// <summary>Cột chứa nội dung form, xếp từ trên xuống và tự căn giữa theo chiều dọc.</summary>
    protected FlowLayoutPanel Stack => _flpStack;

    /// <summary>Đổi số pixel ở 100% sang pixel thật theo tỉ lệ màn hình (125%, 150%...).</summary>
    protected static int Px(int value)
    {
        return (int)Math.Round(value * UiScale);
    }

    /// <summary>Thêm tiêu đề và dòng mô tả. Trả về nhãn mô tả để màn hình có thể đổi nội dung.</summary>
    protected Label AddTitle(string title, string subtitle)
    {
        var lblTitle = new Label
        {
            AutoSize = true,
            MaximumSize = new Size(ContentWidth + Px(TitleTextInset), 0),
            Font = AppFonts.Title,
            ForeColor = AppColors.TextPrimary,
            Text = title,
            Margin = new Padding(-Px(TitleTextInset), 0, 0, 0),
            UseMnemonic = false,
        };

        var lblSubtitle = new Label
        {
            AutoSize = true,
            MaximumSize = new Size(ContentWidth + Px(LabelTextInset), 0),
            Font = AppFonts.Body,
            ForeColor = AppColors.TextSecondary,
            Text = subtitle,
            Margin = new Padding(-Px(LabelTextInset), 0, 0, Px(AppSpacing.Spacing16)),
            UseMnemonic = false,
        };

        _flpStack.Controls.Add(lblTitle);
        _flpStack.Controls.Add(lblSubtitle);
        return lblSubtitle;
    }

    /// <summary>Thêm một ô nhập gồm nhãn, ô nhập và dòng báo lỗi bên dưới.</summary>
    protected TextField AddField(string label, string placeholder, bool isPassword, out Label lblError)
    {
        var lblCaption = new Label
        {
            AutoSize = true,
            Font = AppFonts.BodyBold,
            ForeColor = AppColors.TextPrimary,
            Text = label,
            Margin = new Padding(-Px(LabelTextInset), 0, 0, Px(AppSpacing.Spacing4)),
            UseMnemonic = false,
        };

        var field = new TextField
        {
            Size = new Size(ContentWidth, Px(AppSpacing.InputHeight)),
            PlaceholderText = placeholder,
            UseSystemPasswordChar = isPassword,
            Margin = new Padding(0, 0, 0, Px(2)),
        };

        lblError = new Label
        {
            AutoSize = false,
            Size = new Size(ContentWidth + Px(LabelTextInset), Px(20)),
            Font = AppFonts.Caption,
            ForeColor = AppColors.Error,
            Margin = new Padding(-Px(LabelTextInset), 0, 0, Px(AppSpacing.Spacing4)),
            UseMnemonic = false,
        };

        _flpStack.Controls.Add(lblCaption);
        _flpStack.Controls.Add(field);
        _flpStack.Controls.Add(lblError);
        return field;
    }

    /// <summary>Thêm dòng thông báo lỗi chung, nằm ngay trên nút chính.</summary>
    protected Label AddMessage()
    {
        var lblMessage = new Label
        {
            AutoSize = true,
            MinimumSize = new Size(ContentWidth + Px(LabelTextInset), Px(20)),
            MaximumSize = new Size(ContentWidth + Px(LabelTextInset), 0),
            Font = AppFonts.Caption,
            ForeColor = AppColors.Error,
            Margin = new Padding(-Px(LabelTextInset), 0, 0, Px(AppSpacing.Spacing4)),
            UseMnemonic = false,
        };

        _flpStack.Controls.Add(lblMessage);
        return lblMessage;
    }

    protected PrimaryButton AddButton(string text)
    {
        var button = new PrimaryButton
        {
            Size = new Size(ContentWidth, Px(AppSpacing.ButtonHeight)),
            Text = text,
            Margin = new Padding(0, Px(AppSpacing.Spacing4), 0, Px(AppSpacing.Spacing12)),
        };

        _flpStack.Controls.Add(button);
        return button;
    }

    /// <summary>Thêm một dòng căn giữa gồm chữ thường (có thể để trống) và một liên kết.</summary>
    protected LinkLabel AddLinkRow(string prefix, string linkText)
    {
        var flpRow = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Anchor = AnchorStyles.None,
            Margin = new Padding(0, 0, 0, Px(AppSpacing.Spacing4)),
        };

        if (!string.IsNullOrEmpty(prefix))
        {
            flpRow.Controls.Add(new Label
            {
                AutoSize = true,
                Font = AppFonts.Body,
                ForeColor = AppColors.TextSecondary,
                Text = prefix,
                Margin = new Padding(0),
                UseMnemonic = false,
            });
        }

        var link = CreateLink(linkText);
        flpRow.Controls.Add(link);
        _flpStack.Controls.Add(flpRow);
        return link;
    }

    protected static LinkLabel CreateLink(string text)
    {
        return new LinkLabel
        {
            AutoSize = true,
            Font = AppFonts.BodyBold,
            Text = text,
            LinkColor = AppColors.Primary,
            ActiveLinkColor = AppColors.PrimaryDark,
            VisitedLinkColor = AppColors.Primary,
            DisabledLinkColor = AppColors.TextSecondary,
            LinkBehavior = LinkBehavior.HoverUnderline,
            Margin = new Padding(0),
            TabStop = true,
            UseMnemonic = false,
        };
    }

    /// <summary>Ghi lỗi vào dòng dưới ô nhập và đổi màu viền ô. Truyền null để xóa lỗi.</summary>
    protected static void SetFieldError(TextField field, Label lblError, string? error)
    {
        lblError.Text = error ?? string.Empty;
        field.HasError = error is not null;
    }

    private static float GetUiScale()
    {
        try
        {
            using var graphics = Graphics.FromHwnd(IntPtr.Zero);
            return Math.Max(1F, graphics.DpiX / 96F);
        }
        catch (Exception)
        {
            return 1F;
        }
    }

    private void CenterStack()
    {
        var x = Math.Max(0, (_pnlRight.ClientSize.Width - _flpStack.Width) / 2);
        var y = Math.Max(0, (_pnlRight.ClientSize.Height - _flpStack.Height) / 2);
        _flpStack.Location = new Point(x, y);
    }
}
