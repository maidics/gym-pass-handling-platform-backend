using System.Diagnostics.CodeAnalysis;

namespace FitPass.Application.Common.Extensions;

public static class GuardClauseExtensions
{
    public static T NullParameterRelatedToCurrentUser<T>(
        this IGuardClause _, 
        [NotNull][ValidatedNotNull]T? parameter, 
        string parameterName, 
        string? currentUserId)
    {
        if (parameter is null)
        {
            throw new ArgumentNullException(parameterName, $"No {parameterName} found for '{currentUserId}' user.");
        }

        return parameter;
    }
}
