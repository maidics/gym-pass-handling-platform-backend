using FitPass.Application.Common.Extensions;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Models;
using FitPass.Application.Common.Security;
using FitPass.Domain.Constants;
using FitPass.Infrastructure.Localization.Resources;

namespace FitPass.Application.GymMembershipPasses.Queries;

[Authorize(Roles = $"{Roles.GymAdministrator},{Roles.GymStaff}")]
public record IsGymMembershipPassValidQuery(string GymMembershipPassId) : IRequest<Result<bool>>;

public class IsGymMembershipPassValidQueryValidator : AbstractValidator<IsGymMembershipPassValidQuery>
{
    public IsGymMembershipPassValidQueryValidator(ILocalizer localizer)
    {
        RuleFor(v => v.GymMembershipPassId)
            .PropertyOfEntityNotEmptyWithMessageLocalized(localizer, nameof(SharedResource.Id), nameof(SharedResource.GymMembershipPass));
    }
}

public class IsGymMembershipPassValidQueryHandler : IRequestHandler<IsGymMembershipPassValidQuery, Result<bool>>
{
    private readonly IApplicationDbContext _context;
    private readonly ILocalizer _localizer;
    private readonly TimeProvider _timeProvider;

    public IsGymMembershipPassValidQueryHandler(
        IApplicationDbContext context,
        ILocalizer localizer,
        TimeProvider timeProvider)
    {
        _context = context;
        _localizer = localizer;
        _timeProvider = timeProvider;
    }

    public async Task<Result<bool>> Handle(IsGymMembershipPassValidQuery query, CancellationToken cancellationToken)
    {
        var pass = await _context
            .GymMembershipPasses
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == query.GymMembershipPassId);

        if (pass is null)
        {
            return Result.NotFound(_localizer.GetNotFound(nameof(SharedResource.GymPassProduct)));
        }

        return Result.Success(pass.IsValid(_timeProvider.GetUtcNow()));
    }
}
