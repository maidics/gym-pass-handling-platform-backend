using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Models;
using FitPass.Application.Common.Security;
using FitPass.Application.Extensions;
using FitPass.Application.Requests.DTOs;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;
using FitPass.Domain.Enums;

namespace FitPass.Application.Gyms.Commands;

[Authorize(Roles = Roles.AppAdministrator)]
public record RegisterGymCommand(
    string gymCreationRequestId
) : IRequest<Result>;

public class RegisterGymCommandValidator : AbstractValidator<RegisterGymCommand>
{
    public RegisterGymCommandValidator()
    {
        RuleFor(v => v.gymCreationRequestId).NotEmptyWithMessage("Gym creation request id");
    }
}

public class RegisterGymCommandHandler : IRequestHandler<RegisterGymCommand, Result>
{
    private readonly IIdentityService _identityService;
    private readonly IApplicationDbContext _context;
    private readonly IQrCodeService _qrCodeService;

    public RegisterGymCommandHandler(IIdentityService identityService, IApplicationDbContext context, IQrCodeService qrCodeService)
    {
        _identityService = identityService;
        _context = context;
        _qrCodeService = qrCodeService;
    }

    public async Task<Result> Handle(RegisterGymCommand request, CancellationToken cancellationToken)
    {
        await using var transaction = await _context.BeginTransactionAsync(cancellationToken);

        try
        {
            var gymCreationRequest = await _context.GymCreationRequests.FirstOrDefaultAsync(gcr => gcr.Id == request.gymCreationRequestId);

            if (gymCreationRequest == null)
            {
                return Result.Failure(["GymCreationRequest with this id not found."]);
            }

            var creationDto = gymCreationRequest.RequestDto;

            if (creationDto == null || creationDto is not CreateGymDTO)
            {
                return Result.Failure(["Gym creation details are not found or corrupted for this request."]);
            }

            var gymId = Guid.NewGuid().ToString();
            var gym = new Gym
            {
                Id = gymId,
                QRCode = _qrCodeService.GenerateQrCode(gymId),
                Name = creationDto.GymName,
                Address = creationDto.GymAddress,
                OwnerName = creationDto.GymOwnerName,
            };

            await _context.Gyms.AddAsync(gym);

            var userCreationResult = await _identityService.CreateGymManagementUserAsync( 
                creationDto.GymAdminEmail,
                creationDto.GymAdminPassword,
                creationDto.GymAdminFirstName,
                creationDto.GymAdminLastName,
                Roles.GymAdministrator,
                gym,
                creationDto.EscalationEmail
            ); //this call it's own .SaveChangesAsync- they are using the same instance of dbcontext so the transaction use is correct

            if (!userCreationResult.Result.Succeeded)
            {
                await transaction.RollbackAsync();
                return Result.Failure(["Failed to create gym administrator account."]);
            }

            gymCreationRequest.RequestStatus = RequestStatus.Completed;
            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return Result.Success();
        }
        catch
        {
            await transaction.RollbackAsync(CancellationToken.None);
            throw;
        }
    }
}