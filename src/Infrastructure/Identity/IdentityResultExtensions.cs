using System.Collections.Frozen;
using FitPass.Application.Common.Models;
using Microsoft.AspNetCore.Identity;

namespace FitPass.Infrastructure.Identity;

public static class IdentityResultExtensions
{
    extension(IdentityResult result)
    {
        public Result ToApplicationResult()
        {
            if (result.Succeeded)
            {
                return Result.Success();
            }

            return MapToFailure(result);
        }

        public Result<T> ToApplicationResult<T>(T value)
        {
            if (result.Succeeded)
            {
                return Result<T>.Success(value);
            }

            return MapToFailure(result);
        }
    }

    private static readonly FrozenSet<string> _internalErrors = [
        "DuplicateRoleName",
        "InvalidRoleName",
        "UserAlreadyInRole",
        "UserNotInRole",
        "ConcurrencyFailure",
        "UserLockoutNotEnabled",
        "DefaultError"
    ];

    private static readonly FrozenSet<string> _businessRuleViolationErrors = [
        "PasswordTooShort",
        "PasswordRequiresNonAlphanumeric",
        "PasswordRequiresDigit",
        "PasswordRequiresLower",
        "PasswordRequiresUpper",
        "PasswordRequiresUniqueChars",
        "InvalidUserName",
        "InvalidEmail"
    ];

    private static readonly FrozenSet<string> _unauthorizedErrors = [
        "PasswordMismatch", //incorrect password provided during a change check
        "RecoveryCodeRedemptionFailed"
    ];

    private static readonly FrozenSet<string> _conflictErrors = [
        "DuplicateUserName",
        "DuplicateEmail"
    ];

    private static readonly FrozenSet<string> _forbiddenErrors = [
        "LoginAlreadyAssociated"
    ];

    private static ResultTypes GetResultType(IEnumerable<string> codes)
    {
        if (_internalErrors.Overlaps(codes)) return ResultTypes.InternalError;
        if (_businessRuleViolationErrors.Overlaps(codes)) return ResultTypes.BusinessRuleViolation;
        if (_unauthorizedErrors.Overlaps(codes)) return ResultTypes.Unauthorized;
        if (_forbiddenErrors.Overlaps(codes)) return ResultTypes.Forbidden;
        if (_conflictErrors.Overlaps(codes)) return ResultTypes.Conflict;

        return ResultTypes.InternalError;
    }

    private static ResultFailure MapToFailure(IdentityResult result)
    {
        var codes = result.Errors.Select(e => e.Code);

        return new(GetResultType(codes), string.Join(", ", codes), result.Errors.Select(e => e.Description));
    }
}
