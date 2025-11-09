using System.Text.Json;
using FitPass.Application.Common.Exceptions;
using FitPass.Application.Common.Extensions;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Logging;
using FitPass.Application.Common.Security;
using FitPass.Application.Gyms.DTOs;
using FitPass.Application.Requests.DTOs;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;
using FitPass.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace FitPass.Application.Gyms.Commands;

[Authorize(Roles = Roles.AppAdministrator)]
public record RegisterGymFromRequestCommand(string RequestId) : IRequest<GymDto>;

public class RegisterGymFromRequestCommandValidator : AbstractValidator<RegisterGymFromRequestCommand>
{
    public RegisterGymFromRequestCommandValidator()
    {
        RuleFor(v => v.RequestId).NotEmptyWithMessage(nameof(RegisterGymFromRequestCommand.RequestId));
    }
}

public class RegisterGymFromRequestCommandHandler : IRequestHandler<RegisterGymFromRequestCommand, GymDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<RegisterGymFromRequestCommand> _logger;
    private readonly IUser _user;
    private readonly IIdentityService _identityService;
    private readonly IMapper _mapper;

    public RegisterGymFromRequestCommandHandler(
        IApplicationDbContext context,
        ILogger<RegisterGymFromRequestCommand> logger,
        IUser user,
        IIdentityService identityService,
        IMapper mapper)
    {
        _context = context;
        _logger = logger;
        _user = user;
        _identityService = identityService;
        _mapper = mapper;
    }
    
    public async Task<GymDto> Handle(RegisterGymFromRequestCommand command, CancellationToken cancellationToken)
    {
        var request = await _context
            .Requests
            .FindAsync(command.RequestId);

        Guard.Against.NotFound(command.RequestId, request, nameof(Request));

        if (request.Status != RequestStatus.Submitted)
        {
            throw new ForbiddenAccessException();
        }

        if (request.Type != RequestType.GymCreation)
        {
            throw new BadRequestException("Request is not of GymCreation type.");
        }

        if (request.CreatedBy is null)
        {
            _logger.LogError("CreatedBy property of {Request} is null. Cannot nominate GymAdmin.", request);

            request.Status = RequestStatus.CreatorNotFound;

            await _context.SaveChangesAsync();

            throw new ArgumentNullException(nameof(Request.CreatedBy));
        } else
        {
            if (!await _identityService.IsInRoleAsync(request.CreatedBy, Roles.PendingGymEmployee))
            {
                request.Status |= RequestStatus.RelatedRoleHandlingFailed;

                await _context.SaveChangesAsync();

                throw new BadRequestException("User is no longer a PendingGymEmployee.");
            }
        }

        if (request.Payload is null)
        {
            _logger.LogError("Found Request does not have a payload.");

            request.Status = RequestStatus.PayloadWasNull;

            await _context.SaveChangesAsync();

            throw new ArgumentNullException(nameof(Request.Payload));
        }

        CreateGymDto? createGymDto;

        try
        {
            createGymDto = request.DeserializePayload<CreateGymDto>();

            if (createGymDto == null)
            {
                LogErrorMessages.JsonSerilaizationFailure(
                    _logger,
                    "Deserialization",
                    nameof(Request.DeserializePayload),
                    nameof(Request),
                    request.Id,
                    request.Payload,
                    null);

                request.Status = RequestStatus.PayloadFailedToSerialize;

                await _context.SaveChangesAsync();

                throw new JsonException("Failed to serialize request payload.");
            }
        }
        catch (Exception ex)
        {
            LogErrorMessages.JsonSerilaizationFailure(
                _logger,
                "Deserialization",
                nameof(Request.DeserializePayload),
                nameof(Request),
                request.Id,
                request.Payload,
                ex);

            request.Status = RequestStatus.PayloadFailedToSerialize;

            await _context.SaveChangesAsync();

            throw new JsonException("Failed to deserialize payload.", ex);
        }

        var existingGym = await _context
                .Gyms
                .AsNoTracking()
                .FirstOrDefaultAsync(g => g.Name == createGymDto.GymName); //TODO: make this more robust

        if (existingGym is not null)
        {
            throw new ConflictException($"Gym with '{existingGym.Name}' name already exists.");
        }

        using var transaction = await _context.BeginTransactionAsync();

        try
        {
            var gym = new Gym
            {
                Name = createGymDto.GymName,
                Address = createGymDto.GymAddress,
                Status = createGymDto.GymStatus,
                Tier = createGymDto.GymTier,
                OwnerName = createGymDto.GymOwnerName
            };

            await _context.Gyms.AddAsync(gym);

            var demotionResult = await _identityService.RemoveFromRoleAsync(request.CreatedBy, Roles.PendingGymEmployee);

            if (!demotionResult.Succeeded)
            {
                await transaction.RollbackAsync();

                LogErrorMessages.IdentityServiceMethodFailed(
                    _logger,
                    nameof(IIdentityService.RemoveFromRoleAsync),
                    [Roles.PendingGymEmployee],
                    request.CreatedBy,
                    demotionResult);

                request.Status = RequestStatus.RelatedRoleHandlingFailed;

                await _context.SaveChangesAsync();

                throw new SystemException("Failed to remove request creator from role.");
            }

            var promotionResult = await _identityService.AddToRoleAsync(request.CreatedBy, Roles.GymAdministrator);

            if (!promotionResult.Succeeded)
            {
                await transaction.RollbackAsync();

                LogErrorMessages.IdentityServiceMethodFailed(
                    _logger,
                    nameof(IIdentityService.AddToRoleAsync),
                    [Roles.GymAdministrator],
                    request.CreatedBy,
                    promotionResult);

                request.Status = RequestStatus.RelatedRoleHandlingFailed;

                await _context.SaveChangesAsync();

                throw new SystemException("Failed to add request creator to role.");
            }

            var gymEmployment = new GymEmployment
            {
                ApplicationUserId = request.CreatedBy,
                GymId = gym.Id,
                Role = Roles.GymAdministrator,
                EscalationEmail = createGymDto.EscalationEmail
            };

            await _context.GymEmployments.AddAsync(gymEmployment);

            request.Status = RequestStatus.Completed;

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return _mapper.Map<GymDto>(gym);
        } catch (Exception ex)
        {
            LogErrorMessages.UnhandledExceptionCaught(
                _logger,
                nameof(RegisterGymFromRequestCommand),
                ex);

            await transaction.RollbackAsync();

            throw;
        }
    }
}
