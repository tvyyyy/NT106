using System.ComponentModel;
using BobaShop.ClientNetworkLogic.Controls;
using BobaShop.ClientNetworkLogic.Helpers;
using BobaShop.ClientNetworkLogic.Theme;
using BobaShop.CustomerApp.Services;

namespace BobaShop.CustomerApp.Forms.Auth;

/// <summary>
/// Quên mật khẩu gồm hai bước trong cùng một cửa sổ:
/// bước 1 nhập email để nhận mã, bước 2 nhập mã 6 số và mật khẩu mới.
/// </summary>
[DesignerCategory("Code")]
public class ForgotPasswordForm : AuthFormBase
{
    // Khớp cooldown gửi lại mã và hạn của mã mà nhóm đã chốt.
    private const int ResendCooldownSeconds = 60;
    private const int CodeLifetimeMinutes = 10;

    private readonly IAuthService _authService;
    private readonly System.Windows.Forms.Timer _resendTimer;
    private readonly List<Control> _emailStepControls;
    private readonly List<Control> _resetStepControls;

    // Bước 1.
    private readonly TextField _txtEmail;
    private readonly Label _lblEmailError;
    private readonly Label _lblEmailStepError;
    private readonly PrimaryButton _btnSendCode;
    private readonly LinkLabel _lnkBackFromEmail;

    // Bước 2.
    private readonly Label _lblResetSubtitle;
    private readonly TextField _txtCode;
    private readonly TextField _txtNewPassword;
    private readonly TextField _txtConfirmPassword;
    private readonly Label _lblCodeError;
    private readonly Label _lblNewPasswordError;
    private readonly Label _lblConfirmPasswordError;
    private readonly Label _lblResetStepError;
    private readonly LinkLabel _lnkResend;
    private readonly PrimaryButton _btnReset;
    private readonly LinkLabel _lnkBackFromReset;

    private string _email = string.Empty;
    private int _resendSecondsLeft;

    public ForgotPasswordForm(IAuthService authService, string initialEmail = "")
    {
        _authService = authService;
        _resendTimer = new System.Windows.Forms.Timer { Interval = 1000 };
        _resendTimer.Tick += ResendTimer_Tick;

        Text = "BobaShop - Quên mật khẩu";
        Stack.SuspendLayout();

        // Bước 1: nhập email.
        var firstEmailControl = Stack.Controls.Count;
        AddTitle(
            "Quên mật khẩu",
            $"Nhập email đã đăng ký. Chúng tôi sẽ gửi một mã xác nhận gồm {Validator.OtpLength} số để bạn đặt lại mật khẩu.");
        _txtEmail = AddField("Email", "ten@email.com", false, out _lblEmailError);
        _txtEmail.MaxLength = Validator.EmailMaxLength;
        _txtEmail.Text = initialEmail;
        _lblEmailStepError = AddMessage();
        _btnSendCode = AddButton("Gửi mã xác nhận");
        _lnkBackFromEmail = AddLinkRow(string.Empty, "Quay lại đăng nhập");
        _emailStepControls = TakeControlsFrom(firstEmailControl);

        // Bước 2: nhập mã và mật khẩu mới.
        var firstResetControl = Stack.Controls.Count;
        _lblResetSubtitle = AddTitle("Đặt lại mật khẩu", string.Empty);
        _txtCode = AddField("Mã xác nhận", $"{Validator.OtpLength} chữ số", false, out _lblCodeError);
        _txtCode.MaxLength = Validator.OtpLength;
        _txtCode.TextAlign = HorizontalAlignment.Center;
        _txtCode.InputFont = AppFonts.Heading;
        _lnkResend = AddLinkRow(string.Empty, "Gửi lại mã");
        _txtNewPassword = AddField("Mật khẩu mới", $"Ít nhất {Validator.PasswordMinLength} ký tự", true, out _lblNewPasswordError);
        _txtConfirmPassword = AddField("Nhập lại mật khẩu mới", "Nhập lại mật khẩu mới", true, out _lblConfirmPasswordError);
        _lblResetStepError = AddMessage();
        _btnReset = AddButton("Đặt lại mật khẩu");
        _lnkBackFromReset = AddLinkRow(string.Empty, "Quay lại đăng nhập");
        _resetStepControls = TakeControlsFrom(firstResetControl);

        Stack.ResumeLayout(true);

        _txtEmail.TextChanged += (_, _) => SetFieldError(_txtEmail, _lblEmailError, null);
        _txtCode.TextChanged += (_, _) => SetFieldError(_txtCode, _lblCodeError, null);
        _txtNewPassword.TextChanged += (_, _) => SetFieldError(_txtNewPassword, _lblNewPasswordError, null);
        _txtConfirmPassword.TextChanged += (_, _) => SetFieldError(_txtConfirmPassword, _lblConfirmPasswordError, null);
        _btnSendCode.Click += BtnSendCode_Click;
        _btnReset.Click += BtnReset_Click;
        _lnkResend.LinkClicked += LnkResend_LinkClicked;
        _lnkBackFromEmail.LinkClicked += (_, _) => Close();
        _lnkBackFromReset.LinkClicked += (_, _) => Close();

        ShowEmailStep();
    }

