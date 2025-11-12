using System.Text.Json;
using FitPass.Application.Common.Exceptions;
using FitPass.Application.Common.Extensions;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Security;
using FitPass.Application.Requests.DTOs;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;
using FitPass.Domain.Enums;

namespace FitPass.Application.Requests.Commands;

[Authorize(Roles = Roles.PendingGymEmployee)]
public record CreateGymCreationRequestCommand(
    string RequestDescription,
    PriorityLevel PriorityLevel,
    CreateGymDto CreateGymDto
) : IRequest;

public class CreateGymCreationRequestCommandValidator : AbstractValidator<CreateGymCreationRequestCommand>
{
    public CreateGymCreationRequestCommandValidator()
    {
        RuleFor(v => v.RequestDescription)
            .NotEmptyWithMaxLenghtAndMessage(nameof(CreateGymCreationRequestCommand.RequestDescription), MaxStringLengths.Description);

        RuleFor(v => v.CreateGymDto.GymName)
            .NotEmptyWithMaxLenghtAndMessage(nameof(CreateGymCreationRequestCommand.CreateGymDto.GymName), MaxStringLengths.Description);

        RuleFor(v => v.CreateGymDto.GymAddress)
            .NotEmptyWithMaxLenghtAndMessage(nameof(CreateGymCreationRequestCommand.CreateGymDto.GymAddress), MaxStringLengths.Address);

        RuleFor(v => v.CreateGymDto.EscalationEmail)
            .ValidEmailAddress(nameof(CreateGymCreationRequestCommand.CreateGymDto.EscalationEmail));
    }
}

public class CreateGymCreationRequestCommandHandler : IRequestHandler<CreateGymCreationRequestCommand>
{
    private readonly IIdentityService _identityService;
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public CreateGymCreationRequestCommandHandler(
        IIdentityService identityService,
        IApplicationDbContext context,
        IUser user)
    {
        _identityService = identityService;
        _context = context;
        _user = user;
    }

    public async Task Handle(CreateGymCreationRequestCommand command, CancellationToken cancellationToken)
    {
        if (!await _identityService.IsUserEmailConfirmed(_user.Id!))
        {
            throw new BadRequestException("You must confirm your email before this action.");
        }

        var ongoingRequests = await _context.Requests
            .Where(r => r.CreatedBy == _user.Id && (r.Status == RequestStatus.Submitted))
            .ToListAsync();

        if (ongoingRequests.Count > 0)
        {
            throw new BadRequestException("You already have an ongoing gym creation request.");
        }

        /*
        var sevenDaysAgo = DateTimeOffset.UtcNow.AddDays(-7);

        var requestsInPastWeek = await _context.Requests
            .Where(r => r.CreatedBy == _user.Id && r.CreatedOn <= sevenDaysAgo)
            .OrderByDescending(r => r.CreatedOn)
            .ToListAsync();

        if (requestsInPastWeek.Count > 0)
        {
            throw new BadRequestException($"You can only submit one gym creation request per week. You will be able to submit a request on: {requestsInPastWeek.First().CreatedOn.AddDays(7)}.");
        }
        */

        var gymCreationRequest = new Request
        {
            Id = Guid.NewGuid().ToString(),
            Title = "Gym creation",
            Description = command.RequestDescription,
            PriorityLevel = command.PriorityLevel,
            Type = RequestType.GymCreation,
            Payload = JsonSerializer.Serialize(command.CreateGymDto),
        };

        await _context.Requests.AddAsync(gymCreationRequest);

        await _context.SaveChangesAsync();
    }
}
