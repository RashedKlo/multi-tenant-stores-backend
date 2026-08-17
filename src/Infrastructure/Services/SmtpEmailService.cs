using System.Net;
using System.Net.Mail;
using Application.Common.Interfaces;
using Infrastructure.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

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
        using var message = new MailMessage
        {
            From = new MailAddress(_settings.FromEmail, _settings.FromName),
            Subject = subject,
            Body = body,
            IsBodyHtml = false
        };
        message.To.Add(toEmail);

        using var client = new SmtpClient(_settings.Host, _settings.Port)
        {
            Credentials = new NetworkCredential(_settings.Username, _settings.Password),
            EnableSsl = _settings.UseSsl
        };

        try
        {
            await client.SendMailAsync(message, cancellationToken);
        }
        catch (SmtpException ex)
        {
            logger.LogError(ex, "Failed to send email to {Email}", toEmail);
            throw new InvalidOperationException("Failed to send email.", ex);
        }
    }
}
