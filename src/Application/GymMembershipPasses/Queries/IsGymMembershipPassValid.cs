using FitPass.Application.Common.Extensions;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Models;
using FitPass.Application.Common.Security;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;

namespace FitPass.Application.GymMembershipPasses.Queries;

[Authorize(Roles = $"{Roles.GymAdministrator},{Roles.GymStaff}")]
public record IsGymMembershipPassValidQuery(string GymMembershipPassId) : IRequest<Result<bool>>;

public class IsGymMembershipPassValidQueryValidator : AbstractValidator<IsGymMembershipPassValidQuery>
{
    public IsGymMembershipPassValidQueryValidator()
    {
        RuleFor(v => v.GymMembershipPassId).NotEmptyWithMessage(nameof(IsGymMembershipPassValidQuery.GymMembershipPassId));
    }
}

public class IsGymMembershipPassValidQueryHandler : IRequestHandler<IsGymMembershipPassValidQuery, Result<bool>>
{
    private readonly IApplicationDbContext _context;
    private readonly TimeProvider _timeProvider;

    public IsGymMembershipPassValidQueryHandler(
        IApplicationDbContext context,
        TimeProvider timeProvider)
    {
        _context = context;
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
            return Result.NotFound(nameof(GymPassProduct));
        }

        return Result.Success(pass.IsValid(_timeProvider.GetUtcNow()));
    }
}
