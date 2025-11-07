using FitPass.Application.GymMembershipPasses.DTOs;
using FitPass.Domain.Common;
using FitPass.Domain.Entities;

namespace FitPass.Application.GymMembershipPassUsages.DTOs;

public class GymPassUsageDto : BaseAuditableEntity
{
    //TODO: add name later => QueryService
    public required string ApplicationUserId { get; set; }
    public required GymMembershipPassDto Pass {  get; set; }

    private class Mapping : Profile
    {
        public Mapping() 
        {
            CreateMap<GymPassUsage, GymPassUsageDto>();
        }
    }
}
