using FitPass.Application.Common.Extensions;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Models;
using FitPass.Application.Common.Resources;
using FitPass.Application.Common.Security;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;

namespace FitPass.Application.GymContactInfos.Commands;

[Authorize(Roles = Roles.GymAdministrator)]
public record DeleteGymContactInfoCommand(string GymContactInfoId) : IRequest<Result>;

public class DeleteGymContactInfoCommandValidator : AbstractValidator<DeleteGymContactInfoCommand>
{
    public DeleteGymContactInfoCommandValidator(ILocalizer localizer)
    {
        RuleFor(v => v.GymContactInfoId)
            .PropertyOfEntityNotEmptyWithMessageLocalized(
                localizer, nameof(SharedResource.Id), nameof(SharedResource.ContactInfo));
    }
}

public class DeleteGymContactInfoCommandHandler : IRequestHandler<DeleteGymContactInfoCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    private readonly ILocalizer _localizer;

    public DeleteGymContactInfoCommandHandler(
        IApplicationDbContext context,
        IUser user,
        ILocalizer localizer)
    {
        _context = context;
        _user = user;
        _localizer = localizer;
    }
    
    public async Task<Result> Handle(DeleteGymContactInfoCommand command, CancellationToken cancellationToken)
    {
        var gymId = await _context.GymEmployments
            .AsNoTracking()
            .Where(x => x.UserId == _user.Id)
            .Select(x => x.GymId)
            .FirstOrDefaultAsync(cancellationToken);

        Guard.Against.NullParameterRelatedToCurrentUser(
            gymId, $"{nameof(GymEmployment)}.{nameof(GymEmployment.GymId)}", _user.Id);

        var contactInfo = await _context.Gyms
            .Where(x => x.Id == gymId)
            .Include(x => x.ContactInfos.Where(x => x.Id == command.GymContactInfoId))
            .Select(x => x.ContactInfos.FirstOrDefault())
            .FirstOrDefaultAsync(cancellationToken);

        if (contactInfo is null)
        {
            return Result.NotFound(_localizer.GetNotFound(nameof(SharedResource.ContactInfo)));
        }

        _context.GymContactInfos.Remove(contactInfo);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
