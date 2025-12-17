namespace FitPass.Domain.Entities;

public class UserProfile : BaseEntity
{
    public required string UserId { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string PreferredLanguage { get; set; }
    public required DateTimeOffset CreatedOn { get; set; }
}
