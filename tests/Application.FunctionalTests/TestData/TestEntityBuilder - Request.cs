using System.Text.Json;
using FitPass.Application.Common.Settings;
using FitPass.Application.Requests.DTOs;
using FitPass.Domain.Entities;
using FitPass.Domain.Enums;
using FitPass.Domain.ValueObjects;
using FitPass.Infrastructure.Identity;

namespace FitPass.Application.FunctionalTests.TestData;

using static Testing;

public partial class TestEntityBuilder
{
    public static async Task<(
        Request request,
        ApplicationUser pendingGymEmployee,
        UserProfile userProfile,
        CreateGymDto createGymDto
    )> BuildGymCreationRequest()
    {
        var obj = await BuildPendingGymEmployeeAsync();

        var createGymDto = BuildCreateGymDto();

        var request = new Request
        {
            Title = "Gym Creation Request",
            Description = "Request to create a new gym",
            Type = RequestType.GymCreation,
            Status = RequestStatus.Submitted,
            Payload = JsonSerializer.Serialize(createGymDto, JsonDefaults.SerializerOptions),
            CreatedBy = obj.user.Id,
            PriorityLevel = PriorityLevel.High,
        };

        await AddAsync(request);

        return (request, obj.user, obj.userProfile, createGymDto);
    }

    public static CreateGymDto BuildCreateGymDto()
    {
        return new CreateGymDto
        {
            Name = $"CreateGymDto GymName - {Guid.NewGuid()}",
            Address = new Address("line1", "line2", "city", null, "postalCode", "HU"),
            Status = GymStatus.Active,
            Tier = GymTier.Local,
            SupervisorEmail = "escalation@email.com",
        };
    }

    public static GymAdminPromotionDto CreateGymAdminPromotionDto(
        string gymId,
        string pendingGymEmployeeEmail,
        string supervisorEmail = "escalation@test.com"
    )
    {
        return new GymAdminPromotionDto
        {
            GymId = gymId,
            PendingGymEmployeeEmail = pendingGymEmployeeEmail,
            SupervisorEmail = supervisorEmail,
        };
    }
}
