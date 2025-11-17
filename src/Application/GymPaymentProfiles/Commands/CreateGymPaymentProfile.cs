using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Security;
using FitPass.Domain.Constants;

namespace FitPass.Application.GymPaymentProfiles.Commands;

[Authorize(Roles = Roles.GymAdministrator)]
public class CreateGymPaymentProfileCommand(

) : IRequest;

public class CreateGymPaymentProfileCommandHandler : IRequestHandler<CreateGymPaymentProfileCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IPaymentTenantService _paymentTenantService;
    private readonly IUser _user;

    

    public async Task Handle(CreateGymPaymentProfileCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}