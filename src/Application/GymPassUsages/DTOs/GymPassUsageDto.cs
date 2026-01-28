using FitPass.Domain.Enums;

namespace FitPass.Application.GymPassUsages.DTOs;

public record GymPassUsageDto(
    string Id,
    string? FirstName,
    string? LastName,
    string GymId,
    PassType PassType,
    int? TotalPassUses,
    int? RemainingPassUses,
    DateTimeOffset? PassExpirationDate,
    PassUseResult PassUseResult,
    string? LockerNumber,
    DateTimeOffset CreatedOn,
    DateTimeOffset? GymSessionEndedAt);
