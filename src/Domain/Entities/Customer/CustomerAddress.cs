// Domain/Entities/Customer/CustomerAddress.cs
using Domain.Common;

namespace Domain.Entities;

public sealed class CustomerAddress
{
    public Guid Id { get; private set; }
    public Guid CustomerId { get; private set; }
    public string Label { get; private set; } = null!;
    public decimal Latitude { get; private set; }
    public decimal Longitude { get; private set; }
    public string AddressText { get; private set; } = null!;
    public bool IsDefault { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public DateTime? DeletedAt { get; private set; }

    public bool IsDeleted => DeletedAt.HasValue;


    private CustomerAddress() { }

    // ---------- Factory ----------

    public static Result<CustomerAddress> Create(
        Guid customerId,
        string label,
        decimal latitude,
        decimal longitude,
        string addressText,
        bool isDefault = false)
    {
        var address = new CustomerAddress
        {
            Id = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        return address
            .SetCustomerId(customerId)
            .Bind(() => address.SetLabel(label))
            .Bind(() => address.SetCoordinates(latitude, longitude))
            .Bind(() => address.SetAddressText(addressText))
            .Bind(() =>
            {
                if (isDefault)
                    address.SetAsDefault();
                return Result.Success();
            })
            .Bind(() => Result<CustomerAddress>.Success(address));
    }

    // ---------- Field-level setters (invariants live here) ----------

    private Result SetCustomerId(Guid customerId)
    {
        if (customerId == Guid.Empty)
            return Result.Failure(Error.Validation("Address.CustomerId.Required", "CustomerId is required."));

        CustomerId = customerId;
        return Result.Success();
    }

    private Result SetLabel(string label)
    {
        if (string.IsNullOrWhiteSpace(label))
            return Result.Failure(Error.Validation("Address.Label.Required", "Label is required."));

        var trimmed = label.Trim();
        if (trimmed.Length > 100)
            return Result.Failure(Error.Validation("Address.Label.TooLong", "Label cannot exceed 100 characters."));

        Label = trimmed;
        return Result.Success();
    }

    private Result SetCoordinates(decimal latitude, decimal longitude)
    {
        if (latitude is < -90 or > 90)
            return Result.Failure(Error.Validation("Address.Latitude.Invalid", "Latitude must be between -90 and 90."));

        if (longitude is < -180 or > 180)
            return Result.Failure(Error.Validation("Address.Longitude.Invalid", "Longitude must be between -180 and 180."));

        Latitude = latitude;
        Longitude = longitude;
        return Result.Success();
    }

    private Result SetAddressText(string addressText)
    {
        if (string.IsNullOrWhiteSpace(addressText))
            return Result.Failure(Error.Validation("Address.Text.Required", "Address text is required."));

        var trimmed = addressText.Trim();
        if (trimmed.Length > 500)
            return Result.Failure(Error.Validation("Address.Text.TooLong", "Address text cannot exceed 500 characters."));

        AddressText = trimmed;
        return Result.Success();
    }

    // ---------- Behavior ----------

    public Result Update(string label, decimal latitude, decimal longitude, string addressText)
    {
        if (IsDeleted)
            return Result.Failure(Error.Validation("Address.Deleted", "Cannot update a deleted address."));

        return SetLabel(label)
            .Bind(() => SetCoordinates(latitude, longitude))
            .Bind(() => SetAddressText(addressText))
            .Bind(() =>
            {
                Touch();
                return Result.Success();
            });
    }

    public Result SetAsDefault()
    {
        if (IsDeleted)
            return Result.Failure(Error.Validation("Address.Deleted", "Cannot set a deleted address as default."));

        IsDefault = true;
        Touch();
        return Result.Success();
    }

    public Result UnsetDefault()
    {
        IsDefault = false;
        Touch();
        return Result.Success();
    }

    public Result Delete()
    {
        if (IsDeleted)
            return Result.Success(); // idempotent

        DeletedAt = DateTime.UtcNow;
        IsDefault = false; // deleted address cannot remain default
        Touch();
        return Result.Success();
    }

    public Result Restore()
    {
        if (!IsDeleted)
            return Result.Success();

        DeletedAt = null;
        Touch();
        return Result.Success();
    }

    private void Touch() => UpdatedAt = DateTime.UtcNow;
}