using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Models;
using FitPass.Application.Common.Security;
using FitPass.Application.Extensions;
using FitPass.Domain.Constants;

namespace FitPass.Application.Gyms.Commands;

[Authorize(Roles = Roles.AppAdministrator)]
public record RegisterGymCommand(
    string GymCreationRequestId
) : IRequest<Result>;

public class RegisterGymCommandValidator : AbstractValidator<RegisterGymCommand>
{
    public RegisterGymCommandValidator()
    {
        RuleFor(v => v.GymCreationRequestId).NotEmptyWithMessage(nameof(RegisterGymCommand.GymCreationRequestId));
    }
}

public class RegisterGymCommandHandler : IRequestHandler<RegisterGymCommand, Result>
{
    private readonly IIdentityService _identityService;
    private readonly IApplicationDbContext _context;

    public RegisterGymCommandHandler(IIdentityService identityService, IApplicationDbContext context)
    {
        _identityService = identityService;
        _context = context;
    }

    public Task<Result> Handle(RegisterGymCommand command, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();

        /*

        await using var transaction = await _context.BeginTransactionAsync(cancellationToken);

        try
        {
            var request = await _context.Requests.FindAsync(command.gymCreationRequestId, cancellationToken);

            if (request == null)
            {
                return Result.Failure(["Request with this id not found."]);
            }

            if (request.Type != RequestType.GymCreation)
            {
                return Result.Failure(["Request if not of GymCreation type."]);
            }

            var creationDto = request.DeserializePayload<CreateGymDto>();

            if (creationDto == null)
            {
                return Result.Failure(["Unable to serialize gym creation details or no details found."]);
            }

            var gymId = $"fitpass_gym_{Guid.NewGuid()}";
            var gym = new Gym
            {
                Id = gymId,
                Name = creationDto.GymName,
                Address = creationDto.GymAddress,
                Status = creationDto.GymStatus,
                Tier = creationDto.GymTier,
                OwnerName = creationDto.GymOwnerName
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

            request.Status = RequestStatus.Completed;
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return Result.Success();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }

        */
    }
}
