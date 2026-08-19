namespace Application.Common.Interfaces;

/// <summary>
/// Provides access to the currently authenticated customer (if any).
/// Registered as scoped so it always reflects the current HTTP request.
/// </summary>
public interface ICurrentUserService
{
    /// <summary>
    /// True when the request carries a valid authentication principal
    /// that contains a customer id claim.
    /// </summary>
    bool IsAuthenticated { get; }

    /// <summary>
    /// The authenticated customer's id, or null when the request is anonymous.
    /// </summary>
    Guid? CustomerId { get; }

    /// <summary>
    /// The active guest session id, or null when the request is not bound to a guest cart.
    /// </summary>
    Guid? GuestSessionId { get; }
}
