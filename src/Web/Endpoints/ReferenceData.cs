using System.Collections.Immutable;
using FitPass.Domain.Constants;
using FitPass.Domain.ValueObjects;
using Microsoft.AspNetCore.Http.HttpResults;

namespace FitPass.Web.Endpoints;

public class ReferenceData : EndpointGroupBase
{
    public override void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet(GetCurrencyRules, "CurrencyRules").RequireAuthorization();
    }

    public Ok<ImmutableArray<CurrencyRule>> GetCurrencyRules()
    {
        return TypedResults.Ok(CurrencyPolicies.Rules.Values);
    }
}
