using BobaShop.Application.Auth;

namespace BobaShop.Infrastructure.Auth;

public class PasswordHasher : IPasswordHasher
{
    public string Hash(string value)
    {
        var salt = System.Security.Cryptography.RandomNumberGenerator.GetBytes(16);
        var derived = System.Security.Cryptography.Rfc2898DeriveBytes.Pbkdf2(value, salt, 210_000, System.Security.Cryptography.HashAlgorithmName.SHA256, 32);
        return $"pbkdf2-sha256$210000${Convert.ToBase64String(salt)}${Convert.ToBase64String(derived)}";
    }

    public bool Verify(string value, string hash)
    {
        var parts = hash.Split('$');
        if (parts.Length != 4 || parts[0] != "pbkdf2-sha256" || !int.TryParse(parts[1], out var iterations)) return false;
        try
        {
            var salt = Convert.FromBase64String(parts[2]);
            var expected = Convert.FromBase64String(parts[3]);
            var actual = System.Security.Cryptography.Rfc2898DeriveBytes.Pbkdf2(value, salt, iterations, System.Security.Cryptography.HashAlgorithmName.SHA256, expected.Length);
            return System.Security.Cryptography.CryptographicOperations.FixedTimeEquals(actual, expected);
        }
        catch (FormatException) { return false; }
    }
}
