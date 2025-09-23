using System.Security.Claims;

using FitPass.Application.Common.Interfaces;

namespace FitPass.Web.Services;

public class CurrentUser : IUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUser(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? Id => _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
    public List<string>? Roles => _httpContextAccessor.HttpContext?.User?.FindAll(ClaimTypes.Role).Select(x => x.Value).ToList();

    public void ThrowIfIdNull()
    {
        if (Id == null)
        {
            throw new UnauthorizedAccessException("You must log in for this action.");
        }
    }
}
