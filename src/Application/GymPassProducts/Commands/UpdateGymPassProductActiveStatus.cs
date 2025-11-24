using FitPass.Application.Common.Extensions;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Interfaces.Payment;
using FitPass.Application.Common.Logging;
using FitPass.Application.Common.Models;
using FitPass.Domain.Entities;
using FitPass.Domain.Strings;
using Microsoft.Extensions.Logging;

namespace FitPass.Application.GymPassProducts.Commands;

public record UpdateGymPassProductActiveStatusCommand(
    string GymPassProductId,
    bool IsActive
) : IRequest<Result>;

public class UpdateGymPassProductActiveStatusCommandValidator : AbstractValidator<UpdateGymPassProductActiveStatusCommand>
{
    public UpdateGymPassProductActiveStatusCommandValidator()
    {
        RuleFor(v => v.GymPassProductId).NotEmptyWithMessage(nameof(UpdateGymPassProductActiveStatusCommand.GymPassProductId));

        RuleFor(v => v.IsActive).NotEmptyWithMessage(nameof(UpdateGymPassProductActiveStatusCommand.IsActive));
    }
}

public class UpdateGymPassProductActiveStatusCommandHandler : IRequestHandler<UpdateGymPassProductActiveStatusCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    private readonly ILogger<UpdateGymPassProductActiveStatusCommandHandler> _logger;
    private readonly IPaymentProductService _paymentProductService;
    private readonly IPaymentPriceService _paymentPriceService;

    public UpdateGymPassProductActiveStatusCommandHandler(
        IApplicationDbContext context,
        IUser user,
        ILogger<UpdateGymPassProductActiveStatusCommandHandler> logger,
        IPaymentProductService paymentProductService,
        IPaymentPriceService paymentPriceService
    )
    {
        _context = context;
        _user = user;
        _logger = logger;
        _paymentProductService = paymentProductService;
        _paymentPriceService = paymentPriceService;
    }

    public async Task<Result> Handle(UpdateGymPassProductActiveStatusCommand command, CancellationToken cancellationToken)
    {
        var gymEmployment = await _context.GymEmployments
            .AsNoTracking()
            .FirstOrDefaultAsync(ge => ge.UserId != null && ge.UserId == _user.Id);

        if (gymEmployment is null)
        {
            LogCriticalMessages.AuthenticatedUserRelatedEntityNotFound(
                _logger,
                _user.Roles,
                _user.Id,
                nameof(GymEmployment));

            return Result.InternalError(ErrorMessages.AuthenticatedUserRelatedEntityNotFound(nameof(GymEmployment)));
        }

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