    /// <summary>Email vừa đặt lại mật khẩu thành công, để màn đăng nhập điền sẵn.</summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string? ResetEmail { get; private set; }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _resendTimer.Dispose();
        }

        base.Dispose(disposing);
    }

    private List<Control> TakeControlsFrom(int startIndex)
    {
        var controls = new List<Control>();
        for (var i = startIndex; i < Stack.Controls.Count; i++)
        {
            controls.Add(Stack.Controls[i]);
        }

        return controls;
    }

    private void ShowEmailStep()
    {
        SetStepVisibility(showEmailStep: true);
        AcceptButton = _btnSendCode;
    }

    private void ShowResetStep()
    {
        _lblResetSubtitle.Text =
            $"Mã xác nhận đã gửi tới {_email} và có hiệu lực trong {CodeLifetimeMinutes} phút.";
        SetStepVisibility(showEmailStep: false);
        AcceptButton = _btnReset;
        StartResendCooldown();
        _txtCode.FocusInput();
    }

    private void SetStepVisibility(bool showEmailStep)
    {
        Stack.SuspendLayout();
        foreach (var control in _emailStepControls)
        {
            control.Visible = showEmailStep;
        }

        foreach (var control in _resetStepControls)
        {
            control.Visible = !showEmailStep;
        }

        Stack.ResumeLayout(true);
    }

    private async void BtnSendCode_Click(object? sender, EventArgs e)
    {
        var email = _txtEmail.Text.Trim();
        var emailError = Validator.Email(email);
        SetFieldError(_txtEmail, _lblEmailError, emailError);
        _lblEmailStepError.Text = string.Empty;
        if (emailError is not null)
        {
            _txtEmail.FocusInput();
            return;
        }

        SetEmailStepBusy(true);
        try
        {
            var result = await _authService.SendPasswordResetCodeAsync(email);
            if (IsDisposed)
            {
                return;
            }

            SetEmailStepBusy(false);
            if (!result.Success)
            {
                _lblEmailStepError.Text = result.ErrorMessage ?? "Không gửi được mã. Vui lòng thử lại.";
                return;
            }

            _email = email;
            ShowResetStep();
        }
        catch (Exception)
        {
            if (!IsDisposed)
            {
                _lblEmailStepError.Text = "Không kết nối được máy chủ. Vui lòng thử lại.";
            }
        }
        finally
        {
            if (!IsDisposed)
            {
                SetEmailStepBusy(false);
            }
        }
    }

    private async void BtnReset_Click(object? sender, EventArgs e)
    {
        var code = _txtCode.Text.Trim();
        var newPassword = _txtNewPassword.Text;

        var codeError = Validator.OtpCode(code);
        var passwordError = Validator.NewPassword(newPassword);
        var confirmError = Validator.ConfirmPassword(newPassword, _txtConfirmPassword.Text);
        SetFieldError(_txtCode, _lblCodeError, codeError);
        SetFieldError(_txtNewPassword, _lblNewPasswordError, passwordError);
        SetFieldError(_txtConfirmPassword, _lblConfirmPasswordError, confirmError);
        _lblResetStepError.Text = string.Empty;

        if (codeError is not null)
        {
            _txtCode.FocusInput();
            return;
        }

        if (passwordError is not null)
        {
            _txtNewPassword.FocusInput();
            return;
        }

        if (confirmError is not null)
        {
            _txtConfirmPassword.FocusInput();
            return;
        }

        SetResetStepBusy(true);
        try
        {
            var result = await _authService.ResetPasswordAsync(_email, code, newPassword);
            if (IsDisposed)
            {
                return;
            }

            SetResetStepBusy(false);
            if (!result.Success)
            {
                _lblResetStepError.Text = result.ErrorMessage ?? "Đặt lại mật khẩu thất bại. Vui lòng thử lại.";
                _txtCode.FocusInput();
                return;
            }

            _resendTimer.Stop();
            DialogHelper.ShowInfo(this, "Đặt lại mật khẩu thành công. Hãy đăng nhập bằng mật khẩu mới.");
            ResetEmail = _email;
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception)
        {
            if (!IsDisposed)
            {
                _lblResetStepError.Text = "Không kết nối được máy chủ. Vui lòng thử lại.";
            }
        }
        finally
        {
            if (!IsDisposed)
            {
                SetResetStepBusy(false);
            }
        }
    }

    private async void LnkResend_LinkClicked(object? sender, LinkLabelLinkClickedEventArgs e)
    {
        if (_resendSecondsLeft > 0)
        {
            return;
        }

        _lblResetStepError.Text = string.Empty;
        StartResendCooldown();
        try
        {
            var result = await _authService.SendPasswordResetCodeAsync(_email);
            if (!IsDisposed && !result.Success)
            {
                _lblResetStepError.Text = result.ErrorMessage ?? "Không gửi lại được mã. Vui lòng thử lại.";
            }
        }
        catch (Exception)
        {
            if (!IsDisposed)
            {
                _lblResetStepError.Text = "Không kết nối được máy chủ. Vui lòng thử lại.";
            }
        }
    }

    private void StartResendCooldown()
    {
        _resendSecondsLeft = ResendCooldownSeconds;
        UpdateResendLink();
        _resendTimer.Start();
    }

    private void ResendTimer_Tick(object? sender, EventArgs e)
    {
        _resendSecondsLeft--;
        if (_resendSecondsLeft <= 0)
        {
            _resendSecondsLeft = 0;
            _resendTimer.Stop();
        }

        UpdateResendLink();
    }

    private void UpdateResendLink()
    {
        var waiting = _resendSecondsLeft > 0;
        _lnkResend.Enabled = !waiting;
        _lnkResend.Text = waiting ? $"Gửi lại mã sau {_resendSecondsLeft} giây" : "Gửi lại mã";
    }

    private void SetEmailStepBusy(bool busy)
    {
        _btnSendCode.Enabled = !busy;
        _btnSendCode.Text = busy ? "Đang gửi..." : "Gửi mã xác nhận";
        _txtEmail.Enabled = !busy;
        _lnkBackFromEmail.Enabled = !busy;
        UseWaitCursor = busy;
    }

    private void SetResetStepBusy(bool busy)
    {
        _btnReset.Enabled = !busy;
        _btnReset.Text = busy ? "Đang xử lý..." : "Đặt lại mật khẩu";
        _txtCode.Enabled = !busy;
        _txtNewPassword.Enabled = !busy;
        _txtConfirmPassword.Enabled = !busy;
        _lnkBackFromReset.Enabled = !busy;
        UseWaitCursor = busy;
    }
}
