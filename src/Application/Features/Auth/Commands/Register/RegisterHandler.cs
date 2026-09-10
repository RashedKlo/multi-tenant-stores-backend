// Application/Auth/Commands/Register/RegisterHandler.cs
using Application.Auth.DTOs;
using Application.Common.Interfaces;
using Domain.Common;
using Domain.Entities;
using Domain.Interfaces;
using MediatR;

namespace Application.Auth.Commands.Register;

public sealed class RegisterHandler(
    ICustomerRepository customers,
    IPasswordHasher passwordHasher,
    IVerificationCodeStore codeStore,
    IEmailService emailService)
    : IRequestHandler<RegisterCommand, Result<RegisterResultDto>>
{
    public async Task<Result<RegisterResultDto>> Handle(RegisterCommand request, CancellationToken ct)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        if (await customers.ExistsByEmailAsync(email, ct))
            return Result<RegisterResultDto>.Failure(
                Error.Conflict("Auth.Email.AlreadyExists", "An account with this email already exists."));

        var hash = passwordHasher.Hash(request.Password);

        var createResult = Customer.CreateWithPassword(
            request.FirstName,
            request.LastName,
            email,
            hash);

        if (createResult.IsFailure)
            return Result<RegisterResultDto>.Failure(createResult.Errors);

        var customer = createResult.Value!;
        await customers.AddAsync(customer, ct);
        await customers.SaveChangesAsync(ct);

        var code = GenerateNumericCode();
        await codeStore.StoreCodeAsync(email, code, TimeSpan.FromMinutes(15), ct);
        await emailService.SendVerificationCodeAsync(email, code, ct);

        return Result<RegisterResultDto>.Success(
            new RegisterResultDto(customer.Id, customer.Email));
    }

    private static string GenerateNumericCode()
        => Random.Shared.Next(100000, 999999).ToString();
}