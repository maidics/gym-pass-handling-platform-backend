using FitPass.Application.Gyms.DTOs;
using FitPass.Application.Common.Extensions;
using FitPass.Application.Common.Interfaces;
using FitPass.Domain.Constants;
using FitPass.Application.Common.Models; 
using FitPass.Application.Common.Resources;

namespace FitPass.Application.Gyms.Queries;

public record GetGymByIdQuery(string GymId) : IRequest<Result<GymDto>>;

public class GetGymByIdQueryValidator : AbstractValidator<GetGymByIdQuery>
{
    public GetGymByIdQueryValidator(ILocalizer localizer)
    {
        RuleFor(v => v.GymId)
            .PropertyOfEntityNotEmptyWithMessageLocalized(localizer, nameof(SharedResource.Id), nameof(SharedResource.Gym));
    }
}

public class GetGymByIdQueryHandler : IRequestHandler<GetGymByIdQuery, Result<GymDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    private readonly ILocalizer _localizer;

    public GetGymByIdQueryHandler(
        IApplicationDbContext context,
        IUser user,
        ILocalizer localizer)
    {
        _context = context;
        _user = user;
        _localizer = localizer;
    }

    public async Task<Result<GymDto>> Handle(GetGymByIdQuery query, CancellationToken cancellationToken)
    {
        var gymQuery = _context
            .Gyms
            .Include(x => x.ContactInfos)
            .Include(x => x.PassProducts)
            .AsNoTracking();

        Guard.Against.NullParameterRelatedToCurrentUser(_user.Roles, "roles", _user.Id);

        if (_user.Roles.Contains(Roles.GymAdministrator))
        {
            var employmentGymId = await _context.GymEmployments
                .AsNoTracking()
                .Where(x => x.UserId == _user.Id)
                .Select(x => x.GymId)
                .FirstOrDefaultAsync(cancellationToken);

            if (employmentGymId == query.GymId)
            {
                gymQuery = gymQuery.Include(x => x.PaymentProfile);
            }
        }

        var gym = await gymQuery
            .FirstOrDefaultAsync(g => g.Id == query.GymId, cancellationToken);

        if (gym is null)
        {
            return Result.NotFound(_localizer.GetNotFound(nameof(SharedResource.Gym)));
        }

        return Result.Success(gym.MapToDto());
    }
}
