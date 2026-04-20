using MediatR;

namespace Application.Tenants.Commands.DeleteTenant;

public record DeleteTenantCommand(Guid Id) : IRequest<bool>;
