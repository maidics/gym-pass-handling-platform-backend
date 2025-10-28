using System.Text.Json;
using Fitpass.Application.Gyms.DTOs;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Logging;
using FitPass.Application.Common.Security;
using FitPass.Application.Extensions;
using FitPass.Application.Requests.DTOs;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;
using FitPass.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace FitPass.Application.Gyms.Commands;

[Authorize(Roles = Roles.AppAdministrator)]
public record RegisterGymCommand(
    string GymCreationRequestId
) : IRequest<GymDto>;

public class RegisterGymCommandValidator : AbstractValidator<RegisterGymCommand>
{
    public RegisterGymCommandValidator()
    {
        RuleFor(v => v.GymCreationRequestId).NotEmptyWithMessage(nameof(RegisterGymCommand.GymCreationRequestId));
    }
}

public class RegisterGymCommandHandler : IRequestHandler<RegisterGymCommand, GymDto>
{
    private readonly IIdentityService _identityService;
    private readonly IApplicationDbContext _context;
    private readonly ILogger<RegisterGymCommandHandler> _logger;
    private readonly IMapper _mapper;

    public RegisterGymCommandHandler(
        IIdentityService identityService,
        IApplicationDbContext context,
        ILogger<RegisterGymCommandHandler> logger,
        IMapper mapper)
    {
        _identityService = identityService;
        _context = context;
        _logger = logger;
        _mapper = mapper;
    }

    public async Task<GymDto> Handle(RegisterGymCommand command, CancellationToken cancellationToken)
    {
        var request = await _context
            .Requests
            .FirstOrDefaultAsync(r => r.Id == command.GymCreationRequestId && r.Type == RequestType.GymCreation);

        Guard.Against.NotFound(command.GymCreationRequestId, request, "GymCreationRequest");

        if (request.CreatedBy == null)
        {
            throw new Exception("Failed to retrieve Request creator id.");
        }

        await using var transaction = await _context.BeginTransactionAsync(cancellationToken);

        try
        {
            CreateGymDto? createGymDto;

            try
            {
                createGymDto = request.DeserializePayload<CreateGymDto>();

                if (createGymDto == null)
                {
                    throw new JsonException("Failed to serialize Gym creation request payload.");
                }
            }
            catch (Exception ex)
            {
                LogErrorMessages.JsonSerilaizationFailure(_logger, "Deserialization", nameof(Request.DeserializePayload), nameof(Request), request.Id, request.Payload, ex);
                throw new JsonException("Failed to serialize Gym creation request payload.");
            }

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

                LogErrorMessages.IdentityServiceMethodFailed(_logger, nameof(IIdentityService.RemoveFromRoleAsync), Roles.PendingGymEmployee, request.CreatedBy, demotionResult);

                throw new Exception($"Failed to remove user from {Roles.PendingGymEmployee} role before making them {Roles.GymAdministrator}.");
            }

            var promotionResult = await _identityService.AddToRoleAsync(request.CreatedBy, Roles.GymAdministrator);

            if (!promotionResult.Succeeded)
            {
                await transaction.RollbackAsync();

                LogErrorMessages.IdentityServiceMethodFailed(_logger, nameof(IIdentityService.AddToRoleAsync), Roles.GymAdministrator, request.CreatedBy, promotionResult);

                throw new Exception($"Failed to add user to {Roles.GymAdministrator} role.");
            }

            var gymEmployment = new GymEmployment
            {
                ApplicationUserId = request.CreatedBy,
                GymId = gym.Id,
                Role = Roles.GymAdministrator
            };

            await _context.GymEmployments.AddAsync(gymEmployment);

            request.Status = RequestStatus.Completed;

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return _mapper.Map<GymDto>(gym);
        }
        catch(Exception ex)
        {
            await transaction.RollbackAsync();

            LogErrorMessages.UnhandledExceptionCaught(_logger, nameof(RegisterGymCommandHandler), ex);

            throw;
        }
    }
}
