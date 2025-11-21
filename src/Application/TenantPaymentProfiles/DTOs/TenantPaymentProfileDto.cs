using FitPass.Domain.Entities.Payment;

namespace FitPass.Application.TenantPaymentProfiles.DTOs;

public record TenantPaymentProfileDto
{
    public required string GymId { get; set; }
    public required TenantPaymentAccountStatusDto AccountStatus { get; set; }
    public required DateTimeOffset? LastUpdatedOnPaymentProvidersSide { get; set; }
    public required string? LastUpdatedByOnPaymentProvidersSide { get; set; }
    public required DateTimeOffset? LastAccountLinkGeneratedOn { get; set; }
    public required string? LastAccountLinkGeneratedBy { get; set; }
}

public static partial class Mappings
{
    public static TenantPaymentProfileDto MapToDto(this TenantPaymentProfile tenantPaymentProfile)
    {
        return new TenantPaymentProfileDto
        {
            GymId = tenantPaymentProfile.GymId,
            AccountStatus = tenantPaymentProfile.AccountStatus.MapToDto(),
            LastUpdatedByOnPaymentProvidersSide = tenantPaymentProfile.LastUpdatedByOnPaymentProvidersSide,
            LastUpdatedOnPaymentProvidersSide = tenantPaymentProfile.LastUpdatedOnPaymentProvidersSide,
            LastAccountLinkGeneratedOn = tenantPaymentProfile.LastAccountLinkGeneratedOn,
            LastAccountLinkGeneratedBy = tenantPaymentProfile.LastAccountLinkGeneratedBy
        };
    }
}
