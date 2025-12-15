namespace FitPass.Application.Common.Constants;

public class LocalizationKeys
{
    //Result model messages
    public const string NotFound = nameof(NotFound);
    public const string Conflict = nameof(Conflict);
    public const string ExternalServiceUnavailable = nameof(ExternalServiceUnavailable);
    public const string BusinessRuleViolation = nameof(BusinessRuleViolation);
    public const string InternalServerError = nameof(InternalServerError);
    public const string PaymentRequired = nameof(PaymentRequired);
    public const string Unauthorized = nameof(Unauthorized);
    public const string Forbidden = nameof(Forbidden);
    
    //Business rule violations
    public const string PassNotBelongsToUser = nameof(PassNotBelongsToUser);
    public const string PassIsForAnotherGym = nameof(PassIsForAnotherGym);
    public const string UserIsBannedFromTheGym = nameof(UserIsBannedFromTheGym);
    public const string PassIsExpired = nameof(PassIsExpired);
    public const string PassHasNoUsesLeft = nameof(PassHasNoUsesLeft);
    public const string InvalidPhoneNumber = nameof(InvalidPhoneNumber);
    public const string InvalidEmailAddress = nameof(InvalidEmailAddress);
    
    //Generic property rules
    public const string PropertyIsRequired = nameof(PropertyIsRequired);
    public const string PropertyCannotBeLongerThan = nameof(PropertyCannotBeLongerThan);
    public const string PropertyMustBeAtLeastLength = nameof(PropertyMustBeAtLeastLength);
    
    //Password rules
    public const string PasswordMinimumLength = nameof(PasswordMinimumLength);
    public const string PasswordMaximumLength = nameof(PasswordMaximumLength);
    public const string PasswordAtLeastOneLowerCase = nameof(PasswordAtLeastOneLowerCase);
    public const string PasswordAtLeastOneUpperCase = nameof(PasswordAtLeastOneUpperCase);
    public const string PasswordAtLeastOneNumber = nameof(PasswordAtLeastOneNumber);
    public const string PasswordAtLeastOneSpecial = nameof(PasswordAtLeastOneSpecial);
    
    
    //Generic
    public const string Name = nameof(Name);
    public const string Description = nameof(Description);
    
    //User
    public const string Password = nameof(Password);
    public const string PhoneNumber = nameof(PhoneNumber);
    public const string Email = nameof(Email);
    
    //GymMembership
    public const string GymMembershipId = nameof(GymMembershipId);
    
    //GymMembershipPass/ GymPassProduct
    public const string GymMembershipPassId = nameof(GymMembershipPassId);
    public const string TotalUses = nameof(TotalUses);
    public const string DaysAfterExpires = nameof(DaysAfterExpires);

    public const string SingleUsePassCanOnlyHaveOneTotalUse = nameof(SingleUsePassCanOnlyHaveOneTotalUse);
    public const string UseBasedPassTypeCannotHaveExpirationTime = nameof(UseBasedPassTypeCannotHaveExpirationTime);
    public const string MultiUsePassTypeMustHaveAtLeastTwoUses = nameof(MultiUsePassTypeMustHaveAtLeastTwoUses);
    public const string UnlimitedPassDaysAfterExpiresAtLeastOne = nameof(UnlimitedPassDaysAfterExpiresAtLeastOne);
    
    //GymPassUsage
    public const string LockerNumber = nameof(LockerNumber);
}
