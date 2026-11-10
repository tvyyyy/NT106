using System.Text.RegularExpressions;

namespace BobaShop.ClientNetworkLogic.Helpers;

/// <summary>
/// Kiểm tra dữ liệu nhập phía client. Mỗi hàm trả về null nếu hợp lệ,
/// ngược lại trả về thông báo lỗi để hiển thị dưới ô nhập.
/// Server vẫn kiểm tra lại, đây chỉ để báo lỗi sớm cho người dùng.
/// </summary>
public static partial class Validator
{
    // Khớp độ dài cột bảng Users trong ERD-final.
    public const int FullNameMaxLength = 100;
    public const int EmailMaxLength = 150;

    // TODO: chốt với Vy cho khớp quy tắc phía server.
    public const int PasswordMinLength = 6;
    public const int OtpLength = 6;

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$")]
    private static partial Regex EmailRegex();

    // Số di động Việt Nam: 10 chữ số, bắt đầu bằng 0.
    [GeneratedRegex(@"^0\d{9}$")]
    private static partial Regex PhoneRegex();

    [GeneratedRegex(@"^\d{6}$")]
    private static partial Regex OtpRegex();

    public static string? Required(string? value, string fieldName)
    {
        return string.IsNullOrWhiteSpace(value) ? $"Vui lòng nhập {fieldName}." : null;
    }

    public static string? FullName(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "Vui lòng nhập họ và tên.";
        }

        return value.Trim().Length > FullNameMaxLength
            ? $"Họ và tên không được dài quá {FullNameMaxLength} ký tự."
            : null;
    }

    public static string? Email(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "Vui lòng nhập email.";
        }

        var email = value.Trim();
        if (email.Length > EmailMaxLength)
        {
            return $"Email không được dài quá {EmailMaxLength} ký tự.";
        }

        return EmailRegex().IsMatch(email) ? null : "Email không đúng định dạng.";
    }

    public static string? Phone(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "Vui lòng nhập số điện thoại.";
        }

        var phone = value.Replace(" ", string.Empty);
        return PhoneRegex().IsMatch(phone) ? null : "Số điện thoại gồm 10 chữ số, bắt đầu bằng 0.";
    }

    /// <summary>Dùng khi đặt mật khẩu mới (đăng ký, đặt lại). Đăng nhập chỉ cần Required.</summary>
    public static string? NewPassword(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return "Vui lòng nhập mật khẩu.";
        }

        return value.Length < PasswordMinLength
            ? $"Mật khẩu phải có ít nhất {PasswordMinLength} ký tự."
            : null;
    }

    public static string? ConfirmPassword(string? password, string? confirmation)
    {
        if (string.IsNullOrEmpty(confirmation))
        {
            return "Vui lòng nhập lại mật khẩu.";
        }

        return string.Equals(password, confirmation, StringComparison.Ordinal)
            ? null
            : "Mật khẩu nhập lại không khớp.";
    }

    public static string? OtpCode(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "Vui lòng nhập mã xác nhận.";
        }

        return OtpRegex().IsMatch(value.Trim()) ? null : $"Mã xác nhận gồm {OtpLength} chữ số.";
    }
}
