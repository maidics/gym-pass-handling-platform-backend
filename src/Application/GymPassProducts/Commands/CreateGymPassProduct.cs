using FitPass.Application.Common.Security;
using FitPass.Application.Extensions;
using FitPass.Domain.Constants;
using FitPass.Domain.Enums;

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
        RuleFor(v => v.GymPassProductName).NotEmptyWithMaxLenghtAndMessage(MaxStringLengths.Name, "Pass name");

        RuleFor(v => v.GymPassProductDescription).NotEmptyWithMaxLenghtAndMessage(MaxStringLengths.Description, "Pass description");

        RuleFor(v => v.GymPassProductType).NotEmptyWithMessage("Pass type");

        When(v => v.GymPassProductType == PassType.SingleUse, () =>
        {
            RuleFor(v => v.TotalUses)
                .NotEmptyWithMessage("Total uses")
                .Equal(1).WithMessage("Single use pass type must only have one total use.");

            RuleFor(v => v.DaysAfterExpiring)
                .Null().WithMessage("Multi use pass cannot expire.");
        });

        When(v => v.GymPassProductType == PassType.MultiUse, () =>
        {
            RuleFor(v => v.TotalUses)
                .NotEmptyWithMessage("Total uses")
                .GreaterThan(1).WithMessage("Multi use pass type must have at least two total uses.");

            RuleFor(v => v.DaysAfterExpiring).Null().WithMessage("Multi use pass cannot expire.");
        });

        When(v => v.GymPassProductType == PassType.Unlimited, () =>
        {
            var now = DateTimeOffset.UtcNow;

            RuleFor(v => v.DaysAfterExpiring)
                .NotEmptyWithMessage("Expiration date")
                .GreaterThan(0).WithMessage("Pass must expire after at least 1 day.");

            RuleFor(v => v.TotalUses).Null().WithMessage("Subscription pass type cannot have total uses.");
        });

        RuleFor(v => v.HUFPrice)
            .NotEmptyWithMessage("HUF price")
            .GreaterThan(0).WithMessage("HUF price must be bigger than 0");

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