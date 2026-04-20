using Application.Stores.DTOs;
using MediatR;

namespace Application.Stores.Commands.UpdateStore;

public record UpdateStoreCommand
(
    Guid Id,
    string Name,
    string? Description,
    string? LogoUrl,
    string? BannerUrl,
    bool IsActive
    ) : IRequest<StoreDto>;
