using FitPass.Application.Common.Extensions;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Interfaces.Payment;
using FitPass.Application.Common.Models;
using FitPass.Application.Common.Security;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;
using FitPass.Domain.Entities.Payment;
using FitPass.Domain.Enums;
using FitPass.Domain.Strings;
using FitPass.Domain.ValueObjects;

namespace FitPass.Application.GymPassProducts.Commands;

[Authorize(Roles = Roles.GymAdministrator)]
public record UpdateGymPassProductCommand(
    string GymPassProductId,
    string Name,
    string Description,
    int? TotalUses,
    int? DaysAfterExpiring,
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

        RuleFor(v => v.Price).NotEmptyWithMessage(nameof(UpdateGymPassProductCommand.Price));
    }
}

public class UpdateGymPassProductCommandHandler : IRequestHandler<UpdateGymPassProductCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    private readonly IPaymentPriceService _paymentPriceService;
    private readonly IPaymentProductService _paymentProductService;
    private readonly TimeProvider _timeProvider;
    
    public UpdateGymPassProductCommandHandler(
        IApplicationDbContext context,
        IUser user,
        IPaymentPriceService paymentPriceService,
        IPaymentProductService paymentProductService,
        TimeProvider timeProvider
    )
    {
        _context = context;
        _user = user;
        _paymentPriceService = paymentPriceService;
        _paymentProductService = paymentProductService;
        _timeProvider = timeProvider;
    }

    public async Task<Result> Handle(UpdateGymPassProductCommand command, CancellationToken cancellationToken)
    {
        var moneyValidationResult = _paymentPriceService.ValidateMoney(command.Price);

        if (!moneyValidationResult.Succeeded)
        {
            return moneyValidationResult;
        }
        
        var gymEmployment = await _context.GymEmployments
            .AsNoTracking()
            .FirstOrDefaultAsync(ge => ge.UserId != null && ge.UserId == _user.Id);

        Guard.Against.NullEntityRelatedToCurrentUser(gymEmployment, nameof(GymEmployment), _user.Id);

        var tenantPaymentProfile = await _context.TenantPaymentProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.GymId == gymEmployment.GymId);

        Guard.Against.Null(tenantPaymentProfile, nameof(TenantPaymentProfile), "No payment profile found when trying to update GymPassProduct.");

        var product = await _context.GymPassProducts
            .Include(gpp => gpp.PaymentIdentity)
            .FirstOrDefaultAsync(gpp => gpp.Id == command.GymPassProductId && gpp.GymId == gymEmployment.GymId);

        if (product is null)
        {
            return Result.NotFound(nameof(GymPassProduct));
        }

        if (product.Price != command.Price)
        {
            var result = await _paymentPriceService.UpdatePriceAsync( //new price created
                product.PaymentIdentity.PriceId, 
                product.PaymentIdentity.Id, 
                command.Price,
                product.IsActive,
                tenantPaymentProfile.PaymentAccountId);

            if (!result.Succeeded)
            {
                return result;
            }

            product.PaymentIdentity.ArchivedPaymentProviderPrices.Add(
                new ArchivedPaymentProviderPrice 
                { 
                    Id = product.PaymentIdentity.PriceId, 
                    ArchivedOn = _timeProvider.GetUtcNow() 
                });

            product.Price = command.Price;
            product.PaymentIdentity.PriceId = result.Value;
        }

        if (product.Name != command.Name || product.Description != command.Description)
        {
            var result = await _paymentProductService.UpdateProductAsync(
                productId: product.PaymentIdentity.Id, 
                name: command.Name, 
                description: command.Description);

            if (!result.Succeeded)
            {
                return result;
            }

            product.Name = command.Name;
            product.Description = command.Description;
        }

        product.UpdateTotalUsesIfApplicable((int)command.TotalUses!);
        product.UpdateDaysAfterExpiringIfApplicable((int)command.DaysAfterExpiring!);

        await _context.SaveChangesAsync();

        return Result.Success();
    }
}
