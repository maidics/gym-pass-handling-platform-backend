using Fitpass.Application.Common.Exceptions;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Security;
using FitPass.Application.Extensions;
using FitPass.Application.NonRegisteredUsers.DTOs;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;
using FitPass.Domain.Strings;

namespace FitPass.Application.NonRegisteredUsers.Commands;

[Authorize(Roles = $"{Roles.GymAdministrator},{Roles.GymStaff}")]
public record CreateNonRegisteredUserCommand (
   string? Email, 
   string? PhoneNumber,
   string FirstName,
   string LastName
) : IRequest<NonRegisteredUserDto>;

public class CreateNonRegisteredUserCommandValidator : AbstractValidator<CreateNonRegisteredUserCommand>
{
    public CreateNonRegisteredUserCommandValidator()
    {
        RuleFor(v => v).Must(v => !string.IsNullOrEmpty(v.Email) || !string.IsNullOrEmpty(v.PhoneNumber));

        When(v => string.IsNullOrEmpty(v.Email), () =>
        {
            RuleFor(v => v.PhoneNumber)
                .NotNull()
                .WithMessage(ErrorMessages.PropertyCannotBeNullIfAnotherIsNull(nameof(CreateNonRegisteredUserCommand.PhoneNumber), nameof(CreateNonRegisteredUserCommand.Email)));
        });

        When(v => string.IsNullOrEmpty(v.PhoneNumber), () =>
        {
            RuleFor(v => v.Email)
                .NotNull()
                .WithMessage(ErrorMessages.PropertyCannotBeNullIfAnotherIsNull(nameof(CreateNonRegisteredUserCommand.Email), nameof(CreateNonRegisteredUserCommand.PhoneNumber)));
        });

        When(v => !string.IsNullOrEmpty(v.Email), () =>
        {
            RuleFor(v => v.Email!)
                .MaxLengthWithMessage(nameof(CreateNonRegisteredUserCommand.Email), MaxStringLengths.Email)
                .ValidEmailAddress(nameof(CreateNonRegisteredUserCommand.Email));
        });

        When(v => !string.IsNullOrEmpty(v.PhoneNumber), () =>
        {
            RuleFor(v => v.PhoneNumber!).PhoneNumber(nameof(CreateNonRegisteredUserCommand.PhoneNumber));
        });

        RuleFor(v => v.FirstName).NotEmptyWithMaxLenghtAndMessage(nameof(CreateNonRegisteredUserCommand.FirstName), MaxStringLengths.Name);

        RuleFor(v => v.LastName).NotEmptyWithMaxLenghtAndMessage(nameof(CreateNonRegisteredUserCommand.LastName), MaxStringLengths.Name);
    }
}

public class CreateNonRegisteredUserCommandHandler : IRequestHandler<CreateNonRegisteredUserCommand, NonRegisteredUserDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    private readonly IMapper _mapper;
    private readonly IStripeCustomerService _stripeCustomerService;

    public CreateNonRegisteredUserCommandHandler(IApplicationDbContext context, IUser user, IMapper mapper, IStripeCustomerService stripeCustomerService)
    {
        _context = context;
        _user = user;
        _mapper = mapper;
        _stripeCustomerService = stripeCustomerService;
    }

    public async Task<NonRegisteredUserDto> Handle(CreateNonRegisteredUserCommand command, CancellationToken cancellationToken)
    {
        var gymStaffAssignment = await _context.GymEmployments
            .AsNoTracking()
            .FirstOrDefaultAsync(gsa => gsa.ApplicationUserId == _user.Id, cancellationToken);

        Guard.Against.Null(gymStaffAssignment, "Id", "Failed to find the current Gym Admin or Gym Staff member.");

        var existingNonRegisteredUser = await _context
            .NonRegisteredUsers
            .AsNoTracking()
            .FirstOrDefaultAsync(nru => nru.Email == command.Email || nru.PhoneNumber == command.PhoneNumber, cancellationToken);

        if (existingNonRegisteredUser != null && command.Email != null && existingNonRegisteredUser.Email == command.Email)
        {
            throw new ConflictException("This email is already in use.");
        }

        if (existingNonRegisteredUser != null && command.PhoneNumber != null && existingNonRegisteredUser.PhoneNumber == command.PhoneNumber)
        {
            throw new ConflictException("This phone number is already in use.");
        }

        var existingUser = await _context
            .ApplicationUsers
            .AsNoTracking()
            .FirstOrDefaultAsync(au => au.Email == command.Email || au.PhoneNumber == command.PhoneNumber, cancellationToken);

        if (existingUser != null && command.Email != null && existingUser.Email == command.Email)
        {
            throw new ConflictException("This email is already in use.");
        }

        if (existingUser != null && command.PhoneNumber != null && existingUser.PhoneNumber == command.PhoneNumber)
        {
            throw new ConflictException("This phone number is already in use.");
        }

        var nonRegisteredUser = new NonRegisteredUser
        {
            Email = command.Email,
            PhoneNumber = command.PhoneNumber,
            FirstName = command.FirstName,
            LastName = command.LastName
        };

        var userGymMembership = new UserGymMembership
        {
            ApplicationUserId = null,
            NonRegisteredUserId = nonRegisteredUser.Id,
            GymId = gymStaffAssignment.GymId!
        };

        await _stripeCustomerService.CreateCustomer(nonRegisteredUser);

        nonRegisteredUser.UserGymMemberships.Add(userGymMembership);

        await _context.NonRegisteredUsers.AddAsync(nonRegisteredUser, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return _mapper.Map<NonRegisteredUserDto>(nonRegisteredUser);
    }
}
