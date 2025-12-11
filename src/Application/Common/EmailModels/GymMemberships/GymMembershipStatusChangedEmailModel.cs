using FitPass.Domain.Enums;

namespace FitPass.Application.Common.EmailModels.GymMemberships;

//TODO: add gym contact here & add it to .cshtml
public record GymMembershipStatusChangedEmailModel(
    GymMembershipStatus NewGymMembershipStatus,
    string UserFirstName,
    string GymName) : IEmailModel;
