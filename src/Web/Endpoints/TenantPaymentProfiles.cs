
using FitPass.Application.TenantPaymentProfiles.Commands;
using FitPass.Application.TenantPaymentProfiles.DTOs;
using FitPass.Application.TenantPaymentProfiles.Queries;
using Microsoft.AspNetCore.Http.HttpResults;

namespace FitPass.Web.Endpoints;

public class TenantPaymentProfiles : EndpointGroupBase
{
    public override void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapPost(CreateTenantPaymentProfile).RequireAuthorization();

        groupBuilder.MapGet(GenerateTenantLoginLink, "LoginLink").RequireAuthorization();

        groupBuilder.MapGet(GetTenantPaymentProfile).RequireAuthorization();

        groupBuilder.MapPut(UpdateTenantPaymentAccountPayoutSchedule, "PayoutSchedule").RequireAuthorization();
    }

    public async Task<Results<Ok<(string url, DateTimeOffset expiration)>, ProblemHttpResult>> CreateTenantPaymentProfile(
        ISender sender, CreateTenantPaymentProfileCommand command, CancellationToken cancellationToken)
    {
        var result = await sender.Send(command);

        return result.ToTypedResult();
    }

    public async Task<Results<Ok<string>, ProblemHttpResult>> GenerateTenantLoginLink(ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GenerateTenantLoginLinkCommand(), cancellationToken);

        return result.ToTypedResult();
    }

    public async Task<Results<Ok<TenantPaymentProfileDto>, ProblemHttpResult>> GetTenantPaymentProfile(ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetTenantPaymentProfileQuery());

        return result.ToTypedResult();
    }

    public async Task<Results<NoContent, ProblemHttpResult>> UpdateTenantPaymentAccountPayoutSchedule(ISender sender, UpdateTenantPaymentAccountPayoutScheduleCommand command, CancellationToken cancellationToken)
    {
        var result = await sender.Send(command);

        return result.ToTypedResult();
    }
}
