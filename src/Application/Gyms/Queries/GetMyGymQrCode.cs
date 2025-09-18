using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Security;
using FitPass.Domain.Constants;

namespace FitPass.Application.Gyms.Queries;

[Authorize(Roles = $"{Roles.GymAdministrator},{Roles.GymStaff}")]
public record GetMyGymQrCodeQuery : IRequest<byte[]>;

public class GetMyGymQrCodeQueryHandler : IRequestHandler<GetMyGymQrCodeQuery, byte[]>
{
    private readonly IUser _user;
    private readonly IApplicationDbContext _context;
    private readonly IQrCodeService _qrCodeService;

    public GetMyGymQrCodeQueryHandler(IUser user, IApplicationDbContext context, IQrCodeService qrCodeService)
    {
        _user = user;
        _context = context;
        _qrCodeService = qrCodeService;
    }

    public async Task<byte[]> Handle(GetMyGymQrCodeQuery query, CancellationToken cancellationToken)
    {
        var gymStaffAssigment = await _context
            .GymStaffAssigments
            .AsNoTracking()
            .FirstOrDefaultAsync(gsa => gsa.ApplicationUserId == _user.Id, cancellationToken);

        return _qrCodeService.GetQrCode(gymStaffAssigment!.GymId);
    }
}