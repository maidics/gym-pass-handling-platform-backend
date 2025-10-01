using FitPass.Application.Common.Security;
using FitPass.Domain.Constants;

namespace Fitpass.Application.GymPassProducts.Commands;

[Authorize(Roles = Roles.GymAdministrator)]
public record CreateGymPassProductCommand
    (

    ) : IRequest;

public class CreateGymPassProductCommandValidator : AbstractValidator<CreateGymPassProductCommand>
{
    public CreateGymPassProductCommandValidator()
    {
        
    }
}