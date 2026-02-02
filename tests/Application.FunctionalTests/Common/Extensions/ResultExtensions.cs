using FitPass.Application.Common.Models;

namespace FitPass.Application.FunctionalTests.Common.Extensions;

public static class ResultExtensions
{
    extension(Result result)
    {
        public void ShouldBeFailed(ResultTypes type, bool shouldHaveMessage = true)
        {
            if (type == ResultTypes.Success)
            {
                throw new InvalidOperationException(
                    $"{nameof(ResultTypes.Success)} ${nameof(ResultTypes)} passed."
                );
            }

            result.Type.ShouldBe(type);
            result.Succeeded.ShouldBeFalse();

            if (shouldHaveMessage)
            {
                result.Message.ShouldNotBeNullOrEmpty();
            }
        }

        public void ShouldBeSuccessful()
        {
            result.Type.ShouldBe(ResultTypes.Success);
            result.Succeeded.ShouldBeTrue();
        }
    }
}
