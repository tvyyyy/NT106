namespace BobaShop.Application.Auth;

public sealed class EmailOptions
{
    public string Provider { get; set; } = "Smtp";
    public string Host { get; set; } = "smtp.gmail.com";
    public int Port { get; set; } = 587;
    public bool UseSslOnConnect { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FromAddress { get; set; } = string.Empty;
    public string FromName { get; set; } = "Boba Shop";
}
