using System.ComponentModel;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using BobaShop.ClientNetworkLogic.Controls;
using BobaShop.ClientNetworkLogic.Helpers;
using BobaShop.ClientNetworkLogic.Theme;
using BobaShop.CustomerApp.Services;

namespace BobaShop.CustomerApp.Forms.Profile;

/// <summary>
/// Hồ sơ khách hàng, mở khi bấm vào tên trên thanh đầu trang. Chỉ hiển thị những gì Web API
/// hiện trả về: họ tên, email và loại tài khoản.
/// </summary>
[DesignerCategory("Code")]
public class ProfileForm : Form
{
    private const int WindowWidth = 420;
    private const int WindowHeight = 400;
    private const int SidePadding = 40;
    private const int AvatarSize = 88;
    private const int RowHeight = 44;
    private const int RowCount = 2;
    private const string NotUpdatedText = "Chưa cập nhật";

    private readonly IProfileService _profileService;

    private readonly Panel _pnlAvatar;
    private readonly Label _lblName;
    private readonly Label _lblStatus;
    private readonly Panel _pnlRows;
    private readonly Label _lblEmail;
    private readonly Label _lblRole;
    private readonly PrimaryButton _btnClose;

    private string _initials = string.Empty;

    public ProfileForm(IProfileService profileService, string? displayName)
    {
        _profileService = profileService;

        SuspendLayout();

        Text = "BobaShop - Hồ sơ của tôi";
        AutoScaleMode = AutoScaleMode.None;
        ClientSize = new Size(UiScale.Px(WindowWidth), UiScale.Px(WindowHeight));
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.CenterParent;
        BackColor = AppColors.Background;
        ForeColor = AppColors.TextPrimary;
        Font = AppFonts.Body;

        var contentWidth = ClientSize.Width - (UiScale.Px(SidePadding) * 2);
        var left = UiScale.Px(SidePadding);
        var y = UiScale.Px(32);

        // Ảnh đại diện là hình tròn mang chữ cái đầu của tên.
        _pnlAvatar = new Panel
        {
            Size = new Size(UiScale.Px(AvatarSize), UiScale.Px(AvatarSize)),
            Location = new Point((ClientSize.Width - UiScale.Px(AvatarSize)) / 2, y),
            BackColor = AppColors.Background,
        };
        _pnlAvatar.Paint += PnlAvatar_Paint;
        y += UiScale.Px(AvatarSize) + UiScale.Px(12);

        _lblName = new Label
        {
            AutoSize = false,
            Size = new Size(contentWidth, AppFonts.Title.Height + UiScale.Px(4)),
            Location = new Point(left, y),
            Font = AppFonts.Title,
            ForeColor = AppColors.TextPrimary,
            TextAlign = ContentAlignment.MiddleCenter,
            AutoEllipsis = true,
            UseMnemonic = false,
        };
        y += _lblName.Height + UiScale.Px(16);

        _lblStatus = new Label
        {
            AutoSize = false,
            Size = new Size(contentWidth, UiScale.Px(24)),
            Location = new Point(left, y),
            ForeColor = AppColors.TextSecondary,
            TextAlign = ContentAlignment.MiddleCenter,
            UseMnemonic = false,
        };

        // Bảng thông tin: nhãn bên trái, giá trị bên phải, mỗi dòng có đường kẻ mảnh bên dưới.
        _pnlRows = new Panel
        {
            Size = new Size(contentWidth, UiScale.Px(RowHeight) * RowCount),
            Location = new Point(left, y),
            BackColor = AppColors.Background,
            Visible = false,
        };
        _pnlRows.Paint += PnlRows_Paint;
        _lblEmail = AddRow(0, "Email");
        _lblRole = AddRow(1, "Loại tài khoản");

        _btnClose = new PrimaryButton
        {
            Size = new Size(contentWidth, UiScale.Px(AppSpacing.ButtonHeight)),
            Location = new Point(left, ClientSize.Height - UiScale.Px(32) - UiScale.Px(AppSpacing.ButtonHeight)),
            Text = "Đóng",
            DialogResult = DialogResult.OK,
        };

        Controls.Add(_pnlAvatar);
        Controls.Add(_lblName);
        Controls.Add(_lblStatus);
        Controls.Add(_pnlRows);
        Controls.Add(_btnClose);

        AcceptButton = _btnClose;
        CancelButton = _btnClose;

        ResumeLayout(false);

        // Hiện tên từ phiên đăng nhập ngay, phần còn lại điền sau khi tải xong.
        SetName(displayName);
        Shown += ProfileForm_Shown;
    }

