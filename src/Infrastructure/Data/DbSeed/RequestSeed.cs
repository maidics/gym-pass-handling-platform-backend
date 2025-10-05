using System.Text.Json;
using Fitpass.Application.Requests.DTOs;
using FitPass.Application.Requests.DTOs;
using FitPass.Domain.Entities;
using FitPass.Domain.Enums;

namespace FitPass.Infrastructure.Data.DbSeed;

public partial class ApplicationDbContextInitialiser
{
    public async Task SeedRequestAsync()
    {
        List<Request> requests = [
                new Request {
                    Id = "Request1",
                    Title = "Gym Creation Request",
                    Description = "Gym creation description",
                    PriorityLevel = PriorityLevel.High,
                    Type = RequestType.GymCreation,
                    Payload = JsonSerializer.Serialize(
                            new CreateGymDto {
                                GymName = "GymRequestGym",
                                GymAddress = "localhost",
                                GymStatus = GymStatus.Active,
                                GymTier = GymTier.Local,
                                EscalationEmail = "escalationemail@localhost"
                            }
                        )
                },
                new Request {
                    Id = "Request2",
                    Title = "Gym Admin Nomination",
                    Description = "Gym admin nomination description",
                    PriorityLevel = PriorityLevel.Medium,
                    Type = RequestType.GymAdminNomination,
                    Payload = JsonSerializer.Serialize(
                            new GymAdminNominationDto {
                                GymId = gymId2,
                                UserIdToNominate = "User2",
                                EscalationEmail = "escalationemail@localhost"
                            }
                        )
                }
            ];

        await _context.Requests.AddRangeAsync(requests);
    }
}
