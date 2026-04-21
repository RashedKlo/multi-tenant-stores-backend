using MediatR;

namespace Application.Departments.Commands.DeleteDepartment;

public record DeleteDepartmentCommand(Guid Id) : IRequest<bool>;
