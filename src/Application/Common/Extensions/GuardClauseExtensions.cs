using System.Diagnostics.CodeAnalysis;
using FitPass.Domain.Entities;

namespace FitPass.Application.Common.Extensions;

public static class GuardClauseExtensions
{
    public static T NullEntityRelatedToCurrentUser<T>(
        this IGuardClause _, 
        [NotNull][ValidatedNotNull]T? entity, 
        string entityName, 
        string? currentUserId)
    {
        if (entity is null)
        {
            throw new ArgumentNullException(entityName, $"No {entityName} found for '{currentUserId}' user.");
        }

        return entity;
    }
}
