using FitPass.Application.Common.Extensions;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Security;
using FitPass.Application.GymEmployments.DTOs;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;

namespace FitPass.Application.GymEmployments.Queries;

[Authorize(Roles = $"{Roles.GymAdministrator},{Roles.GymStaff}")]
public record GetMyGymEmploymentQuery : IRequest<GymEmploymentDto>;

public class GetMyGymEmploymentQueryHandler : IRequestHandler<GetMyGymEmploymentQuery, GymEmploymentDto>
{
    private readonly IUser _user;
    private readonly IQueryService _queryService;

    public GetMyGymEmploymentQueryHandler(
        IUser user,
        IQueryService queryService)
    {
        _user = user;
        _queryService = queryService;
    }

    public async Task<GymEmploymentDto> Handle(GetMyGymEmploymentQuery request, CancellationToken cancellationToken)
    {
        var gymEmploymentDto = await _queryService.GetGymEmploymentWithUserProfileAndEmailByUserId(_user.Id!);

        Guard.Against.NullParameterRelatedToCurrentUser(gymEmploymentDto, nameof(GymEmployment), _user.Id);

        return gymEmploymentDto;
    }
}
