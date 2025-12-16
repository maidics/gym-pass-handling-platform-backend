using FitPass.Domain.Enums;

namespace FitPass.Application.Common.EmailModels.GymMemberships;

//TODO: add gym contact here & add it to .cshtml
public class GymMembershipStatusChangedEmailModel : IEmailModel
{
    public required string? Language { get; set; }
    public required GymMembershipStatus NewGymMembershipStatus { get; init; }
    public required string UserFirstName  { get; init; }
    public required string GymName { get; init; }
}
    
