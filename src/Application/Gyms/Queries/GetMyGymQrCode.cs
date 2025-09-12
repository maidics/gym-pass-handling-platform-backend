using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Security;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;

namespace FitPass.Application.Gyms.Queries;

[Authorize(Roles = $"{Roles.GymAdministrator},{Roles.GymStaff}")]
public record GetMyGymQrCodeQuery : IRequest<byte[]>;

public class GetMyGymQrCodeQueryHandler : IRequestHandler<GetMyGymQrCodeQuery, byte[]>
{
    private readonly IUser _user;
    private readonly IApplicationDbContext _context;

    public GetMyGymQrCodeQueryHandler(IUser user, IApplicationDbContext context)
    {
        _user = user;
        _context = context;
    }

    public async Task<byte[]> Handle(GetMyGymQrCodeQuery request, CancellationToken cancellationToken)
    {
        GymStaffAssigment gymStaffAssigment = await _context.GymStaffAssigments.FirstAsync(gsa => gsa.ApplicationUserId == _user.Id, cancellationToken);

        return gymStaffAssigment.Gym.QRCode;
    }
}