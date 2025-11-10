using FitPass.Application.FunctionalTests.TestData.EntityBuilders;
using FitPass.Application.Requests.DTOs;
using FitPass.Domain.Entities;
using FitPass.Domain.Enums;
using FitPass.Infrastructure.Identity;

namespace FitPass.Application.FunctionalTests.TestData;

using static Testing;

public partial class TestEntityBuilder
{
    public static async Task<(Request request, ApplicationUser pendingGymEmployee, UserProfile userProfile, CreateGymDto createGymDto)> BuildGymCreationRequest()
    {
        var obj = await BuildPendingGymEmployeeAsync();

        var createGymDto = new CreateGymDto
        {
            GymName = "Gym from Request",
            GymAddress = "Gym from Request Address",
            GymStatus = GymStatus.Active,
            GymTier = GymTier.Local,
            EscalationEmail = "escalation@email"
        };

        var request = await RequestBuilder
            .WithRequestType(RequestType.GymCreation)
            .WithRequestStatus(RequestStatus.Submitted)
            .WithPayload(createGymDto)
            .WithCreatedBy(obj.user.Id)
            .BuildAsync();

        return (request, obj.user, obj.userProfile, createGymDto);
    }
}
