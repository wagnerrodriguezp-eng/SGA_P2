using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using SGA.SharedKernel.Domain.Settings;
using SGA.Shared.Messaging.Abstractions;

namespace SGA.Shared.Messaging;

public class MailKitEmailSender : IEmailSender
{
    private readonly EmailSettings _settings;

    public MailKitEmailSender(IOptions<EmailSettings> settings)
    {
        _settings = settings.Value;
    }

    public async Task SendAsync(string toEmail, string subject, string htmlBody, CancellationToken ct = default)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_settings.SenderName, _settings.SenderEmail));
        message.To.Add(MailboxAddress.Parse(toEmail));
        message.Subject = subject;
        message.Body = new BodyBuilder { HtmlBody = htmlBody }.ToMessageBody();

        using var client = new SmtpClient();
        await client.ConnectAsync(
            _settings.Host,
            _settings.Port,
            _settings.EnableSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None,
            ct);
        await client.AuthenticateAsync(_settings.Username, _settings.Password, ct);
        await client.SendAsync(message, ct);
        await client.DisconnectAsync(true, ct);
    }
}
