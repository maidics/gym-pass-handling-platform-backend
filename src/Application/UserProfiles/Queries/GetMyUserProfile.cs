using FitPass.Application.Common.Extensions;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Security;
using FitPass.Application.UserProfiles.DTOs;
using FitPass.Domain.Entities;

namespace FitPass.Application.UserProfiles.Queries;

[Authorize]
public record GetMyUserProfileQuery : IRequest<UserProfileWithEmailDto>;

public class GetMyUserProfileQueryHandler : IRequestHandler<GetMyUserProfileQuery, UserProfileWithEmailDto>
{
    private readonly IQueryService _queryService;
    private readonly IUser _user;

    public GetMyUserProfileQueryHandler(
        IQueryService queryService, 
        IUser user)
    {
        _queryService = queryService;
        _user = user;
    }
    public async Task<UserProfileWithEmailDto> Handle(GetMyUserProfileQuery request, CancellationToken cancellationToken)
    {
        var profile = await _queryService.GetUserProfileWithEmailByApplicationUserId(_user.Id!);

        Guard.Against.NullParameterRelatedToCurrentUser(profile, nameof(UserProfile), _user.Id);

        return profile;
    }
}
