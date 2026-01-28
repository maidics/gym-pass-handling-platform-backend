using FitPass.Domain.Entities.Payment;

namespace FitPass.Application.TenantPaymentProfiles.DTOs;

public record TenantPaymentProfileDto
{
    public required string GymId { get; set; }
    //public required TenantPaymentAccountStatusDto AccountStatus { get; set; }
    public required DateTimeOffset CreatedOn { get; set; }
    public required DateTimeOffset? LastAccountLinkGeneratedOn { get; set; }
    public required string? LastAccountLinkGeneratedBy { get; set; }
}

public static partial class Mappings
{
    extension(TenantPaymentProfile tenantPaymentProfile)
    {
        public TenantPaymentProfileDto MapToDto()
        {
            return new TenantPaymentProfileDto
            {
                GymId = tenantPaymentProfile.GymId,
                //AccountStatus = tenantPaymentProfile.AccountStatus.MapToDto(),
                CreatedOn =  tenantPaymentProfile.CreatedOn,
                LastAccountLinkGeneratedOn = tenantPaymentProfile.LastAccountLinkGeneratedOn,
                LastAccountLinkGeneratedBy = tenantPaymentProfile.LastAccountLinkGeneratedBy
            };
        }
    }
}
