namespace Application.Common.Interfaces;

public interface IEmailService
{
    Task SendVerificationCodeAsync(
        string email,
        string code,
        CancellationToken cancellationToken = default);

    Task SendPasswordResetCodeAsync(
        string email,
        string code,
        CancellationToken cancellationToken = default);
}
