using System.ComponentModel;
using BobaShop.ClientNetworkLogic.Controls;
using BobaShop.ClientNetworkLogic.Helpers;
using BobaShop.CustomerApp.Services;

namespace BobaShop.CustomerApp.Forms.Auth;

[DesignerCategory("Code")]
public class RegisterForm : AuthFormBase
{
    private readonly IAuthService _authService;

    private readonly TextField _txtFullName;
    private readonly TextField _txtEmail;
    private readonly TextField _txtPhone;
    private readonly TextField _txtPassword;
    private readonly TextField _txtConfirmPassword;
    private readonly Label _lblFullNameError;
    private readonly Label _lblEmailError;
    private readonly Label _lblPhoneError;
    private readonly Label _lblPasswordError;
    private readonly Label _lblConfirmPasswordError;
    private readonly Label _lblError;
    private readonly PrimaryButton _btnRegister;
    private readonly LinkLabel _lnkLogin;

    public RegisterForm(IAuthService authService)
    {
        _authService = authService;

        Text = "BobaShop - Đăng ký";
        Stack.SuspendLayout();

        AddTitle("Tạo tài khoản", "Đăng ký để đặt món và theo dõi đơn");
        _txtFullName = AddField("Họ và tên", "Nguyễn Văn A", false, out _lblFullNameError);
        _txtFullName.MaxLength = Validator.FullNameMaxLength;
        _txtEmail = AddField("Email", "ten@email.com", false, out _lblEmailError);
        _txtEmail.MaxLength = Validator.EmailMaxLength;
        _txtPhone = AddField("Số điện thoại", "09xx xxx xxx", false, out _lblPhoneError);
        _txtPhone.MaxLength = 15;
        _txtPassword = AddField("Mật khẩu", $"Ít nhất {Validator.PasswordMinLength} ký tự", true, out _lblPasswordError);
        _txtConfirmPassword = AddField("Nhập lại mật khẩu", "Nhập lại mật khẩu", true, out _lblConfirmPasswordError);
        _lblError = AddMessage();
        _btnRegister = AddButton("Đăng ký");
        _lnkLogin = AddLinkRow("Đã có tài khoản?", "Đăng nhập");

        Stack.ResumeLayout(true);

        AcceptButton = _btnRegister;
        _txtFullName.TextChanged += (_, _) => SetFieldError(_txtFullName, _lblFullNameError, null);
        _txtEmail.TextChanged += (_, _) => SetFieldError(_txtEmail, _lblEmailError, null);
        _txtPhone.TextChanged += (_, _) => SetFieldError(_txtPhone, _lblPhoneError, null);
        _txtPassword.TextChanged += (_, _) => SetFieldError(_txtPassword, _lblPasswordError, null);
        _txtConfirmPassword.TextChanged += (_, _) => SetFieldError(_txtConfirmPassword, _lblConfirmPasswordError, null);
        _btnRegister.Click += BtnRegister_Click;
        _lnkLogin.LinkClicked += (_, _) => Close();
    }

    /// <summary>Email vừa đăng ký thành công, để màn đăng nhập điền sẵn.</summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string? RegisteredEmail { get; private set; }

    private async void BtnRegister_Click(object? sender, EventArgs e)
    {
        var fullName = _txtFullName.Text.Trim();
        var email = _txtEmail.Text.Trim();
        var phone = _txtPhone.Text.Replace(" ", string.Empty);
        var password = _txtPassword.Text;

        var errors = new (TextField Field, Label Label, string? Error)[]
        {
            (_txtFullName, _lblFullNameError, Validator.FullName(fullName)),
            (_txtEmail, _lblEmailError, Validator.Email(email)),
            (_txtPhone, _lblPhoneError, Validator.Phone(phone)),
            (_txtPassword, _lblPasswordError, Validator.NewPassword(password)),
            (_txtConfirmPassword, _lblConfirmPasswordError, Validator.ConfirmPassword(password, _txtConfirmPassword.Text)),
        };

        _lblError.Text = string.Empty;
        TextField? firstInvalid = null;
        foreach (var (field, label, error) in errors)
        {
            SetFieldError(field, label, error);
            if (error is not null)
            {
                firstInvalid ??= field;
            }
        }

        if (firstInvalid is not null)
        {
            firstInvalid.FocusInput();
            return;
        }

        SetBusy(true);
        try
        {
            var result = await _authService.RegisterAsync(fullName, email, phone, password);
            if (IsDisposed)
            {
                return;
            }

            if (!result.Success)
            {
                SetBusy(false);
                _lblError.Text = result.ErrorMessage ?? "Đăng ký thất bại. Vui lòng thử lại.";
                return;
            }

            SetBusy(false);
            DialogHelper.ShowInfo(this, "Đăng ký thành công. Bạn có thể đăng nhập ngay bây giờ.");
            RegisteredEmail = email;
            DialogResult = DialogResult.OK;
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

    private void SetBusy(bool busy)
    {
        _btnRegister.Enabled = !busy;
        _btnRegister.Text = busy ? "Đang đăng ký..." : "Đăng ký";
        _txtFullName.Enabled = !busy;
        _txtEmail.Enabled = !busy;
        _txtPhone.Enabled = !busy;
        _txtPassword.Enabled = !busy;
        _txtConfirmPassword.Enabled = !busy;
        _lnkLogin.Enabled = !busy;
        UseWaitCursor = busy;
    }
}
