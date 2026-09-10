// Application/Features/Customers/Commands/UpdateProfile/UpdateProfileCommand.cs
using Application.Customers.DTOs;
using Domain.Common;
using MediatR;

namespace Application.Customers.Commands.UpdateProfile;

public sealed record UpdateProfileCommand(
    string FirstName,
    string LastName) : IRequest<Result<CustomerDto>>;