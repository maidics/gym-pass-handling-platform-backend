using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Security;
using FitPass.Application.Extensions;
using FitPass.Application.NonRegisteredUsers.DTOs;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;

namespace FitPass.Application.NonRegisteredUsers.Commands;

[Authorize(Roles = $"{Roles.GymAdministrator},{Roles.GymStaff}")]
public record CreateNonRegisteredUserCommand (
   string? Email, 
   string? PhoneNumber,
   string FirstName,
   string? LastName
) : IRequest<NonRegisteredUserDto>;

public class CreateNonRegisteredUserCommandValidator : AbstractValidator<CreateNonRegisteredUserCommand>
{
    public CreateNonRegisteredUserCommandValidator()
    {
        RuleFor(v => v).Must(v => !string.IsNullOrEmpty(v.Email) || !string.IsNullOrEmpty(v.PhoneNumber));

        RuleFor(v => v.Email)
            .EmailAddress()
            .NotEmptyWithMaxLenghtAndMessage(MaxStringLengths.Email, "Email")
            .When(v => !string.IsNullOrEmpty(v.Email));

        When(v => !string.IsNullOrEmpty(v.PhoneNumber), () =>
        {
            RuleFor(v => v.PhoneNumber!).PhoneNumber("Phone number");
        });

        RuleFor(v => v.FirstName).NotEmptyWithMaxLenghtAndMessage(MaxStringLengths.Name, "First name");

        When(v => !string.IsNullOrEmpty(v.LastName), () =>
        {
            RuleFor(v => v.LastName!).NotEmptyWithMaxLenghtAndMessage(MaxStringLengths.Name, "Last name");
        });
    }
}

public class CreateNonRegisteredUserCommandHandler : IRequestHandler<CreateNonRegisteredUserCommand, NonRegisteredUserDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public CreateNonRegisteredUserCommandHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<NonRegisteredUserDto> Handle(CreateNonRegisteredUserCommand command, CancellationToken cancellationToken)
    {
        var nonRegisteredUser = new NonRegisteredUser
        {
            Id = Guid.NewGuid().ToString(),
            Email = command.Email,
            PhoneNumber = command.PhoneNumber,
            FirstName = command.FirstName,
            LastName = command.LastName
        };

        await _context.NonRegisteredUsers.AddAsync(nonRegisteredUser, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return _mapper.Map<NonRegisteredUserDto>(nonRegisteredUser);
    }
}
