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
        services.AddSingleton<IEmailSender, LogEmailSender>();
        return services;
    }
}
