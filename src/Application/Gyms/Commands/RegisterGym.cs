using System.Text.Json;
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

    public RegisterGymCommandHandler(IIdentityService identityService, IApplicationDbContext context)
    {
        _identityService = identityService;
        _context = context;
    }

    public async Task<Result> Handle(RegisterGymCommand command, CancellationToken cancellationToken)
    {
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

            if (request.Payload == null)
            {
                return Result.Failure(["Gym creation request details not found."]);
            }

            var creationDto = JsonSerializer.Deserialize<CreateGymDto>(request.Payload);

            if (creationDto == null)
            {
                return Result.Failure(["Unable to serialize gym creation details."]);
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
    }
}