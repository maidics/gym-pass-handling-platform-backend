using FitPass.Application.Common.Exceptions;
using FitPass.Application.Common.Extensions;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Logging;
using FitPass.Application.Common.Security;
using FitPass.Application.Requests.Commands;
using FitPass.Application.Requests.DTOs;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;
using FitPass.Domain.Enums;
using FitPass.Domain.Strings;
using Microsoft.Extensions.Logging;

namespace FitPass.Application.ApplicationUsers.Commands.RoleHandling;


[Authorize(Roles = Roles.AppAdministrator)]
public record PromotePendingGymEmployeeToGymAdminFromRequestCommand(string RequestId) : IRequest;

public class PromotePendingGymEmployeeToGymAdminFromRequestCommandValidator : AbstractValidator<PromotePendingGymEmployeeToGymAdminFromRequestCommand>
{
    public PromotePendingGymEmployeeToGymAdminFromRequestCommandValidator()
    {
        RuleFor(v => v.RequestId).NotEmptyWithMessage(nameof(PromotePendingGymEmployeeToGymAdminFromRequestCommand.RequestId));
    }
}

public class PromotePendingGymEmployeeToGymAdminFromRequestCommandHandler : IRequestHandler<PromotePendingGymEmployeeToGymAdminFromRequestCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ISender _sender;
    private readonly ILogger<PromotePendingGymEmployeeToGymAdminFromRequestCommand> _logger;
    private readonly IIdentityService _identityService;

    public PromotePendingGymEmployeeToGymAdminFromRequestCommandHandler(
        IApplicationDbContext context,
        ISender sender,
        ILogger<PromotePendingGymEmployeeToGymAdminFromRequestCommand> logger,
        IIdentityService identityService)
    {
        _context = context;
        _sender = sender;
        _logger = logger;
        _identityService = identityService;
    }

    public async Task Handle(PromotePendingGymEmployeeToGymAdminFromRequestCommand command, CancellationToken cancellationToken)
    {
        var request = await _context.Requests.FindAsync(command.RequestId);

        Guard.Against.NotFound(command.RequestId, request);

        if (request.Status != RequestStatus.Submitted)
        {
            throw new ForbiddenAccessException();
        }

        if (request.Type != RequestType.GymCreation)
        {
            throw new BadRequestException("Request is not of GymCreation type.");
        }

        var deserializationResult = await _sender.Send(new DeserializeRequestPayloadCommand<GymAdminPromotionDto>(request));

        if (!deserializationResult.Succeeded)
        {
            request.Status = RequestStatus.Error;
            request.Error = string.Join(", ", deserializationResult.Errors);

            await _context.SaveChangesAsync();

            throw new ArgumentException("Failed to deserialize payload.");
        }

        var promotionDto = deserializationResult.Value;

        if (!await _identityService.DoesUserExist(promotionDto.UserIdToNominate))
        {
            throw new NotFoundException(promotionDto.UserIdToNominate, "PendingGymEmployee to promote.");
        }

        if (!await _identityService.IsInRoleAsync(promotionDto.UserIdToNominate, Roles.PendingGymEmployee))
        {
            throw new BadRequestException("User is not a PendingGymEmployee.");
        }

        using var transaction = await _context.BeginTransactionAsync();

        try
        {
            var demotionResult = await _identityService.RemoveFromRoleAsync(promotionDto.UserIdToNominate, Roles.PendingGymEmployee);

            if (!demotionResult.Succeeded)
            {
                LogErrorMessages.IdentityServiceMethodFailed(
                    _logger,
                    nameof(IIdentityService.RemoveFromRoleAsync),
                    [Roles.PendingGymEmployee],
                    promotionDto.UserIdToNominate,
                    demotionResult);

                await transaction.RollbackAsync();

                throw new SystemException(ErrorMessages.FailedToHandleRole(Roles.PendingGymEmployee, false, demotionResult.Errors));
            }

            var promotionResult = await _identityService.AddToRoleAsync(promotionDto.UserIdToNominate, Roles.GymAdministrator);

            if (!promotionResult.Succeeded)
            {
                LogErrorMessages.IdentityServiceMethodFailed(
                    _logger,
                    nameof(IIdentityService.AddToRoleAsync),
                    [Roles.GymAdministrator],
                    promotionDto.UserIdToNominate,
                    promotionResult);

                await transaction.RollbackAsync();

                throw new SystemException(ErrorMessages.FailedToHandleRole(Roles.GymAdministrator, true, demotionResult.Errors));
            }

            var gymEmployment = new GymEmployment
            {
                UserId = promotionDto.UserIdToNominate,
                GymId = promotionDto.GymId,
                Role = Roles.GymAdministrator,
                EscalationEmail = promotionDto.EscalationEmail
            };

            await _context.GymEmployments.AddAsync(gymEmployment);

            request.Status = RequestStatus.Completed;

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
        } catch (Exception ex)
        {
            LogErrorMessages.UnhandledExceptionCaught(
                _logger,
                nameof(PromotePendingGymEmployeeToGymAdminFromRequestCommandHandler),
                ex);

            await transaction.RollbackAsync();
            throw;
        }
    }
}
