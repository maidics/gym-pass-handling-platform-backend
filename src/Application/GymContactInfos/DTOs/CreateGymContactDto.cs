using FitPass.Domain.ValueObjects;

namespace FitPass.Application.GymContactInfos.DTOs;

public record PhoneCreateGymContactDto(PhoneNumber? PhoneNumber, string? Email, string FullName, Address? Address);
