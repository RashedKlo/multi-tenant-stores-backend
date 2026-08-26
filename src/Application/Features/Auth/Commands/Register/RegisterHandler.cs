using System.Security.Cryptography;
using Application.Auth.DTOs;
using Application.Common.Interfaces;
using Domain.Common;
using Domain.Entities;
using Domain.Interfaces;
using MediatR;

namespace Application.Auth.Commands.Register;

public class RegisterHandler(
    ICustomerRepository customerRepository,
    IPasswordHasher passwordHasher,
    IVerificationCodeStore codeStore,
    IEmailService emailService)
    : IRequestHandler<RegisterCommand, Result<RegisterResultDto>>
{
    private static readonly TimeSpan CodeTtl = TimeSpan.FromMinutes(10);

    public async Task<Result<RegisterResultDto>> Handle(
        RegisterCommand request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        // Fast-path check for a friendly error; the DB unique index is the real guarantee.
        if (await customerRepository.EmailExistsAsync(email, cancellationToken))
            return EmailConflict();

        var createResult = Customer.Create(
            request.FirstName.Trim(),
            request.LastName.Trim(),
            email,
            passwordHasher.Hash(request.Password));

        if (createResult.IsFailure)
            return Result<RegisterResultDto>.Failure(createResult.Errors);

        var customer = createResult.Value!;
        customerRepository.Add(customer);

        try
        {
            await customerRepository.SaveChangesAsync(cancellationToken);
        }
        catch (Exception )   // adjust to your actual exception type / catch DbUpdateException and inspect
        {
            return EmailConflict();                  // concurrent registration lost the race
        }
  var code = RandomNumberGenerator.GetInt32(100_000, 1_000_000).ToString();

        await codeStore.StoreCodeAsync(email, code, CodeTtl, cancellationToken);
        await emailService.SendVerificationCodeAsync(email, code, cancellationToken);

        return Result<RegisterResultDto>.Success(
            new RegisterResultDto(customer.Id, customer.Email));
    }

    private static Result<RegisterResultDto> EmailConflict() =>
        Result<RegisterResultDto>.Failure(
            Error.Conflict("Email.Exists", "An account with this email already exists."));
}
