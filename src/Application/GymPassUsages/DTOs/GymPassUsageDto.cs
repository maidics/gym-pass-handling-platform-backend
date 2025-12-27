using FitPass.Domain.Entities;
using FitPass.Domain.Enums;

namespace FitPass.Application.GymPassUsages.DTOs;

public class GymPassUsageDto
{
    //TODO: add name later => QueryService
    public required string UserId { get; set; }
    public required string GymId { get; init; }
    public required PassType PassType { get; init; } //this is here so if the pass is archived then still available
    public required int? TotalPassUses { get; init; } //same ^
    public required int? RemainingPassUses { get; init; }
    public required DateTimeOffset? PassExpirationDate { get; init; }
    public required PassUseResult PassUseResult { get; init; }
    public required string? LockerNumber { get; set; }
    public required DateTimeOffset CreatedOn { get; set; }
    //Started time can be retrieved from CreatedOn
    public DateTimeOffset? GymSessionEndedAt {  get; set; }
}

public static class Mappings
{
    extension(GymPassUsage gymPassUsage)
    {
        public GymPassUsageDto MapToDto()
        {
            return new GymPassUsageDto
            {
                UserId = gymPassUsage.UserId,
                GymId = gymPassUsage.GymId,
                PassType = gymPassUsage.PassType,
                TotalPassUses = gymPassUsage.TotalPassUses,
                RemainingPassUses = gymPassUsage.RemainingPassUses,
                PassExpirationDate = gymPassUsage.PassExpirationDate,
                PassUseResult = gymPassUsage.PassUseResult,
                LockerNumber = gymPassUsage.LockerNumber,
                CreatedOn = gymPassUsage.CreatedOn,
                GymSessionEndedAt = gymPassUsage.GymSessionEndedAt
            };
        }
    }
}
