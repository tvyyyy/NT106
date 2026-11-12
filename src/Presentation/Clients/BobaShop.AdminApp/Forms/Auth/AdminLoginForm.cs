using System.ComponentModel;
using BobaShop.AdminApp.Services;
using BobaShop.ClientNetworkLogic.Controls;
using BobaShop.ClientNetworkLogic.Helpers;
using BobaShop.ClientNetworkLogic.Theme;

namespace BobaShop.AdminApp.Forms.Auth;

/// <summary>
/// Màn đăng nhập của AdminApp. Chỉ tài khoản có vai trò Admin mới vào được;
/// tài khoản khách hàng đăng nhập đúng mật khẩu vẫn bị từ chối.
/// Giao diện dựng bằng code, dùng chung Theme và control với CustomerApp.
/// </summary>
[DesignerCategory("Code")]
public class AdminLoginForm : Form
{
    private const int WindowWidth = 440;
    private const int WindowHeight = 560;
    private const int ContentWidthAt100 = 320;

    // Chữ trong Label lùi vào so với mép control. Hai số này kéo nhãn ra cho thẳng mép ô nhập.
    private const int TitleTextInset = 10;
    private const int LabelTextInset = 4;

    private readonly IAdminAuthService _authService;

    private readonly FlowLayoutPanel _flpStack;
    private readonly TextField _txtEmail;
    private readonly TextField _txtPassword;
    private readonly Label _lblEmailError;
    private readonly Label _lblPasswordError;
    private readonly Label _lblError;
    private readonly CheckBox _chkShowPassword;
    private readonly PrimaryButton _btnLogin;

    public AdminLoginForm(IAdminAuthService authService)
    {
        _authService = authService;

        SuspendLayout();

        Text = "BobaShop Quản trị - Đăng nhập";
        AutoScaleMode = AutoScaleMode.None;
        ClientSize = new Size(UiScale.Px(WindowWidth), UiScale.Px(WindowHeight));
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = AppColors.Background;
        ForeColor = AppColors.TextPrimary;
        Font = AppFonts.Body;

        var contentWidth = UiScale.Px(ContentWidthAt100);
        var titleInset = UiScale.Px(TitleTextInset);
        var labelInset = UiScale.Px(LabelTextInset);

        _flpStack = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            BackColor = AppColors.Background,
            Padding = new Padding(titleInset, 0, 0, 0),
        };

        _flpStack.Controls.Add(new Label
        {
            AutoSize = true,
            Font = AppFonts.Brand,
            ForeColor = AppColors.Primary,
            Text = "BobaShop",
            Margin = new Padding(-titleInset, 0, 0, UiScale.Px(AppSpacing.Spacing16)),
            UseMnemonic = false,
        });
        _flpStack.Controls.Add(new Label
        {
            AutoSize = true,
            MaximumSize = new Size(contentWidth + titleInset, 0),
            Font = AppFonts.Title,
            ForeColor = AppColors.TextPrimary,
            Text = "Đăng nhập quản trị",
            Margin = new Padding(-titleInset, 0, 0, 0),
            UseMnemonic = false,
        });
        _flpStack.Controls.Add(new Label
        {
            AutoSize = true,
            MaximumSize = new Size(contentWidth + labelInset, 0),
            ForeColor = AppColors.TextSecondary,
            Text = "Dành cho tài khoản quản lý cửa hàng",
            Margin = new Padding(-labelInset, 0, 0, UiScale.Px(AppSpacing.Spacing16)),
            UseMnemonic = false,
        });

        _txtEmail = AddField("Email", "ten@email.com", false, contentWidth, out _lblEmailError);
        _txtEmail.MaxLength = Validator.EmailMaxLength;
        _txtPassword = AddField("Mật khẩu", "Nhập mật khẩu", true, contentWidth, out _lblPasswordError);

        _chkShowPassword = new CheckBox
        {
            AutoSize = true,
            Text = "Hiện mật khẩu",
            Margin = new Padding(0, 0, 0, UiScale.Px(AppSpacing.Spacing8)),
            UseMnemonic = false,
        };
        _flpStack.Controls.Add(_chkShowPassword);

        _lblError = new Label
        {
            AutoSize = true,
            MinimumSize = new Size(contentWidth + labelInset, UiScale.Px(20)),
            MaximumSize = new Size(contentWidth + labelInset, 0),
            Font = AppFonts.Caption,
            ForeColor = AppColors.Error,
            Margin = new Padding(-labelInset, 0, 0, UiScale.Px(AppSpacing.Spacing4)),
            UseMnemonic = false,
        };
        _flpStack.Controls.Add(_lblError);

