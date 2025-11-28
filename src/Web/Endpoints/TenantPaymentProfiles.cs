
using FitPass.Application.TenantPaymentProfiles.Commands;
using FitPass.Application.TenantPaymentProfiles.Queries;

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

    public async Task<IResult> CreateTenantPaymentProfile(ISender sender, CreateTenantPaymentProfileCommand command, CancellationToken cancellationToken)
    {
        var result = await sender.Send(command);

        return result.ToTypedResult();
    }

    public async Task<IResult> GenerateTenantLoginLink(ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GenerateTenantLoginLinkCommand(), cancellationToken);

        return result.ToTypedResult();
    }

    public async Task<IResult> GetTenantPaymentProfile(ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetTenantPaymentProfileQuery());

        return result.ToTypedResult();
    }

    public async Task<IResult> UpdateTenantPaymentAccountPayoutSchedule(ISender sender, UpdateTenantPaymentAccountPayoutScheduleCommand command, CancellationToken cancellationToken)
    {
        var result = await sender.Send(command);

        return result.ToTypedResult();
    }
}
