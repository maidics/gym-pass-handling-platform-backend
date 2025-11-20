using FitPass.Application.Common.Models;
using Microsoft.AspNetCore.Identity;

namespace FitPass.Infrastructure.Identity;

public static class IdentityResultExtensions
{
    public static Result ToApplicationResult(this IdentityResult result, ResultTypes type = ResultTypes.InternalError)
    {
        return result.Succeeded
            ? Result.Success()
            : Result.Failure(result.Errors.Select(e => e.Description), type);
    }
}
