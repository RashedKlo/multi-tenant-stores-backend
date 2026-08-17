namespace Application.Common.Interfaces;

/// <summary>
/// Stores short-lived verification / password-reset codes.
/// Kept separate from <see cref="ICacheService"/> so TTL, attempt limits,
/// and single-use semantics stay explicit and cannot be mixed with general cache keys.
/// </summary>
public interface IVerificationCodeStore
{
    Task StoreCodeAsync(
        string email,
        string code,
        TimeSpan ttl,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns true and consumes the code when it is valid; false otherwise
    /// (wrong code, expired, or too many attempts).
    /// </summary>
    Task<bool> ValidateAndConsumeAsync(
        string email,
        string code,
        CancellationToken cancellationToken = default);
}
