using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Security;
using FitPass.Application.Extensions;
using FitPass.Application.NonRegisteredUsers.DTOs;
using FitPass.Domain.Constants;

namespace Fitpass.Application.NonRegisteredUsers.Queries;

[Authorize(Roles = $"{Roles.GymAdministrator},{Roles.GymStaff}")]
public record GetNonRegisteredUserQuery(string NonRegisteredUserId) : IRequest<NonRegisteredUserDto?>;

public class GetNonRegisteredUserQueryValidator : AbstractValidator<GetNonRegisteredUserQuery>
{
    public GetNonRegisteredUserQueryValidator()
    {
        RuleFor(v => v.NonRegisteredUserId).NotEmptyWithMessage("Non registered user id");
    }
}

public class GetNonRegisteredUserQueryHandler : IRequestHandler<GetNonRegisteredUserQuery, NonRegisteredUserDto?>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetNonRegisteredUserQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }
    public async Task<NonRegisteredUserDto?> Handle(GetNonRegisteredUserQuery query, CancellationToken cancellationToken)
    {
        var nonRegisteredUser = await _context
            .NonRegisteredUsers
            .AsNoTracking()
            .Include(nru => nru.UserGymMemberships)
            .FirstOrDefaultAsync(nru => nru.Id == query.NonRegisteredUserId, cancellationToken);

        return _mapper.Map<NonRegisteredUserDto>(nonRegisteredUser);
    }
}
