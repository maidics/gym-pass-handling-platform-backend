using FitPass.Application.Common.Extensions;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Security;
using FitPass.Domain.Constants;

namespace FitPass.Application.GymMembershipPasses.Queries;

[Authorize(Roles = $"{Roles.GymAdministrator},{Roles.GymStaff}")]
public record IsGymMembershipPassValidQuery(string GymMembershipPassId) : IRequest<bool>;

public class IsGymMembershipPassValidQueryValidator : AbstractValidator<IsGymMembershipPassValidQuery>
{
    public IsGymMembershipPassValidQueryValidator()
    {
        RuleFor(v => v.GymMembershipPassId).NotEmptyWithMessage(nameof(IsGymMembershipPassValidQuery.GymMembershipPassId));
    }
}

public class IsGymMembershipPassValidQueryHandler : IRequestHandler<IsGymMembershipPassValidQuery, bool>
{
    private readonly IApplicationDbContext _context;

    public IsGymMembershipPassValidQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(IsGymMembershipPassValidQuery query, CancellationToken cancellationToken)
    {
        var pass = await _context
            .GymMembershipPasses
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == query.GymMembershipPassId);

        Guard.Against.NotFound(query.GymMembershipPassId, pass);

        return !pass.IsExpired() && !pass.HasNoUsesLeft();
    }
}
