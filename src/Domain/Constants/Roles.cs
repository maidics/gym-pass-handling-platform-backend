namespace FitPass.Domain.Constants;

public abstract class Roles
{
    public const string AppAdministrator = nameof(AppAdministrator);
    public const string GymAdministrator = nameof(GymAdministrator);
    public const string GymStaff = nameof(GymStaff);
    public const string PendingGymEmployee = nameof(PendingGymEmployee);
    public const string User = nameof(User);

    public static string[] All =
        [
            nameof(AppAdministrator),
            nameof(GymAdministrator),
            nameof(GymStaff),
            nameof(PendingGymEmployee),
            nameof(User)
        ];

    public static bool IsValidRole(string roleToCheck)
    {
        return All.Contains(roleToCheck);
    }
}
