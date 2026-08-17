using Application.Discovery.DTOs;
using MediatR;
using Domain.Common;

namespace Application.Discovery.Queries.GetModuleDetail;

public record GetModuleDetailQuery(Guid ModuleId) : IRequest<Result<ModuleDetailDto>>;