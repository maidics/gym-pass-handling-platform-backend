using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Logging;
using FitPass.Application.Common.Security;
using FitPass.Application.UserProfiles.DTOs;
using FitPass.Domain.Entities;
using FitPass.Domain.Strings;
using Microsoft.Extensions.Logging;

namespace FitPass.Application.UserProfiles.Queries;

[Authorize]
public record GetMyUserProfileQuery : IRequest<UserProfileWithEmailDto>;

public class GetMyUserProfileQueryHandler : IRequestHandler<GetMyUserProfileQuery, UserProfileWithEmailDto>
{
    private readonly IQueryService _queryService;
    private readonly IUser _user;
    private readonly ILogger<GetMyUserProfileQueryHandler> _logger;

    public GetMyUserProfileQueryHandler(IQueryService queryService, IUser user, ILogger<GetMyUserProfileQueryHandler> logger)
    {
        _queryService = queryService;
        _user = user;
        _logger = logger;
    }
    public async Task<UserProfileWithEmailDto> Handle(GetMyUserProfileQuery request, CancellationToken cancellationToken)
    {
        var userProfileWithEmailDto = await _queryService.GetUserProfileWithEmailByApplicationUserId(_user.Id!);

        if (userProfileWithEmailDto == null)
        {
            LogCriticalMessages.AuthenticatedUserRelatedEntityNotFound(_logger, _user.Roles, _user.Id, nameof(UserProfile));
            throw new Exception(ErrorMessages.AuthenticatedUserRelatedEntityNotFound(nameof(UserProfile)));
        }

        return userProfileWithEmailDto;
    }
}