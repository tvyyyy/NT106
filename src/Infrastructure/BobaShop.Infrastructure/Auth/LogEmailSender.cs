using BobaShop.Application.Auth;
using Microsoft.Extensions.Logging;

namespace BobaShop.Infrastructure.Auth;

public class LogEmailSender(ILogger<LogEmailSender> logger) : IEmailSender
{
    public Task SendPasswordResetCodeAsync(string email, string otp, CancellationToken ct)
    {
        logger.LogInformation("Mã đặt lại mật khẩu cho {Email}: {Otp}", email, otp);
        return Task.CompletedTask;
    }
}
