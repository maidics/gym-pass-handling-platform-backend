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
        
        groupBuilder.MapGet(GetCountryAlpha2Codes, "CountryAlpha2Codes").RequireAuthorization();
    }

    public Ok<ImmutableArray<CurrencyRule>> GetCurrencyRules()
    {
        return TypedResults.Ok(CurrencyPolicies.Rules.Values);
    }

    public Ok<List<string>> GetCountryAlpha2Codes()
    {
        return TypedResults.Ok(Country.Lookup.Keys.ToList());
    } 
}
