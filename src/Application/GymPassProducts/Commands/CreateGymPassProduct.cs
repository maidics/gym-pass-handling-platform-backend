using FitPass.Application.Common.Extensions;
using FitPass.Application.Common.Security;
using FitPass.Domain.Constants;
using FitPass.Domain.Enums;
using FitPass.Domain.Strings;

namespace Fitpass.Application.GymPassProducts.Commands;

[Authorize(Roles = Roles.GymAdministrator)]
public record CreateGymPassProductCommand
    (
        string GymPassProductName,
        string GymPassProductDescription,
        PassType GymPassProductType,
        int? TotalUses,
        int? DaysAfterExpiring,
        decimal HUFPrice,
        bool IsActive
    ) : IRequest;

public class CreateGymPassProductCommandValidator : AbstractValidator<CreateGymPassProductCommand>
{
    public CreateGymPassProductCommandValidator()
    {
        RuleFor(v => v.GymPassProductName).NotEmptyWithMaxLenghtAndMessage(nameof(CreateGymPassProductCommand.GymPassProductName), MaxStringLengths.Name);

        RuleFor(v => v.GymPassProductDescription).NotEmptyWithMaxLenghtAndMessage(nameof(CreateGymPassProductCommand.GymPassProductDescription), MaxStringLengths.Description);

        RuleFor(v => v.GymPassProductType).NotEmptyWithMessage(nameof(CreateGymPassProductCommand.GymPassProductType));

        When(v => v.GymPassProductType == PassType.SingleUse, () =>
        {
            RuleFor(v => v.TotalUses)
                .NotEmptyWithMessage(nameof(CreateGymPassProductCommand.TotalUses))
                .Equal(1).WithMessage(ErrorMessages.SingleUsePassTypeOnlyOneUse());

            RuleFor(v => v.DaysAfterExpiring)
                .Null().WithMessage(ErrorMessages.SingleUsePassCannotExpire());
        });

        When(v => v.GymPassProductType == PassType.MultiUse, () =>
        {
            RuleFor(v => v.TotalUses)
                .NotEmptyWithMessage(nameof(CreateGymPassProductCommand.TotalUses))
                .GreaterThan(1).WithMessage(ErrorMessages.MultiUsePassTypeAtLeastTwoUses());

            RuleFor(v => v.DaysAfterExpiring).Null().WithMessage(ErrorMessages.MultiUsePassCannotExpire());
        });

        When(v => v.GymPassProductType == PassType.Unlimited, () =>
        {
            var now = DateTimeOffset.UtcNow;

            RuleFor(v => v.DaysAfterExpiring)
                .NotEmptyWithMessage(nameof(CreateGymPassProductCommand.DaysAfterExpiring))
                .GreaterThan(0).WithMessage(ErrorMessages.UnlimitedPassTypeExpirationDayAtleastOne());

            RuleFor(v => v.TotalUses).Null().WithMessage(ErrorMessages.UnlimitedPassTypeNoUses());
        });

        RuleFor(v => v.HUFPrice)
            .NotEmptyWithMessage(nameof(CreateGymPassProductCommand.HUFPrice))
            .GreaterThan(0).WithMessage(ErrorMessages.PriceMustBePositive(nameof(CreateGymPassProductCommand.HUFPrice)));

        RuleFor(v => v.IsActive).NotEmptyWithMessage("Active status");
    }
}

public class CreateGymPassProductCommandHandler : IRequestHandler<CreateGymPassProductCommand>
{
    public Task Handle(CreateGymPassProductCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
