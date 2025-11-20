
using FitPass.Application.TenantPaymentProfiles.Commands;
using FitPass.Application.TenantPaymentProfiles.Queries;

namespace FitPass.Web.Endpoints;

public class TenantPaymentProfiles : EndpointGroupBase
{
    public override void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapPost(CreateTenantPaymentProfile).RequireAuthorization();

        groupBuilder.MapGet(GenerateTenantPaymentAccountLink, "AccountLink").RequireAuthorization();

        groupBuilder.MapGet(GetTenantPaymentProfile).RequireAuthorization();
    }

    public async Task<IResult> CreateTenantPaymentProfile(ISender sender, CreateTenantPaymentProfileCommand command, CancellationToken cancellationToken)
    {
        var result = await sender.Send(command);

        return result.ToTypedResult();
    }

    public async Task<IResult> GenerateTenantPaymentAccountLink(ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GenerateTenantPaymentAccountLinkCommand(), cancellationToken);

        return result.ToTypedResult();
    }

    public async Task<IResult> GetTenantPaymentProfile(ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetTenantPaymentProfileQuery());

        return result.ToTypedResult();
    }
}
