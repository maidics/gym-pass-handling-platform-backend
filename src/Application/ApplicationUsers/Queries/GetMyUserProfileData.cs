using Fitpass.Application.ApplicationUsers.DTOs;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Security;

namespace Fitpass.Application.ApplicationUsers.Queries;

[Authorize]
public record GetMyUserProfileDataQuery : IRequest<ApplicationUserProfileDataDto>;

public class GetMyUserProfileDataQueryHandler : IRequestHandler<GetMyUserProfileDataQuery, ApplicationUserProfileDataDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    private readonly IMapper _mapper;

    public GetMyUserProfileDataQueryHandler(IApplicationDbContext context, IUser user, IMapper mapper)
    {
        _context = context;
        _user = user;
        _mapper = mapper;
    }
    public async Task<ApplicationUserProfileDataDto> Handle(GetMyUserProfileDataQuery request, CancellationToken cancellationToken)
    {
        var user = await _context
            .ApplicationUsers
            .AsNoTracking()
            .FirstOrDefaultAsync(au => au.Id == _user.Id!, cancellationToken);

        if (user == null)
        {
            throw new UnauthorizedAccessException();
        }

        return _mapper.Map<ApplicationUserProfileDataDto>(user);
    }
}