using FitPass.Application.Common.Extensions;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Interfaces.Payment;
using FitPass.Application.Common.Models;
using FitPass.Application.Common.Resources;
using FitPass.Application.Common.Security;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;
using FitPass.Domain.Entities.Payment;

namespace FitPass.Application.GymPassProducts.Commands;

[Authorize(Roles = Roles.GymAdministrator)]
public record UpdateGymPassProductActiveStatusCommand(string GymPassProductId, bool IsActive)
    : IRequest<Result>;

public class UpdateGymPassProductActiveStatusCommandValidator
    : AbstractValidator<UpdateGymPassProductActiveStatusCommand>
{
    public UpdateGymPassProductActiveStatusCommandValidator(ILocalizer localizer)
    {
        RuleFor(v => v.GymPassProductId)
            .PropertyOfEntityNotEmptyWithMessageLocalized(
                localizer,
                nameof(SharedResource.Id),
                nameof(SharedResource.GymPassProduct)
            );
    }
}

public class UpdateGymPassProductActiveStatusCommandHandler
    : IRequestHandler<UpdateGymPassProductActiveStatusCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    private readonly IPaymentProductService _paymentProductService;
    private readonly IPaymentPriceService _paymentPriceService;
    private readonly ILocalizer _localizer;

    public UpdateGymPassProductActiveStatusCommandHandler(
        IApplicationDbContext context,
        IUser user,
        IPaymentProductService paymentProductService,
        IPaymentPriceService paymentPriceService,
        ILocalizer localizer
    )
    {
        _context = context;
        _user = user;
        _paymentProductService = paymentProductService;
        _paymentPriceService = paymentPriceService;
        _localizer = localizer;
    }

    public async Task<Result> Handle(
        UpdateGymPassProductActiveStatusCommand command,
        CancellationToken cancellationToken
    )
    {
        var gymId = await _context
            .GymEmployments.AsNoTracking()
            .Where(x => x.UserId == _user.Id)
            .Select(x => x.GymId)
            .FirstOrDefaultAsync(cancellationToken);

        Guard.Against.NullParameterRelatedToCurrentUser(
            gymId,
            $"{nameof(GymEmployment)}.{nameof(GymEmployment.GymId)}",
            _user.Id
        );

        var product = await _context
            .GymPassProducts.Include(gpp => gpp.PaymentIdentity)
            .FirstOrDefaultAsync(
                gpp => gpp.Id == command.GymPassProductId && gpp.GymId == gymId,
                cancellationToken
            );

        if (product is null)
        {
            return Result.NotFound(_localizer.GetNotFound(nameof(SharedResource.GymPassProduct)));
        }

        Guard.Against.Null(product.PaymentIdentity, nameof(ProductPaymentIdentity));

        if (product.IsActive == command.IsActive)
        {
            return Result.Success();
        }

        var paymentAccountId = await _context
            .TenantPaymentProfiles.AsNoTracking()
            .Where(x => x.GymId == gymId)
            .Select(x => x.PaymentAccountId)
            .FirstOrDefaultAsync(cancellationToken);

        Guard.Against.Null(paymentAccountId); //this should exist if the product exists

        var productResult = await _paymentProductService.UpdateProductAsync(
            product.PaymentIdentity.ProductId,
            paymentAccountId,
            isActive: command.IsActive
        );

        if (!productResult.Succeeded)
        {
            return productResult;
        }

        var priceResult = await _paymentPriceService.UpdateActiveStatusAsync(
            product.PaymentIdentity.PriceId,
            paymentAccountId,
            command.IsActive
        );

        if (!priceResult.Succeeded)
        {
            return priceResult;
        }

        product.IsActive = command.IsActive;

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
