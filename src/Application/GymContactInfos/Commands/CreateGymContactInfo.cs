using FitPass.Application.Common.Extensions;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Models;
using FitPass.Application.Common.Resources;
using FitPass.Application.Common.Security;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;
using FitPass.Domain.ValueObjects;

namespace FitPass.Application.GymContactInfos.Commands;

[Authorize(Roles = Roles.GymAdministrator)]
public record CreateGymContactInfoCommand(string? PhoneNumber, string? Email, string FullName, Address? Address) : IRequest<Result>;

public class CreateGymContactInfoDtoValidator : AbstractValidator<CreateGymContactInfoCommand>
{
    public CreateGymContactInfoDtoValidator(ILocalizer localizer)
    {
        When(x => x.Email is null, () =>
        {
            RuleFor(v => v.PhoneNumber)
                .NotNull().WithMessage(localizer.Get(nameof(SharedResource.PropertyIsRequired),
                    localizer.Get(nameof(SharedResource.EmailOrPhoneNumber))));
        });

        When(v => v.Email is null, () =>
        {
            RuleFor(v => v.PhoneNumber!).NotEmptyWithMessageLocalized(localizer, nameof(SharedResource.PhoneNumber));
        });
        
        When(v => v.PhoneNumber is null, () =>
        {
            RuleFor(v => v.Email!).EmailAddressWithMessageLocalized(localizer);
        });
        
        When(v => v.PhoneNumber is null, () =>
        {
            RuleFor(v => v.Email!).EmailAddressWithMessageLocalized(localizer);
        });

        When(v => v.Email is not null, () =>
        {
            RuleFor(v => v.Email!).EmailAddressWithMessageLocalized(localizer);
        });

        RuleFor(v => v.FullName)
            .NotEmptyWithMaxLengthAndMessageLocalized(localizer, nameof(SharedResource.FullName), MaxLengths.FullName);
    }
}

public class CreateGymContactInfoCommandHandler : IRequestHandler<CreateGymContactInfoCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public CreateGymContactInfoCommandHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }
    
    public async Task<Result> Handle(CreateGymContactInfoCommand command, CancellationToken cancellationToken)
    {
        var gym = await _context.GymEmployments
            .Where(x => x.UserId == _user.Id)
            .Include(x => x.Gym)
            .Select(x => x.Gym)
            .FirstOrDefaultAsync(cancellationToken);

        Guard.Against.NullParameterRelatedToCurrentUser(gym, nameof(Gym), _user.Id);

        gym.ContactInfos.Add(new GymContactInfo
        {
            Address = command.Address, 
            Email = command.Email, 
            PhoneNumber = command.PhoneNumber is null ? null : PhoneNumber.Create(command.PhoneNumber), 
            FullName = command.FullName
        });

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
