
using FitPass.Application.Common.Extensions;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Interfaces.Payment;
using FitPass.Application.Common.Logging;
using FitPass.Application.Common.Models;
using FitPass.Application.Common.Security;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;
using FitPass.Domain.Strings;
using FitPass.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace FitPass.Application.GymPassProducts.Commands;

[Authorize(Roles = Roles.GymAdministrator)]
public record UpdateGymPassProductCommand(
    string GymPassProductId,
    string Name,
    string Description,
    int? TotalUses,
    int? DaysAfterExpiring,
    bool IsActive,
    Money Price
) : IRequest<Result>;

public class UpdateGymPassProductCommandValidator : AbstractValidator<UpdateGymPassProductCommand>
{
    public UpdateGymPassProductCommandValidator()
    {
        RuleFor(v => v.Name).NotEmptyWithMaxLenghtAndMessage(nameof(UpdateGymPassProductCommand.Name), MaxStringLengths.Name);

        RuleFor(v => v.Description).NotEmptyWithMaxLenghtAndMessage(nameof(UpdateGymPassProductCommand.Description), MaxStringLengths.Description);

        When(v => v.TotalUses is not null, () =>
        {
            RuleFor(v => v.TotalUses)
                .NotEmptyWithMessage(nameof(CreateGymPassProductCommand.TotalUses))
                .GreaterThan(1).WithMessage(ErrorMessages.MultiUsePassTypeAtLeastTwoUses());

            RuleFor(v => v.DaysAfterExpiring).Null().WithMessage(ErrorMessages.MultiUsePassCannotExpire());
        });

        When(v => v.DaysAfterExpiring is not null, () =>
        {
           RuleFor(v => v.DaysAfterExpiring)
                .NotEmptyWithMessage(nameof(UpdateGymPassProductCommand.DaysAfterExpiring))
                .GreaterThan(0).WithMessage(ErrorMessages.UnlimitedPassTypeExpirationDayAtleastOne());

           RuleFor(v => v.TotalUses).Null().WithMessage(ErrorMessages.UnlimitedPassTypeNoUses()); 
        });

        RuleFor(v => v.IsActive).NotEmptyWithMessage(nameof(UpdateGymPassProductCommand.IsActive));

        RuleFor(v => v.Price).NotEmptyWithMessage(nameof(UpdateGymPassProductCommand.Price));
    }
}

public class UpdateGymPassProductCommandHandler : IRequestHandler<UpdateGymPassProductCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    private readonly ILogger<UpdateGymPassProductCommandHandler> _logger;
    private readonly IPaymentPriceService _paymentPriceService;
    
    public UpdateGymPassProductCommandHandler(
        IApplicationDbContext context,
        IUser user,
        ILogger<UpdateGymPassProductCommandHandler> logger,
        IPaymentPriceService paymentPriceService
    )
    {
        _context = context;
        _user = user;
        _logger = logger;
        _paymentPriceService = paymentPriceService;
    }

    public async Task<Result> Handle(UpdateGymPassProductCommand command, CancellationToken cancellationToken)
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

        var product = await _context.GymPassProducts.FindAsync(command.GymPassProductId);

        if (product is null)
        {
            return Result.NotFound(nameof(GymPassProduct));
        }

        if (product.GymId != gymEmployment.GymId)
        {
            return Result.Forbidden();
        }

        if (product.Price != command.Price)
        {
            var result = await _paymentPriceService.UpdatePriceAsync(
                product.PaymentProviderPriceId, 
                product.PaymentProviderProductId, 
                command.Price);

            if (!result.Succeeded)
            {
                return result;
            }

            product.Price = command.Price;
            product.PaymentProviderPriceId = result.Value;
        }

        if (product.IsActive != command.IsActive)
        {
            var result = await _paymentPriceService.SetActiveFlagAsync(product.PaymentProviderPriceId, command.IsActive);

            if (!result.Succeeded)
            {
                return result;
            }

            product.IsActive = command.IsActive;
        }

        product.Name = command.Name;
        product.Description = command.Description;
        product.TotalUses = command.TotalUses;
        product.DaysAfterExpiring = command.DaysAfterExpiring;

        await _context.SaveChangesAsync();

        return Result.Success();
    }
}