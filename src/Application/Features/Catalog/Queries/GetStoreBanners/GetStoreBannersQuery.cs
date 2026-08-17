using Application.Catalog.DTOs;
using Domain.Common;
using MediatR;

namespace Application.Catalog.Queries.GetStoreBanners;

public record GetStoreBannersQuery(Guid StoreId) : IRequest<Result<List<StoreBannerDto>>>;
