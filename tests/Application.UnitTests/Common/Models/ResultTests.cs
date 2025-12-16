
using FitPass.Application.Common.Models;
using FitPass.Domain.Entities;
using NUnit.Framework;
using Shouldly;

namespace FitPass.Application.UnitTests.Common.Models;

public class ResultTests
{
    [Test]
    public void ShouldReturnSuccessResult()
    {
        string genericValue = "value";

        var nonGenericResult = Result.Success();
        var genericResult = Result.Success(genericValue);

        nonGenericResult.ShouldSatisfyAllConditions(
            () => nonGenericResult.Succeeded.ShouldBeTrue(),
            () => nonGenericResult.Errors.ShouldBeEmpty(),
            () => nonGenericResult.Type.ShouldBe(ResultTypes.Success));

        genericResult.ShouldSatisfyAllConditions(
            () => genericResult.Succeeded.ShouldBeTrue(),
            () => genericResult.Errors.ShouldBeEmpty(),
            () => genericResult.Type.ShouldBe(ResultTypes.Success),
            () => genericResult.Value.ShouldBe(genericValue));
    }

    [Test]
    public void GenericSuccessFactoryMethodShouldThrowWhenValueIsNull()
    {
        string? value = default; 

        Should.Throw<ArgumentNullException>(() => Result.Success(value));
    }

    [Test]
    public void ShouldImplicitlyConvertResultFailureToResult()
    {
        var notFound = Result.NotFound(nameof(Gym));

        Result nonGenericResult = notFound;

        nonGenericResult.ShouldNotBeNull();
        nonGenericResult.Message.ShouldNotBeEmpty();
        nonGenericResult.Errors.ShouldBeEquivalentTo(notFound.Errors);
        nonGenericResult.Succeeded.ShouldBeFalse();

        Result<string> genericResult = notFound;

        genericResult.ShouldNotBeNull();
        genericResult.Message.ShouldNotBeEmpty();
        genericResult.Errors.ShouldBeEquivalentTo(notFound.Errors);
        genericResult.Succeeded.ShouldBeFalse();
    }

    [Test]
    public void ToFailureMethodShouldConvertToAnotherGenericResult()
    {
        static Result<string> GetFailedResult() => Result.Unauthorized(string.Empty);

        var result1 = GetFailedResult();

        var result2 = result1.ToFailure<int>();

        result2.Succeeded.ShouldBe(result1.Succeeded);
        result2.Message.ShouldBe(result1.Message);
        result2.Errors.ShouldBeEquivalentTo(result1.Errors);
    }

    [Test]
    public void FailedGenericResultShouldThrowWhenValueAccessed()
    {
        static Result<string> GetFailedResult() => Result.Forbidden(string.Empty);

        var result = GetFailedResult();

        var action = () => result.Value.Trim();

        var ex = Should.Throw<InvalidOperationException>(action);
        ex.Message.ShouldContain("Failed result does not have inner value");
    }

    [Test]
    public void ToFailureMethodShouldThrowIfResultSucceeded()
    {
        var result = Result.Success("success");

        var action = () => result.ToFailure<int>();

        var ex = Should.Throw<InvalidOperationException>(action);
        ex.Message.ShouldContain("Cannot convert to new Result failure when Result is succeeded.");
    }

    [Test]
    public void ShouldCreateResultFailure()
    {
        var resultFailure1 = new ResultFailure(ResultTypes.NotFound, "Resource not found.", ["failure"]);

        resultFailure1.Type.ShouldBe(ResultTypes.NotFound);
        resultFailure1.Message.ShouldContain("Resource not found");
        resultFailure1.Errors.Length.ShouldBe(1);
        resultFailure1.Errors.ShouldContain("failure");


        var resultFailure2 = new ResultFailure(Result.NotFound(nameof(UserProfile)));

        resultFailure2.Type.ShouldBe(ResultTypes.NotFound);
        resultFailure2.Message.ShouldContain("not found");
        resultFailure2.Errors.Length.ShouldBe(0);
    }

    [Test]
    public void ResultFailureConstructorShouldThrowIfPassedResultIsSuccess()
    {
        var result = Result.Success();

        var ex = Should.Throw<InvalidOperationException>(() => new ResultFailure(result));
        ex.Message.ShouldContain("Cannot create");
    }

    [Test]
    public void ShouldCreateNotFound()
    {
        var notFound = Result.NotFound(nameof(GymEmployment), ["failure1", "failure2"]);
        notFound.Message.ShouldContain("not found");
        notFound.Type.ShouldBe(ResultTypes.NotFound);
        notFound.Errors.Length.ShouldBe(2);
        notFound.Errors.ShouldSatisfyAllConditions(
            () => notFound.Errors.ShouldContain("failure1"),
            () => notFound.Errors.ShouldContain("failure2"));
    }

    [Test]
    public void ShouldCreateConflict()
    {
        var conflict = Result.Conflict(nameof(GymEmployment), ["failure1", "failure2"]);
        conflict.Message.ShouldContain("is already taken");
        conflict.Type.ShouldBe(ResultTypes.Conflict);
        conflict.Errors.Length.ShouldBe(2);
        conflict.Errors.ShouldSatisfyAllConditions(
            () => conflict.Errors.ShouldContain("failure1"),
            () => conflict.Errors.ShouldContain("failure2"));
    }

    [Test]
    public void ShouldCreateExternalServiceUnavailable()
    {
        var notFound = Result.ExternalServiceUnavailable("service", ["failure1", "failure2"]);
        notFound.Message.ShouldContain("is currently not available");
        notFound.Type.ShouldBe(ResultTypes.ExternalServiceUnavailable);
        notFound.Errors.Length.ShouldBe(2);
        notFound.Errors.ShouldSatisfyAllConditions(
            () => notFound.Errors.ShouldContain("failure1"),
            () => notFound.Errors.ShouldContain("failure2"));
    }

    [Test]
    public void ShouldCreateInternalError()
    {
        var notFound = Result.InternalError("Failed to finish action.", ["failure1", "failure2"]);
        notFound.Message.ShouldContain("Failed to finish action");
        notFound.Type.ShouldBe(ResultTypes.InternalError);
        notFound.Errors.Length.ShouldBe(2);
        notFound.Errors.ShouldSatisfyAllConditions(
            () => notFound.Errors.ShouldContain("failure1"),
            () => notFound.Errors.ShouldContain("failure2"));
    }

    [Test]
    public void ShouldCreatePaymentRequired()
    {
        var notFound = Result.PaymentRequired(string.Empty);
        notFound.Message.ShouldContain("Payment required");
        notFound.Type.ShouldBe(ResultTypes.PaymentRequired);
    }

    [Test]
    public void ShouldCreateUnauthorized()
    {
        var notFound = Result.Unauthorized("message");
        notFound.Message.ShouldContain("message");
        notFound.Type.ShouldBe(ResultTypes.Unauthorized);
        notFound.Errors.Length.ShouldBe(0);
    }

    [Test]
    public void ShouldCreateForbidden()
    {
        var notFound = Result.Forbidden("message");
        notFound.Message.ShouldContain("message");
        notFound.Type.ShouldBe(ResultTypes.Forbidden);
        notFound.Errors.Length.ShouldBe(0);
    }
}