    /// <summary>Đổi tên vai trò server trả về sang chữ hiển thị cho người dùng.</summary>
    private static string GetRoleDisplayName(string? role)
    {
        return role switch
        {
            "Customer" => "Khách hàng",
            "Admin" => "Quản lý",
            _ => string.IsNullOrWhiteSpace(role) ? NotUpdatedText : role,
        };
    }

    private Label AddRow(int index, string caption)
    {
        var rowHeight = UiScale.Px(RowHeight);
        var top = index * rowHeight;
        var captionWidth = UiScale.Px(130);

        var lblCaption = new Label
        {
            AutoSize = false,
            Size = new Size(captionWidth, rowHeight - 1),
            Location = new Point(0, top),
            ForeColor = AppColors.TextSecondary,
            Text = caption,
            TextAlign = ContentAlignment.MiddleLeft,
            UseMnemonic = false,
        };

        var lblValue = new Label
        {
            AutoSize = false,
            Size = new Size(_pnlRows.Width - captionWidth, rowHeight - 1),
            Location = new Point(captionWidth, top),
            Font = AppFonts.BodyBold,
            ForeColor = AppColors.TextPrimary,
            TextAlign = ContentAlignment.MiddleRight,
            AutoEllipsis = true,
            UseMnemonic = false,
        };

        _pnlRows.Controls.Add(lblCaption);
        _pnlRows.Controls.Add(lblValue);
        return lblValue;
    }

    private void SetName(string? fullName)
    {
        _lblName.Text = string.IsNullOrWhiteSpace(fullName) ? "Khách hàng" : fullName.Trim();
        _initials = Formatter.Initials(fullName);
        _pnlAvatar.Invalidate();
    }

    private async void ProfileForm_Shown(object? sender, EventArgs e)
    {
        _lblStatus.Text = "Đang tải hồ sơ...";
        try
        {
            var profile = await _profileService.GetMyProfileAsync();
            if (IsDisposed)
            {
                return;
            }

            ShowProfile(profile);
        }
        catch (Exception)
        {
            if (!IsDisposed)
            {
                _lblStatus.Text = "Không tải được hồ sơ. Vui lòng thử lại.";
            }
        }
    }

    private void ShowProfile(CustomerProfile profile)
    {
        SetName(profile.FullName);
        _lblEmail.Text = string.IsNullOrWhiteSpace(profile.Email) ? NotUpdatedText : profile.Email;
        _lblRole.Text = GetRoleDisplayName(profile.Role);

        _lblStatus.Visible = false;
        _pnlRows.Visible = true;
    }

    private void PnlAvatar_Paint(object? sender, PaintEventArgs e)
    {
        var graphics = e.Graphics;
        graphics.SmoothingMode = SmoothingMode.AntiAlias;
        graphics.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;

        var bounds = new Rectangle(0, 0, _pnlAvatar.Width - 1, _pnlAvatar.Height - 1);
        using var background = new SolidBrush(AppColors.Primary);
        graphics.FillEllipse(background, bounds);

        using var textBrush = new SolidBrush(AppColors.OnPrimary);
        using var centered = new StringFormat
        {
            Alignment = StringAlignment.Center,
            LineAlignment = StringAlignment.Center,
        };
        graphics.DrawString(_initials, AppFonts.Title, textBrush, bounds, centered);
    }

    private void PnlRows_Paint(object? sender, PaintEventArgs e)
    {
        using var pen = new Pen(AppColors.Divider);
        var rowHeight = UiScale.Px(RowHeight);
        for (var y = rowHeight - 1; y < _pnlRows.Height; y += rowHeight)
        {
            e.Graphics.DrawLine(pen, 0, y, _pnlRows.Width, y);
        }
    }
}
