using FitPass.Application.Common.Extensions;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Interfaces.Payment;
using FitPass.Application.Common.Logging;
using FitPass.Application.Common.Models;
using FitPass.Application.Common.Security;
using FitPass.Application.GymPassProducts.DTOs;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;
using FitPass.Domain.Enums;
using FitPass.Domain.Strings;
using FitPass.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace FitPass.Application.GymPassProducts.Commands;

[Authorize(Roles = Roles.GymAdministrator)]
public record CreateGymPassProductCommand
    (
        string Name,
        string Description,
        PassType Type,
        int? TotalUses,
        int? DaysAfterExpiring,
        bool IsActive,
        Money Price
    ) : IRequest<Result<GymPassProductDto>>;

public class CreateGymPassProductCommandValidator : AbstractValidator<CreateGymPassProductCommand>
{
    public CreateGymPassProductCommandValidator()
    {
        RuleFor(v => v.Name).NotEmptyWithMaxLenghtAndMessage(nameof(CreateGymPassProductCommand.Name), MaxStringLengths.Name);

        RuleFor(v => v.Description).NotEmptyWithMaxLenghtAndMessage(nameof(CreateGymPassProductCommand.Description), MaxStringLengths.Description);

        RuleFor(v => v.Type).NotEmptyWithMessage(nameof(CreateGymPassProductCommand.Type));

        When(v => v.Type == PassType.SingleUse, () =>
        {
            RuleFor(v => v.TotalUses)
                .NotEmptyWithMessage(nameof(CreateGymPassProductCommand.TotalUses))
                .Equal(1).WithMessage(ErrorMessages.SingleUsePassTypeOnlyOneUse());

            RuleFor(v => v.DaysAfterExpiring)
                .Null().WithMessage(ErrorMessages.SingleUsePassCannotExpire());
        });

        When(v => v.Type == PassType.MultiUse, () =>
        {
            RuleFor(v => v.TotalUses)
                .NotEmptyWithMessage(nameof(CreateGymPassProductCommand.TotalUses))
                .GreaterThan(1).WithMessage(ErrorMessages.MultiUsePassTypeAtLeastTwoUses());

            RuleFor(v => v.DaysAfterExpiring).Null().WithMessage(ErrorMessages.MultiUsePassCannotExpire());
        });

        When(v => v.Type == PassType.Unlimited, () =>
        {
            var now = DateTimeOffset.UtcNow;

            RuleFor(v => v.DaysAfterExpiring)
                .NotEmptyWithMessage(nameof(CreateGymPassProductCommand.DaysAfterExpiring))
                .GreaterThan(0).WithMessage(ErrorMessages.UnlimitedPassTypeExpirationDayAtleastOne());

            RuleFor(v => v.TotalUses).Null().WithMessage(ErrorMessages.UnlimitedPassTypeNoUses());
        });

        RuleFor(v => v.Price)
            .NotEmptyWithMessage(nameof(CreateGymPassProductCommand.Price));

        RuleFor(v => v.IsActive).NotEmptyWithMessage("Active status");
    }
}

public class CreateGymPassProductCommandHandler : IRequestHandler<CreateGymPassProductCommand, Result<GymPassProductDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    private readonly ILogger<CreateGymPassProductCommandHandler> _logger;
    private readonly IPaymentProductService _paymentProductService;
    private readonly IPaymentPriceService _paymentPriceService; 

    public CreateGymPassProductCommandHandler(
        IApplicationDbContext context,
        IUser user,
        ILogger<CreateGymPassProductCommandHandler> logger,
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

    public async Task<Result<GymPassProductDto>> Handle(CreateGymPassProductCommand command, CancellationToken cancellationToken)
    {
        var gymEmployment = await _context.GymEmployments
            .AsNoTracking()
            .FirstOrDefaultAsync(ge => ge.UserId == _user.Id);

        if (gymEmployment is null)
        {
            LogCriticalMessages.AuthenticatedUserRelatedEntityNotFound(
                _logger,
                _user.Roles,
                _user.Id,
                nameof(GymEmployment));

            return Result.InternalError([ErrorMessages.AuthenticatedUserRelatedEntityNotFound(nameof(GymEmployment))]);
        }

        //TODO: ensure atomicity

        var productResult = await _paymentProductService.CreateProduct(command.Name, command.Description, command.Type);

        if (!productResult.Succeeded)
        {
            return productResult.ToFailure<GymPassProductDto>();
        }

        var priceResult = await _paymentPriceService.CreatePrice(productResult.Value, command.Price);

        if (!priceResult.Succeeded)
        {
            return priceResult.ToFailure<GymPassProductDto>();
        }

        var product = new GymPassProduct
        {
            GymId = gymEmployment.GymId,
            Name = command.Name,
            Description = command.Description,
            Type = command.Type,
            TotalUses = command.TotalUses,
            DaysAfterExpiring = command.DaysAfterExpiring,
            IsActive = command.IsActive,
            Price = command.Price,
            PaymentProviderProductId = productResult.Value,
            PaymentProviderPriceId = priceResult.Value
        };

        await _context.GymPassProducts.AddAsync(product);
        await _context.SaveChangesAsync();

        return Result.Success(product.MapToDto());
    }
}
