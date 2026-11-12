using System.Globalization;

namespace BobaShop.ClientNetworkLogic.Helpers;

/// <summary>Định dạng dữ liệu để hiển thị, dùng chung cho hai client.</summary>
public static class Formatter
{
    private static readonly CultureInfo Vietnamese = CultureInfo.GetCultureInfo("vi-VN");

    /// <summary>Định dạng tiền VND, ví dụ 35000 thành "35.000 đ".</summary>
    public static string Currency(decimal amount)
    {
        return string.Format(Vietnamese, "{0:N0} đ", amount);
    }

    /// <summary>Lấy chữ cái đầu của từ đầu và từ cuối trong tên, ví dụ "Khách Demo" thành "KD".</summary>
    public static string Initials(string? fullName)
    {
        var words = (fullName ?? string.Empty).Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (words.Length == 0)
        {
            return "?";
        }

        var first = char.ToUpperInvariant(words[0][0]).ToString();
        return words.Length == 1 ? first : first + char.ToUpperInvariant(words[^1][0]);
    }
}
