using Application.Discovery.DTOs;
using Domain.Common;
using MediatR;

namespace Application.Discovery.Queries.GetModules;

public record GetModulesQuery : IRequest<Result<List<ModuleDto>>>;