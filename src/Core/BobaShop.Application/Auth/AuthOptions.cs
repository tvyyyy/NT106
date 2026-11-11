namespace BobaShop.Application.Auth;

public class AuthOptions
{
    public string Issuer { get; set; } = "BobaShop";
    public string Audience { get; set; } = "BobaShop.Clients";
    public string Key { get; set; } = string.Empty;
    public int ExpiryMinutes { get; set; } = 60;
}
