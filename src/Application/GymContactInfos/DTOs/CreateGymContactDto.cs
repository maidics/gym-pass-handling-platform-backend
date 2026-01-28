using FitPass.Domain.ValueObjects;

namespace FitPass.Application.GymContactInfos.DTOs;

public record CreateGymContactDto(PhoneNumber? PhoneNumber, string? Email, string FullName, Address? Address);
