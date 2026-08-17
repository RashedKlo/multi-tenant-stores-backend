using Application.Customers.DTOs;
using Domain.Common;
using MediatR;

namespace Application.Customers.Commands.UpdateProfile;

public record UpdateProfileCommand(
    string FirstName,
    string LastName) : IRequest<Result<CustomerDto>>;
