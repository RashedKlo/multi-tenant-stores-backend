using Application.Discovery.DTOs;
using Domain.Common;
using MediatR;

namespace Application.Discovery.Queries.GetHomeBanners;

public record GetHomeBannersQuery : IRequest<Result<List<HomeBannerDto>>>;