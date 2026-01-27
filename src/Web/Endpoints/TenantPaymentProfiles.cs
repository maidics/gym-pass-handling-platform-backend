
using FitPass.Application.TenantPaymentProfiles.Commands;
using FitPass.Application.TenantPaymentProfiles.DTOs;
using FitPass.Application.TenantPaymentProfiles.Queries;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace FitPass.Web.Endpoints;

public class TenantPaymentProfiles : EndpointGroupBase
{
    public override void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapPost(CreateTenantPaymentProfile).RequireAuthorization();

        groupBuilder.MapPost(GeneratePaymentProviderLink, "PaymentProviderLink").RequireAuthorization();

        groupBuilder.MapGet(GetMyTenantPaymentProfile).RequireAuthorization();

        //groupBuilder.MapPut(UpdateTenantPaymentAccountPayoutSchedule, "PayoutSchedule").RequireAuthorization();
    }

    public async Task<Results<Ok<PaymentProviderLinkDto>, ProblemHttpResult>> CreateTenantPaymentProfile(
        ISender sender, CreateTenantPaymentProfileCommand command, CancellationToken cancellationToken)
    {
        var result = await sender.Send(command);

        return result.ToTypedResult();
    }

    public async Task<Results<Ok<PaymentProviderLinkDto>, ProblemHttpResult>> GeneratePaymentProviderLink(
        ISender sender, GeneratePaymentProviderLinkCommand command, CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);

        return result.ToTypedResult();
    }

    public async Task<Results<Ok<TenantPaymentProfileDto>, ProblemHttpResult>> GetMyTenantPaymentProfile(ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetMyTenantPaymentProfileQuery(), cancellationToken);

        return result.ToTypedResult();
    }

    // public async Task<Results<NoContent, ProblemHttpResult>> UpdateTenantPaymentAccountPayoutSchedule(
    //     ISender sender, UpdateTenantPaymentAccountPayoutScheduleCommand command, CancellationToken cancellationToken)
    // {
    //     var result = await sender.Send(command);
    //
    //     return result.ToTypedResult();
    // }
}
