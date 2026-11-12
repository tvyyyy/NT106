using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace BobaShop.Application.Auth;

public static partial class AuthValidation
{
    public static bool ValidEmail(string value) => new EmailAddressAttribute().IsValid(value);
    public static bool ValidPhone(string value) => PhoneRegex().IsMatch(value);
    public static bool StrongPassword(string value) => value.Length >= 8 && UpperRegex().IsMatch(value) && LowerRegex().IsMatch(value) && DigitRegex().IsMatch(value) && SymbolRegex().IsMatch(value);

    [GeneratedRegex("^0[0-9]{9,10}$")] private static partial Regex PhoneRegex();
    [GeneratedRegex("[A-Z]")] private static partial Regex UpperRegex();
    [GeneratedRegex("[a-z]")] private static partial Regex LowerRegex();
    [GeneratedRegex("[0-9]")] private static partial Regex DigitRegex();
    [GeneratedRegex("[^A-Za-z0-9]")] private static partial Regex SymbolRegex();
}
