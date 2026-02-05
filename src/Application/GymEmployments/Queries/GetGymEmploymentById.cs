using FitPass.Application.Common.Extensions;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Models;
using FitPass.Application.Common.Resources;
using FitPass.Application.Common.Security;
using FitPass.Application.GymEmployments.DTOs;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;

namespace FitPass.Application.GymEmployments.Queries;

[Authorize(Roles = $"{Roles.GymAdministrator},{Roles.GymStaff}")]
public record GetGymEmploymentByIdQuery(string GymEmploymentId) : IRequest<Result<GymEmploymentDto>>;

public class GetGymEmploymentByIdQueryValidator : AbstractValidator<GetGymEmploymentByIdQuery>
{
    public GetGymEmploymentByIdQueryValidator(ILocalizer localizer)
    {
        RuleFor(v => v.GymEmploymentId)
            .NotEmpty()
            .WithMessage(localizer.GetPropertyOfEntityIsRequired(nameof(SharedResource.Id), nameof(SharedResource.GymEmployment)));
    }
}

public class GetGymEmploymentByIdQueryHandler : IRequestHandler<GetGymEmploymentByIdQuery, Result<GymEmploymentDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    private readonly IQueryService  _queryService;
    private readonly ILocalizer _localizer;

    public GetGymEmploymentByIdQueryHandler(IApplicationDbContext context, IUser user, IQueryService queryService, ILocalizer localizer)
    {
        _context = context;
        _user = user;
        _queryService = queryService;
        _localizer = localizer;
    }
    
    public async Task<Result<GymEmploymentDto>> Handle(GetGymEmploymentByIdQuery query, CancellationToken cancellationToken)
    {
        var dto = await _queryService.GetGymEmploymentWithUserProfileAndEmailByIdAsync(query.GymEmploymentId, cancellationToken);

        if (dto is null)
        {
            return Result.NotFound(_localizer.GetNotFound(nameof(SharedResource.GymEmployment)));
        }
        
        if (!_user.Roles!.Contains(Roles.AppAdministrator))
        {
            var ownEmployment = await _context.GymEmployments
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.UserId == _user.Id, cancellationToken);
            
            Guard.Against.NullParameterRelatedToCurrentUser(ownEmployment, nameof(GymEmployment), _user.Id);

            if (dto.GymId != ownEmployment.GymId)
            {
                return Result.NotFound(_localizer.GetNotFound(nameof(SharedResource.GymEmployment)));
            }
        }
        
        return Result.Success(dto);
    }
} 
