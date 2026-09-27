using Application.Features.Addresses.DTOs;
using Domain.Common;
using MediatR;

namespace Application.Addresses.Queries.GetDefaultAddress;

public record GetDefaultAddressQuery : IRequest<Result<DefaultAddressDto>>;