        _btnLogin = new PrimaryButton
        {
            Size = new Size(contentWidth, UiScale.Px(AppSpacing.ButtonHeight)),
            Text = "Đăng nhập",
            Margin = new Padding(0, UiScale.Px(AppSpacing.Spacing4), 0, 0),
        };
        _flpStack.Controls.Add(_btnLogin);

        Controls.Add(_flpStack);
        ResumeLayout(true);

        AcceptButton = _btnLogin;
        _flpStack.SizeChanged += (_, _) => CenterStack();
        _txtEmail.TextChanged += (_, _) => ClearFieldError(_txtEmail, _lblEmailError);
        _txtPassword.TextChanged += (_, _) => ClearFieldError(_txtPassword, _lblPasswordError);
        _chkShowPassword.CheckedChanged += (_, _) => _txtPassword.UseSystemPasswordChar = !_chkShowPassword.Checked;
        _btnLogin.Click += BtnLogin_Click;

        CenterStack();
    }

    /// <summary>Kết quả đăng nhập thành công. Null nếu người dùng đóng cửa sổ mà chưa đăng nhập.</summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public AdminLoginResult? Result { get; private set; }

    /// <summary>Thêm một ô nhập gồm nhãn, ô nhập và dòng báo lỗi bên dưới.</summary>
    private TextField AddField(string label, string placeholder, bool isPassword, int width, out Label lblError)
    {
        var labelInset = UiScale.Px(LabelTextInset);

        _flpStack.Controls.Add(new Label
        {
            AutoSize = true,
            Font = AppFonts.BodyBold,
            ForeColor = AppColors.TextPrimary,
            Text = label,
            Margin = new Padding(-labelInset, 0, 0, UiScale.Px(AppSpacing.Spacing4)),
            UseMnemonic = false,
        });

        var field = new TextField
        {
            Size = new Size(width, UiScale.Px(AppSpacing.InputHeight)),
            PlaceholderText = placeholder,
            UseSystemPasswordChar = isPassword,
            Margin = new Padding(0, 0, 0, UiScale.Px(2)),
        };
        _flpStack.Controls.Add(field);

        lblError = new Label
        {
            AutoSize = true,
            MinimumSize = new Size(width + labelInset, UiScale.Px(20)),
            MaximumSize = new Size(width + labelInset, 0),
            Font = AppFonts.Caption,
            ForeColor = AppColors.Error,
            Margin = new Padding(-labelInset, 0, 0, UiScale.Px(AppSpacing.Spacing4)),
            UseMnemonic = false,
        };
        _flpStack.Controls.Add(lblError);
        return field;
    }

    private async void BtnLogin_Click(object? sender, EventArgs e)
    {
        var email = _txtEmail.Text.Trim();
        var password = _txtPassword.Text;

        var emailError = Validator.Email(email);
        var passwordError = Validator.Required(password, "mật khẩu");
        SetFieldError(_txtEmail, _lblEmailError, emailError);
        SetFieldError(_txtPassword, _lblPasswordError, passwordError);
        _lblError.Text = string.Empty;

        if (emailError is not null)
        {
            _txtEmail.FocusInput();
            return;
        }

        if (passwordError is not null)
        {
            _txtPassword.FocusInput();
            return;
        }

        SetBusy(true);
        try
        {
            var result = await _authService.LoginAsync(email, password);
            if (IsDisposed)
            {
                return;
            }

            if (!result.Success)
            {
                SetBusy(false);
                _lblError.Text = result.ErrorMessage ?? "Đăng nhập thất bại. Vui lòng thử lại.";
                _txtPassword.FocusInput();
                return;
            }

            Result = result;
            Close();
        }
        catch (Exception)
        {
            if (!IsDisposed)
            {
                _lblError.Text = "Không kết nối được máy chủ. Vui lòng thử lại.";
            }
        }
        finally
        {
            if (!IsDisposed)
            {
                SetBusy(false);
            }
        }
    }

    private static void SetFieldError(TextField field, Label lblError, string? error)
    {
        lblError.Text = error ?? string.Empty;
        field.HasError = error is not null;
    }

    private void ClearFieldError(TextField field, Label lblError)
    {
        SetFieldError(field, lblError, null);
        _lblError.Text = string.Empty;
    }

    private void SetBusy(bool busy)
    {
        _btnLogin.Enabled = !busy;
        _btnLogin.Text = busy ? "Đang đăng nhập..." : "Đăng nhập";
        _txtEmail.Enabled = !busy;
        _txtPassword.Enabled = !busy;
        _chkShowPassword.Enabled = !busy;
        UseWaitCursor = busy;
    }

    private void CenterStack()
    {
        var x = Math.Max(0, (ClientSize.Width - _flpStack.Width) / 2);
        var y = Math.Max(0, (ClientSize.Height - _flpStack.Height) / 2);
        _flpStack.Location = new Point(x, y);
    }
}
