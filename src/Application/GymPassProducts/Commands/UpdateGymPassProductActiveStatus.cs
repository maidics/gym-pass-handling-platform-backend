using FitPass.Application.Common.Extensions;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Interfaces.Payment;
using FitPass.Application.Common.Models;
using FitPass.Application.Common.Security;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace FitPass.Application.GymPassProducts.Commands;

[Authorize(Roles = Roles.GymAdministrator)]
public record UpdateGymPassProductActiveStatusCommand(
    string GymPassProductId,
    bool IsActive
) : IRequest<Result>;

public class UpdateGymPassProductActiveStatusCommandValidator : AbstractValidator<UpdateGymPassProductActiveStatusCommand>
{
    public UpdateGymPassProductActiveStatusCommandValidator()
    {
        RuleFor(v => v.GymPassProductId).NotEmptyLocalized(nameof(UpdateGymPassProductActiveStatusCommand.GymPassProductId));

        RuleFor(v => v.IsActive).NotEmptyLocalized(nameof(UpdateGymPassProductActiveStatusCommand.IsActive));
    }
}

public class UpdateGymPassProductActiveStatusCommandHandler : IRequestHandler<UpdateGymPassProductActiveStatusCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    private readonly IPaymentProductService _paymentProductService;
    private readonly IPaymentPriceService _paymentPriceService;

    public UpdateGymPassProductActiveStatusCommandHandler(
        IApplicationDbContext context,
        IUser user,
        IPaymentProductService paymentProductService,
        IPaymentPriceService paymentPriceService
    )
    {
        _context = context;
        _user = user;
        _paymentProductService = paymentProductService;
        _paymentPriceService = paymentPriceService;
    }

    public async Task<Result> Handle(UpdateGymPassProductActiveStatusCommand command, CancellationToken cancellationToken)
    {
        var gymEmployment = await _context.GymEmployments
            .AsNoTracking()
            .FirstOrDefaultAsync(ge => ge.UserId == _user.Id);

        Guard.Against.NullParameterRelatedToCurrentUser(gymEmployment, nameof(GymEmployment), _user.Id);

        var product = await _context.GymPassProducts
            .Include(gpp => gpp.PaymentIdentity)
            .FirstOrDefaultAsync(gpp => gpp.Id == command.GymPassProductId && gpp.GymId == gymEmployment.GymId);

        if (product is null)
        {
            return Result.NotFound(nameof(GymPassProduct));
        }

        if (product.IsActive == command.IsActive)
        {
            return Result.Success();
        }

        var productResult = await _paymentProductService.UpdateProductAsync(
            product.PaymentIdentity.Id,
            isActive: command.IsActive);

        if (!productResult.Succeeded)
        {
            return productResult;
        }

        var priceResult = await _paymentPriceService.UpdateActiveStatusAsync(product.PaymentIdentity.PriceId, command.IsActive);

        if (!priceResult.Succeeded)
        {
            return priceResult;
        }

        product.IsActive = command.IsActive;

        return Result.Success();
    }
}
