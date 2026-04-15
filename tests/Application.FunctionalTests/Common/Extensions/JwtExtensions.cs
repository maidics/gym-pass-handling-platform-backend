using FitPass.Application.Users.DTOs;

namespace FitPass.Application.FunctionalTests.Common.Extensions;

public static class JwtExtensions
{
    extension(Jwt jwt)
    {
        public void ShouldBeValid()
        {
            jwt.ShouldNotBeNull();
            jwt.AccessToken.ShouldNotBeNullOrEmpty();
        }
    }
}
