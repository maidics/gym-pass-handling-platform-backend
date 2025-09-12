using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Security;
using FitPass.Domain.Constants;

namespace FitPass.Application.Gyms.Queries;

[Authorize(Roles = $"{Roles.GymAdministrator},{Roles.GymStaff}")]
public record GetMyGymQrCodeQuery : IRequest<byte[]?>;

public class GetMyGymQrCodeQueryHandler : IRequestHandler<GetMyGymQrCodeQuery, byte[]?>
{
    private readonly IUserProfileService _userProfileService;

    public GetMyGymQrCodeQueryHandler(IUserProfileService userProfileService)
    {
        _userProfileService = userProfileService;
    }

    public async Task<byte[]?> Handle(GetMyGymQrCodeQuery request, CancellationToken cancellationToken)
    {
        var gymStaffAssigment = await _userProfileService.GetUserGymStaffAssigment(cancellationToken);

        return gymStaffAssigment == null ? null : gymStaffAssigment.Gym.QRCode;
    }
}