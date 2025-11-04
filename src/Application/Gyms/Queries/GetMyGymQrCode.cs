using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Logging;
using FitPass.Application.Common.Security;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;
using FitPass.Domain.Strings;
using Microsoft.Extensions.Logging;

namespace FitPass.Application.Gyms.Queries;

[Authorize(Roles = $"{Roles.GymAdministrator},{Roles.GymStaff}")]
public record GetMyGymQrCodeQuery : IRequest<byte[]>;

public class GetMyGymQrCodeQueryHandler : IRequestHandler<GetMyGymQrCodeQuery, byte[]>
{
    private readonly IUser _user;
    private readonly IApplicationDbContext _context;
    private readonly ILogger<GetMyGymQrCodeQuery> _logger;
    private readonly IQrCodeService _qrCodeService;

    public GetMyGymQrCodeQueryHandler(
        IUser user,
        IApplicationDbContext context,
        ILogger<GetMyGymQrCodeQuery> logger,
        IQrCodeService qrCodeService)
    {
        _user = user;
        _context = context;
        _logger = logger;
        _qrCodeService = qrCodeService;
    }

    public async Task<byte[]> Handle(GetMyGymQrCodeQuery query, CancellationToken cancellationToken)
    {
        var gymEmployment = await _context
            .GymEmployments
            .AsNoTracking()
            .FirstOrDefaultAsync(gsa => gsa.ApplicationUserId == _user.Id, cancellationToken);

        if (gymEmployment is null)
        {
            LogCriticalMessages.AuthenticatedUserRelatedEntityNotFound(
                _logger,
                _user.Roles,
                _user.Id,
                nameof(GymEmployment));

            throw new Exception(ErrorMessages.AuthenticatedUserRelatedEntityNotFound(nameof(GymEmployment)));
        }

        return _qrCodeService.GetQrCode(gymEmployment.GymId!);
    }
}
