using System.Text.RegularExpressions;

namespace Domain.Common;

/// <summary>
/// Shared domain-level validation & normalization helpers
/// derived from the PostgreSQL schema CHECK constraints.
/// Collects <see cref="Error"/> instances instead of plain strings.
/// </summary>
public static class DomainValidation
{
    // ------------------------------------------------------------------
    // Email
    // ------------------------------------------------------------------

    private static readonly Regex EmailRegex = new(
        @"^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}$",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    /// <summary>
    /// Trims, lower-cases and validates a required email.
    /// Returns the normalized value or adds an error.
    /// </summary>
    public static string NormalizeRequiredEmail(
        string? email, List<Error> errors, string fieldName = "Email")
    {
        email = email?.Trim().ToLowerInvariant() ?? string.Empty;

        if (email.Length == 0)
            errors.Add(Error.Validation($"{fieldName}.Required", $"{fieldName} is required."));
        else if (!EmailRegex.IsMatch(email))
            errors.Add(Error.Validation($"{fieldName}.InvalidFormat", $"Invalid {fieldName.ToLowerInvariant()} format."));

        return email;
    }

    /// <summary>
    /// Trims, lower-cases and validates an optional email.
    /// Empty / whitespace → null. Invalid format → error.
    /// </summary>
    public static string? NormalizeOptionalEmail(
        string? email, List<Error> errors, string fieldName = "Email")
    {
        if (email is null)
            return null;

        var trimmed = email.Trim().ToLowerInvariant();
        if (trimmed.Length == 0)
            return null;

        if (!EmailRegex.IsMatch(trimmed))
            errors.Add(Error.Validation($"{fieldName}.InvalidFormat", $"Invalid {fieldName.ToLowerInvariant()} format."));

        return trimmed;
    }
// ------------------------------------------------------------------
// Guid (not empty)
// ------------------------------------------------------------------

/// <summary>
/// Ensures a required Guid is not <see cref="Guid.Empty"/>.
/// </summary>
public static void EnsureNotEmptyGuid(Guid value, List<Error> errors, string fieldName)
{
    if (value == Guid.Empty)
        errors.Add(Error.Validation($"{fieldName}.Required", $"{fieldName} is required."));
}

/// <summary>
/// Ensures an optional Guid is either null or not <see cref="Guid.Empty"/>.
/// </summary>
public static void EnsureNotEmptyGuid(Guid? value, List<Error> errors, string fieldName)
{
    if (value is null)
        return;

    if (value == Guid.Empty)
        errors.Add(Error.Validation($"{fieldName}.Required", $"{fieldName} cannot be an empty GUID."));
}
    // ------------------------------------------------------------------
    // Required / Optional strings (btrim length > 0)
    // ------------------------------------------------------------------

    /// <summary>
    /// Trims a required string. Empty after trim → error.
    /// </summary>
    public static string NormalizeRequiredString(
        string? value, List<Error> errors, string fieldName)
    {
        value = value?.Trim() ?? string.Empty;

        if (value.Length == 0)
            errors.Add(Error.Validation($"{fieldName}.Required", $"{fieldName} cannot be empty."));

        return value;
    }

    /// <summary>
    /// Trims an optional string. Empty / whitespace → null.
    /// </summary>
    public static string? NormalizeOptional(string? value)
    {
        if (value is null)
            return null;

        var trimmed = value.Trim();
        return trimmed.Length == 0 ? null : trimmed;
    }

    // ------------------------------------------------------------------
    // Non-negative integers / decimals (display_order, stock, price …)
    // ------------------------------------------------------------------

    public static void EnsureNonNegative(int value, List<Error> errors, string fieldName)
    {
        if (value < 0)
            errors.Add(Error.Validation($"{fieldName}.Negative", $"{fieldName} cannot be negative."));
    }

    public static void EnsureNonNegative(decimal value, List<Error> errors, string fieldName)
    {
        if (value < 0)
            errors.Add(Error.Validation($"{fieldName}.Negative", $"{fieldName} cannot be negative."));
    }

    public static void EnsurePositive(int value, List<Error> errors, string fieldName)
    {
        if (value <= 0)
            errors.Add(Error.Validation($"{fieldName}.NotPositive", $"{fieldName} must be greater than zero."));
    }

    public static void EnsurePositive(decimal value, List<Error> errors, string fieldName)
    {
        if (value <= 0)
            errors.Add(Error.Validation($"{fieldName}.NotPositive", $"{fieldName} must be greater than zero."));
    }

    // ------------------------------------------------------------------
    // Geographic coordinates
    // ------------------------------------------------------------------

    public static void EnsureValidLatitude(decimal? latitude, List<Error> errors, string fieldName = "Latitude")
    {
        if (latitude is null)
            return;

        if (latitude < -90 || latitude > 90)
            errors.Add(Error.Validation($"{fieldName}.OutOfRange", $"{fieldName} must be between -90 and 90."));
    }

    public static void EnsureValidLongitude(decimal? longitude, List<Error> errors, string fieldName = "Longitude")
    {
        if (longitude is null)
            return;

        if (longitude < -180 || longitude > 180)
            errors.Add(Error.Validation($"{fieldName}.OutOfRange", $"{fieldName} must be between -180 and 180."));
    }

    public static void EnsureValidLatitude(decimal latitude, List<Error> errors, string fieldName = "Latitude")
    {
        if (latitude < -90 || latitude > 90)
            errors.Add(Error.Validation($"{fieldName}.OutOfRange", $"{fieldName} must be between -90 and 90."));
    }

    public static void EnsureValidLongitude(decimal longitude, List<Error> errors, string fieldName = "Longitude")
    {
        if (longitude < -180 || longitude > 180)
            errors.Add(Error.Validation($"{fieldName}.OutOfRange", $"{fieldName} must be between -180 and 180."));
    }

    // ------------------------------------------------------------------
    // Rating (0 – 5)
    // ------------------------------------------------------------------

    public static void EnsureValidRating(decimal rating, List<Error> errors, string fieldName = "Rating")
    {
        if (rating < 0 || rating > 5)
            errors.Add(Error.Validation($"{fieldName}.OutOfRange", $"{fieldName} must be between 0 and 5."));
    }

    // ------------------------------------------------------------------
    // Percentage discount (0 < value ≤ 100)
    // ------------------------------------------------------------------

    public static void EnsureValidPercentage(decimal value, List<Error> errors, string fieldName = "Value")
    {
        if (value <= 0 || value > 100)
            errors.Add(Error.Validation($"{fieldName}.OutOfRange", $"{fieldName} must be greater than 0 and less than or equal to 100."));
    }

    // ------------------------------------------------------------------
    // Date range (start < end)
    // ------------------------------------------------------------------

    public static void EnsureValidDateRange(DateTime? start, DateTime? end, List<Error> errors)
    {
        if (start.HasValue && end.HasValue && start >= end)
            errors.Add(Error.Validation("DateRange.Invalid", "Start date must be earlier than end date."));
    }

    // ------------------------------------------------------------------
    // Password hash / token (non-empty text)
    // ------------------------------------------------------------------

    public static string NormalizeRequiredHash(
        string? hash, List<Error> errors, string fieldName = "PasswordHash")
    {
        if (string.IsNullOrEmpty(hash))
        {
            errors.Add(Error.Validation($"{fieldName}.Required", $"{fieldName} cannot be empty."));
            return string.Empty;
        }

        return hash;
    }
// ------------------------------------------------------------------
// DateTime — must be in the future
// ------------------------------------------------------------------

/// <summary>
/// Ensures a required DateTime is strictly after <see cref="DateTime.UtcNow"/>.
/// </summary>
public static void EnsureInFuture(DateTime value, List<Error> errors, string fieldName = "ExpiresAt")
{
    if (value <= DateTime.UtcNow)
        errors.Add(Error.Validation($"{fieldName}.NotInFuture", $"{fieldName} must be in the future."));
}

/// <summary>
/// Ensures an optional DateTime is either null or strictly after <see cref="DateTime.UtcNow"/>.
/// </summary>
public static void EnsureInFuture(DateTime? value, List<Error> errors, string fieldName = "ExpiresAt")
{
    if (value is null)
        return;

    if (value <= DateTime.UtcNow)
        errors.Add(Error.Validation($"{fieldName}.NotInFuture", $"{fieldName} must be in the future."));
}
    // ------------------------------------------------------------------
    // Compare-price rule (compare_price >= price when present)
    // ------------------------------------------------------------------

    public static void EnsureComparePriceValid(decimal price, decimal? comparePrice, List<Error> errors)
    {
        if (comparePrice.HasValue && comparePrice < price)
            errors.Add(Error.Validation("ComparePrice.LessThanPrice", "Compare price cannot be lower than the regular price."));
    }

    // ------------------------------------------------------------------
    // Selection constraints (option groups)
    // ------------------------------------------------------------------

    public static void EnsureSelectionBounds(int minSelection, int maxSelection, List<Error> errors)
    {
        if (minSelection < 0)
            errors.Add(Error.Validation("MinSelection.Negative", "Minimum selection cannot be negative."));

        if (maxSelection < 1)
            errors.Add(Error.Validation("MaxSelection.TooLow", "Maximum selection must be at least 1."));

        if (maxSelection < minSelection)
            errors.Add(Error.Validation("SelectionBounds.Invalid", "Maximum selection cannot be less than minimum selection."));
    }
}