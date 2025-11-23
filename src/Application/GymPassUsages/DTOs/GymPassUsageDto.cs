using FitPass.Domain.Common;
using FitPass.Domain.Entities;
using FitPass.Domain.Enums;

namespace FitPass.Application.GymPassUsages.DTOs;

public class GymPassUsageDto : BaseAuditableEntity
{
    //TODO: add name later => QueryService
    public required string ApplicationUserId { get; set; }
    public required string GymId { get; init; }
    public required PassType PassType { get; init; } //this is here so if the pass is archived then still available
    public required int? TotalPassUses { get; init; } //same ^
    public required int? RemainingPassUses { get; init; }
    public required DateOnly? PassExpirationDate { get; init; }
    public required PassUseResult PassUseResult { get; init; }
    public required string? LockerNumber { get; set; }
    //Started time can be retrieved from CreatedOn
    public DateTimeOffset? GymSessionEndedAt {  get; set; }
}

public static partial class Mappings
{
    extension(GymPassUsage gymPassUsage)
    {
        public GymPassUsageDto MapToDto()
        {
            return new GymPassUsageDto
            {
                ApplicationUserId = gymPassUsage.ApplicationUserId,
                GymId = gymPassUsage.GymId,
                PassType = gymPassUsage.PassType,
                TotalPassUses = gymPassUsage.TotalPassUses,
                RemainingPassUses = gymPassUsage.RemainingPassUses,
                PassExpirationDate = gymPassUsage.PassExpirationDate,
                PassUseResult = gymPassUsage.PassUseResult,
                LockerNumber = gymPassUsage.LockerNumber,
                GymSessionEndedAt = gymPassUsage.GymSessionEndedAt
            };
        }
    }
}
