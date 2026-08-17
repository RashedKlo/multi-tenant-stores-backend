using FluentValidation;

namespace Application.Auth.Commands.CreateGuestSession;

public class CreateGuestSessionValidator : AbstractValidator<CreateGuestSessionCommand>
{
    public CreateGuestSessionValidator()
    {
        
        // No inputs — intentionally empty.
    }
}
