using System.ComponentModel;
using BobaShop.ClientNetworkLogic.Controls;
using BobaShop.ClientNetworkLogic.Helpers;
using BobaShop.ClientNetworkLogic.Theme;
using BobaShop.CustomerApp.Services;

namespace BobaShop.CustomerApp.Forms.Auth;

[DesignerCategory("Code")]
public class LoginForm : AuthFormBase
{
    private readonly IAuthService _authService;

    private readonly TextField _txtEmail;
    private readonly TextField _txtPassword;
    private readonly Label _lblEmailError;
    private readonly Label _lblPasswordError;
    private readonly Label _lblError;
    private readonly CheckBox _chkShowPassword;
    private readonly LinkLabel _lnkForgotPassword;
    private readonly LinkLabel _lnkRegister;
    private readonly PrimaryButton _btnLogin;

    public LoginForm(IAuthService authService)
    {
        _authService = authService;

        Text = "BobaShop - Đăng nhập";
        Stack.SuspendLayout();

        AddTitle("Đăng nhập", "Chào mừng bạn quay lại");
        _txtEmail = AddField("Email", "ten@email.com", false, out _lblEmailError);
        _txtEmail.MaxLength = Validator.EmailMaxLength;
        _txtPassword = AddField("Mật khẩu", "Nhập mật khẩu", true, out _lblPasswordError);

        // Hàng tùy chọn: hiện mật khẩu bên trái, quên mật khẩu bên phải.
        _chkShowPassword = new CheckBox
        {
            AutoSize = true,
            Text = "Hiện mật khẩu",
            Location = new Point(0, Px(2)),
            UseMnemonic = false,
        };
        _lnkForgotPassword = CreateLink("Quên mật khẩu?");
        _lnkForgotPassword.Location = new Point(ContentWidth - _lnkForgotPassword.PreferredWidth, Px(4));
        var pnlOptions = new Panel
        {
            Size = new Size(ContentWidth, Px(28)),
            Margin = new Padding(0, 0, 0, Px(AppSpacing.Spacing8)),
        };
        pnlOptions.Controls.Add(_chkShowPassword);
        pnlOptions.Controls.Add(_lnkForgotPassword);
        Stack.Controls.Add(pnlOptions);

        _lblError = AddMessage();
        _btnLogin = AddButton("Đăng nhập");
        _lnkRegister = AddLinkRow("Chưa có tài khoản?", "Đăng ký ngay");

        Stack.ResumeLayout(true);

        AcceptButton = _btnLogin;
        _txtEmail.TextChanged += (_, _) => ClearFieldError(_txtEmail, _lblEmailError);
        _txtPassword.TextChanged += (_, _) => ClearFieldError(_txtPassword, _lblPasswordError);
        _chkShowPassword.CheckedChanged += (_, _) => _txtPassword.UseSystemPasswordChar = !_chkShowPassword.Checked;
        _btnLogin.Click += BtnLogin_Click;
        _lnkRegister.LinkClicked += LnkRegister_LinkClicked;
        _lnkForgotPassword.LinkClicked += LnkForgotPassword_LinkClicked;
    }

    /// <summary>Kết quả đăng nhập thành công. Null nếu người dùng đóng cửa sổ mà chưa đăng nhập.</summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public LoginResult? Result { get; private set; }

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

            // TODO: lưu result vào SessionManager của Ngân khi có.
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

    private void LnkRegister_LinkClicked(object? sender, LinkLabelLinkClickedEventArgs e)
    {
        using var registerForm = new RegisterForm(_authService);
        if (ShowAuthDialog(registerForm) == DialogResult.OK && registerForm.RegisteredEmail is { } email)
        {
            PrefillEmail(email);
        }
    }

    private void LnkForgotPassword_LinkClicked(object? sender, LinkLabelLinkClickedEventArgs e)
    {
        using var forgotForm = new ForgotPasswordForm(_authService, _txtEmail.Text.Trim());
        if (ShowAuthDialog(forgotForm) == DialogResult.OK && forgotForm.ResetEmail is { } email)
        {
            PrefillEmail(email);
        }
    }

    /// <summary>Ẩn màn đăng nhập trong lúc mở màn con, đóng màn con thì hiện lại.</summary>
    private DialogResult ShowAuthDialog(Form dialog)
    {
        Hide();
        try
        {
            return dialog.ShowDialog();
        }
        finally
        {
            Show();
        }
    }

    private void PrefillEmail(string email)
    {
        _txtEmail.Text = email;
        _txtPassword.Text = string.Empty;
        _lblError.Text = string.Empty;
        _txtPassword.FocusInput();
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
        _lnkForgotPassword.Enabled = !busy;
        _lnkRegister.Enabled = !busy;
        UseWaitCursor = busy;
    }
}
