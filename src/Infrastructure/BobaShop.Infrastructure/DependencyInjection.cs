using BobaShop.Application.Auth;
using BobaShop.Infrastructure.Auth;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BobaShop.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var section = configuration.GetSection("Jwt");
        var settings = new AuthOptions
        {
            Issuer = section["Issuer"] ?? "BobaShop",
            Audience = section["Audience"] ?? "BobaShop.Clients",
            Key = section["Key"] ?? string.Empty,
            ExpiryMinutes = int.TryParse(section["ExpiryMinutes"], out var minutes) ? minutes : 60
        };
        services.AddSingleton(settings);
        services.AddScoped<IAuthService, AuthService>();
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddSingleton<IJwtTokenService, JwtTokenService>();
        services.AddSingleton<IOtpGenerator, OtpGenerator>();
        var emailSection = configuration.GetSection("Email");
        var emailOptions = new EmailOptions
        {
            Provider = emailSection["Provider"] ?? "Smtp",
            Host = emailSection["Host"] ?? "smtp.gmail.com",
            Port = int.TryParse(emailSection["Port"], out var port) ? port : 587,
            UseSslOnConnect = bool.TryParse(emailSection["UseSslOnConnect"], out var useSsl) && useSsl,
            Username = emailSection["Username"] ?? string.Empty,
            Password = emailSection["Password"] ?? string.Empty,
            FromAddress = emailSection["FromAddress"] ?? string.Empty,
            FromName = emailSection["FromName"] ?? "Boba Shop"
        };
        services.AddSingleton(emailOptions);
        if (string.Equals(emailOptions.Provider, "Log", StringComparison.OrdinalIgnoreCase))
            services.AddSingleton<IEmailSender, LogEmailSender>();
        else if (string.Equals(emailOptions.Provider, "Smtp", StringComparison.OrdinalIgnoreCase))
            services.AddSingleton<IEmailSender, SmtpEmailSender>();
        else
            throw new InvalidOperationException("Email:Provider chỉ nhận 'Smtp' hoặc 'Log'.");
        return services;
    }
}
