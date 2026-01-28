using FitPass.Application.Common.Extensions;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Models;
using FitPass.Application.Common.Resources;
using FitPass.Application.Common.Security;
using FitPass.Domain.Constants;
using FitPass.Domain.ValueObjects;

namespace FitPass.Application.GymContactInfos.Commands;

[Authorize(Roles = Roles.GymAdministrator)]
public record UpdateGymContactInfoCommand(
    string GymContactInfoId, string? Email, PhoneNumber? PhoneNumber, string FullName, Address? Address) : IRequest<Result>;

public class UpdateGymContactInfoCommandValidator : AbstractValidator<UpdateGymContactInfoCommand>
{
    public UpdateGymContactInfoCommandValidator(ILocalizer localizer)
    {
        RuleFor(v => v.GymContactInfoId)
            .PropertyOfEntityNotEmptyWithMessageLocalized(localizer, nameof(SharedResource.Id),
                nameof(SharedResource.ContactInfo));
        
        When(v => v.Email is null, () =>
        {
            RuleFor(v => v.PhoneNumber!).NotEmptyWithMessageLocalized(localizer, nameof(SharedResource.PhoneNumber));
        });
        
        When(v => v.PhoneNumber is null, () =>
        {
            RuleFor(v => v.Email!).EmailAddressWithMessageLocalized(localizer);
        });
        
        RuleFor(v => v.FullName)
            .NotEmptyWithMaxLengthAndMessageLocalized(localizer, nameof(SharedResource.FullName), MaxLengths.FullName);
    }
}

public class UpdateGymContactInfoCommandHandler : IRequestHandler<UpdateGymContactInfoCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    private readonly ILocalizer _localizer;

    public UpdateGymContactInfoCommandHandler(
        IApplicationDbContext context, 
        IUser user,
        ILocalizer localizer)
    {
        _context = context;
        _user = user;
        _localizer = localizer;
        
    }
    
    public async Task<Result> Handle(UpdateGymContactInfoCommand command, CancellationToken cancellationToken)
    {
        var contactInfo = await _context.GymEmployments
            .Where(x => x.UserId == _user.Id)
            .Include(x => x.Gym)
            .ThenInclude(x => x.ContactInfos)
            .Select(x => x.Gym.ContactInfos.FirstOrDefault(y => y.Id == command.GymContactInfoId))
            .FirstOrDefaultAsync();

        if (contactInfo is null)
        {
            return Result.NotFound(_localizer.GetNotFound(nameof(SharedResource.ContactInfo)));
        }

        contactInfo.FullName = command.FullName;
        contactInfo.Email = command.Email;
        contactInfo.PhoneNumber = command.PhoneNumber;
        contactInfo.Address = command.Address;

        await _context.SaveChangesAsync();

        return Result.Success();
    }
}
