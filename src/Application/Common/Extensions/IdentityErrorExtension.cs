using Microsoft.AspNetCore.Identity;

namespace Fitpass.Application.Common.Extensions;

public static class IdentityResultExtensions
{
    public static bool IsDuplicateUserName(this IdentityResult result)
    {
        return result.Errors.Any(e => e.Code == "DuplicateUserName");
    }

    public static bool IsDuplicateEmail(this IdentityResult result)
    {
        return result.Errors.Any(e => e.Code == "DuplicateEmail");
    }
}