using BobaShop.Application.Auth;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace BobaShop.Infrastructure.Auth;

public sealed class SmtpEmailSender(EmailOptions options, ILogger<SmtpEmailSender> logger) : IEmailSender
{
    public async Task SendPasswordResetCodeAsync(string email, string otp, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(options.Host) || options.Port is < 1 or > 65535 ||
            string.IsNullOrWhiteSpace(options.Username) || string.IsNullOrWhiteSpace(options.Password) ||
            string.IsNullOrWhiteSpace(options.FromAddress))
            throw new InvalidOperationException("Cần cấu hình Email:Host, Port, Username, Password và FromAddress. Đặt thông tin đăng nhập bằng User Secrets.");

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(options.FromName, options.FromAddress));
        message.To.Add(MailboxAddress.Parse(email));
        message.Subject = "Mã xác nhận đặt lại mật khẩu Boba Shop";
        message.Body = new TextPart("plain")
        {
            Text = $"Mã xác nhận của bạn là {otp}. Mã có hiệu lực trong 10 phút và chỉ sử dụng được một lần. Nếu bạn không yêu cầu đặt lại mật khẩu, hãy bỏ qua email này."
        };

        using var client = new SmtpClient();
        try
        {
            var socketOptions = options.UseSslOnConnect ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTls;
            await client.ConnectAsync(options.Host, options.Port, socketOptions, ct);
            await client.AuthenticateAsync(options.Username, options.Password, ct);
            await client.SendAsync(message, ct);
            await client.DisconnectAsync(true, ct);
            logger.LogInformation("Đã gửi email đặt lại mật khẩu đến {Email}.", email);
        }
        catch
        {
            if (client.IsConnected)
            {
                try { await client.DisconnectAsync(true, CancellationToken.None); }
                catch (Exception disconnectError) { logger.LogDebug(disconnectError, "Không ngắt được kết nối SMTP sau lỗi gửi email."); }
            }
            throw;
        }
    }
}
