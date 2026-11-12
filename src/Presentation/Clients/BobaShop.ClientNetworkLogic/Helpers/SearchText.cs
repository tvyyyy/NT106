using System.Globalization;
using System.Text;

namespace BobaShop.ClientNetworkLogic.Helpers;

/// <summary>Chuẩn hóa chuỗi để tìm kiếm không phân biệt hoa thường và dấu tiếng Việt.</summary>
public static class SearchText
{
    /// <summary>Bỏ dấu, đổi về chữ thường. Ví dụ "Trà Sữa" thành "tra sua".</summary>
    public static string Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var decomposed = value.Trim().Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(decomposed.Length);
        foreach (var character in decomposed)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(character) == UnicodeCategory.NonSpacingMark)
            {
                continue;
            }

            // Chữ đ không tách được thành d + dấu nên phải đổi riêng.
            builder.Append(character is 'đ' or 'Đ' ? 'd' : char.ToLowerInvariant(character));
        }

        return builder.ToString();
    }

    public static bool Contains(string? text, string normalizedQuery)
    {
        return normalizedQuery.Length == 0 || Normalize(text).Contains(normalizedQuery, StringComparison.Ordinal);
    }
}
