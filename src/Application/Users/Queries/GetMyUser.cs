using FitPass.Application.Common.Extensions;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Security;
using FitPass.Application.Users.DTOs;

namespace FitPass.Application.Users.Queries;

[Authorize]
public record GetMyUserQuery : IRequest<UserDto>;

public class GetMyUserQueryHandler : IRequestHandler<GetMyUserQuery, UserDto>
{
    private readonly IQueryService _queryService;
    private readonly IUser _user;
    
    public GetMyUserQueryHandler(IQueryService queryService,  IUser user)
    {
        _queryService = queryService;
        _user = user;
    }
    public async Task<UserDto> Handle(GetMyUserQuery request, CancellationToken cancellationToken)
    {
        var user = await _queryService.GetUserAsync(_user.Id!);

        Guard.Against.NullParameterRelatedToCurrentUser(user, nameof(UserDto), _user.Id);
        
        return user!;
    }
}
