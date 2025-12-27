
namespace FitPass.Application.Common.EmailModels.GymMemberships;

//TODO: add gym contact here & add it to .cshtml
public class GymMembershipStatusChangedEmailModel : IEmailModel
{
    public required string Language { get; init; }
    public required string Subject { get; init; }
    public required string Greeting { get; init; }
    public required string Body { get; init; }
    public required string Body2 { get; init; }
    public required string Farewell { get; init; }
}
    
