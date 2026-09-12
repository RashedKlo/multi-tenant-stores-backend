// StartConversationValidator.cs
using FluentValidation;

namespace Application.Features.Support.Commands.StartConversation;

public sealed class StartConversationValidator : AbstractValidator<StartConversationCommand>
{
    public StartConversationValidator() => RuleFor(x => x.TenantId).NotEmpty();
}