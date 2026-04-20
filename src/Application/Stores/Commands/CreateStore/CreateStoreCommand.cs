using Application.Stores.DTOs;
using MediatR;

namespace Application.Stores.Commands.CreateStore;

public record CreateStoreCommand
(Guid TenantId,
 string Name,
 string? Description,
 string? LogoUrl,
 string? BannerUrl
    )
: IRequest<StoreDto>;
