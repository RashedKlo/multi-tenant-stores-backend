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
        if (await customerRepository.EmailExistsAsync(request.Email, cancellationToken))
            return Result<RegisterResultDto>.Failure(
                Error.Conflict("Email", $"An account with email '{request.Email}' already exists."));

        var passwordHash = passwordHasher.Hash(request.Password);
        var customer = Customer.Create(
            request.FirstName, request.LastName, request.Email, passwordHash);
if(customer is null)
            return Result<RegisterResultDto>.Failure(
                Error.Validation("Customer.Invalid", "Failed to create customer."));
        customerRepository.Add(customer.Value!);
        await customerRepository.SaveChangesAsync(cancellationToken);

        var code = GenerateCode();
        await codeStore.StoreCodeAsync(customer.Value!.Email, code, CodeTtl, cancellationToken);
        await emailService.SendVerificationCodeAsync(customer.Value!.Email, code, cancellationToken);

        return Result<RegisterResultDto>.Success(
            new RegisterResultDto(customer.Value!.Id, customer.Value!.Email));
    }

    private static string GenerateCode() =>
        Random.Shared.Next(100_000, 999_999).ToString();
}
