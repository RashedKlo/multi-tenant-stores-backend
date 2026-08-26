using Application.Common.Interfaces;
using Infrastructure.Settings;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace Infrastructure.Services;

public class SmtpEmailService(
    IOptions<SmtpSettings> options,
    ILogger<SmtpEmailService> logger) : IEmailService
{
    private readonly SmtpSettings _settings = options.Value;

    public Task SendVerificationCodeAsync(
        string toEmail,
        string code,
        CancellationToken cancellationToken = default) =>
        SendAsync(
            toEmail,
            "Verify your email",
            $"Your verification code is: {code}\n\nThis code expires in 10 minutes.",
            cancellationToken);

    public Task SendPasswordResetCodeAsync(
        string toEmail,
        string code,
        CancellationToken cancellationToken = default) =>
        SendAsync(
            toEmail,
            "Reset your password",
            $"Your password reset code is: {code}\n\nThis code expires in 10 minutes. If you didn't request this, you can ignore this email.",
            cancellationToken);

    private async Task SendAsync(
        string toEmail,
        string subject,
        string body,
        CancellationToken cancellationToken)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_settings.FromName, _settings.FromEmail));
        message.To.Add(MailboxAddress.Parse(toEmail));
        message.Subject = subject;
        message.Body = new TextPart("plain") { Text = body };

        using var client = new SmtpClient();

        try
        {
            // Automatically chooses the right secure option
            var secureSocketOptions = _settings.UseSsl
                ? SecureSocketOptions.StartTls
                : SecureSocketOptions.None;

            await client.ConnectAsync(
                _settings.Host,
                _settings.Port,
                secureSocketOptions,
                cancellationToken);

            // Only authenticate if credentials are provided (MailHog doesn't need them)
            if (!string.IsNullOrWhiteSpace(_settings.Username))
            {
                await client.AuthenticateAsync(
                    _settings.Username,
                    _settings.Password,
                    cancellationToken);
            }

            await client.SendAsync(message, cancellationToken);
            await client.DisconnectAsync(true, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send email to {Email}", toEmail);
            throw new InvalidOperationException("Failed to send email.", ex);
        }
    }
}