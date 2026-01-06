using FitPass.Application.GymPassProducts.DTOs;
using FitPass.Application.TenantPaymentProfiles.DTOs;
using FitPass.Domain.Entities;
using FitPass.Domain.Enums;
using FitPass.Domain.ValueObjects;

namespace FitPass.Application.Gyms.DTOs;

public class GymDto
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public required Address Address { get; set; }
    public required GymStatus Status { get; set; }
    public required GymTier Tier { get; set; }
    public required DateTimeOffset CreatedOn { get; set; }
    public required string? CreatedBy { get; set; }
    public required DateTimeOffset LastModifiedOn { get; set; }
    public required string? LastModifiedBy { get; set; }
    public required TenantPaymentProfileDto? PaymentProfile { get; set; }
    public required List<GymPassProductDto> PassProducts { get; set; }
    public required List<GymContactInfoDto> ContactInfos { get; set; }
}

public static partial class Mappings
{
    extension(Gym gym)
    {
        public GymDto MapToDto()
        {
            return new GymDto
            {
                Id = gym.Id,
                Name = gym.Name,
                Address = gym.Address,
                Status = gym.Status,
                Tier = gym.Tier,
                CreatedOn = gym.CreatedOn,
                CreatedBy = gym.CreatedBy,
                LastModifiedOn = gym.LastModifiedOn,
                LastModifiedBy = gym.LastModifiedBy,
                PaymentProfile = gym.PaymentProfile?.MapToDto(),
                PassProducts = [.. gym.PassProducts.Select(p => p.MapToDto())],
                ContactInfos = [..gym.ContactInfos.Select(x => x.MapToDto())]
            };
        }
    }
}
