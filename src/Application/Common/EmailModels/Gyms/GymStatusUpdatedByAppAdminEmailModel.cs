using FitPass.Domain.Enums;

namespace FitPass.Application.Common.EmailModels.Gyms;

public class GymStatusUpdatedByAppAdminEmailModel : IEmailModel
{
    public required string? Language { get; set; }
    public required GymStatus NewStatus { get; init; }
    public required string GymName  { get; init; }
}
