using System.Text.Json;
using Fitpass.Application.Common.Exceptions;
using Fitpass.Application.Requests.DTOs;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Requests.DTOs;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;
using FitPass.Domain.Enums;

namespace FitPass.Application.Common.Services;

public class RequestService : IRequestService
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityService _identityService;

    public RequestService(IApplicationDbContext context, IIdentityService identityService)
    {
        _context = context;
        _identityService = identityService;
    }

    public async Task FulfillRequest(string requestId)
    {
        var request = await _context.Requests.FindAsync(requestId);

        Guard.Against.NotFound(requestId, request, "Id");

        if (request.Status == RequestStatus.Completed || request.Status == RequestStatus.Rejected)
        {
            throw new BadRequestException($"This request has been {request.Status}");
        }

        switch (request.Type)
        {
            case RequestType.GymAdminNomination:
                await FulFillGymAdminNomination(request);
                break;
            case RequestType.GymCreation:
                await FulFillGymCreation(request);
                break;
        }

        throw new Exception($"Failed to fulfill '{request.Id}' request.");
    }

    private async Task FulFillGymAdminNomination(Request request)
    {
        var requestDto = JsonSerializer.Deserialize<GymAdminNominationDto>(request.Payload!);

        Guard.Against.Null(requestDto, "Request object", "Failed to retrieve Gym Admin Nomination details.");

        var user = await _context.ApplicationUsers.FindAsync(requestDto.UserIdToNominate);

        Guard.Against.NotFound(requestDto.UserIdToNominate, user, "Email");

        if (await _identityService.IsInRoleAsync(user, Roles.GymAdministrator))
        {
            request.Status = RequestStatus.Rejected;

            await _context.SaveChangesAsync();

            throw new BadRequestException("User is already a Gym Administrator.");
        }

        if (user.IsGymMember)
        {
            request.Status = RequestStatus.Rejected;

            await _context.SaveChangesAsync();

            throw new BadRequestException("User has purchased passed before so they are not eligible for nomination.");
        }

        if (user.GymStaffAssigment != null && user.GymStaffAssigment.GymId != requestDto.GymId)
        {
            throw new BadRequestException("Gym staff member cannot be nominated to Gym Administrator to another gym.");
        }

        if (user.GymStaffAssigment == null)
        {
            var assigment = new GymStaffAssigment
            {
                ApplicationUserId = user.Id,
                GymId = requestDto.GymId,
                EscalationEmail = requestDto.EscalationEmail
            };

            user.GymStaffAssigment = assigment;
        }
        else
        {
            user.GymStaffAssigment.EscalationEmail = requestDto.EscalationEmail;
        }

        var result = await _identityService.AddToRoleAsync(user, Roles.GymAdministrator);

        if (!result.Succeeded)
        {
            throw new Exception("An unhandled error occured during user nomination.");
        }

        await _context.SaveChangesAsync();

        return;
    }

    private async Task FulFillGymCreation(Request request)
    {
        var requestDto = JsonSerializer.Deserialize<CreateGymDto>(request.Payload!);

        Guard.Against.Null(requestDto, "Request object", "Failed to retrieve Gym Creation Details.");

        if (await _context.Gyms.AsNoTracking().FirstOrDefaultAsync(g => g.Name == requestDto.GymName) != null)
        {
            throw new ConflictException($"Gym with '{requestDto.GymName}' already exists.");
        }

        var userToNominate = await _context.ApplicationUsers.FindAsync(request.CreatedBy);

        Guard.Against.NotFound(request.CreatedBy!, userToNominate, "Id");

        var gym = new Gym
        {
            Name = requestDto.GymName,
            Address = requestDto.GymAddress,
            Status = requestDto.GymStatus,
            Tier = requestDto.GymTier
        };

        _context.Gyms.Add(gym);

        var result = await _identityService.AddToRoleAsync(userToNominate, Roles.GymAdministrator);

        if (!result.Succeeded)
        {
            throw new Exception("An unhandled error occured during user nomination.");
        }

        await _context.SaveChangesAsync();

        return;
    }
}