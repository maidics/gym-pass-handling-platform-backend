using FitPass.Application.Common.Extensions;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Security;
using FitPass.Application.GymEmployments.DTOs;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace FitPass.Application.GymEmployments.Queries;

[Authorize(Roles = $"{Roles.GymAdministrator},{Roles.GymStaff}")]
public record GetMyGymEmploymentQuery : IRequest<GymEmploymentDto>;

public class GetMyGymEmploymentQueryHandler : IRequestHandler<GetMyGymEmploymentQuery, GymEmploymentDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    private readonly ILogger<GetMyGymEmploymentQueryHandler> _logger;
    private readonly IQueryService _queryService;

    public GetMyGymEmploymentQueryHandler(
        IApplicationDbContext context,
        IUser user,
        ILogger<GetMyGymEmploymentQueryHandler> logger,
        IQueryService queryService)
    {
        _context = context;
        _user = user;
        _logger = logger;
        _queryService = queryService;
    }

    public async Task<GymEmploymentDto> Handle(GetMyGymEmploymentQuery request, CancellationToken cancellationToken)
    {
        var gymEmploymentDto = await _queryService.GetGymEmploymentWithUserProfileAndEmailByUserId(_user.Id!);

        Guard.Against.NullParameterRelatedToCurrentUser(gymEmploymentDto, nameof(GymEmployment), _user.Id);

        return gymEmploymentDto;
    }
}