using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Models;
using FitPass.Application.Common.Security;
using FitPass.Application.Extensions;
using FitPass.Domain.Constants;
using FitPass.Domain.Enums;

namespace Fitpass.Application.GymPassProductsTemplates.Commands;

[Authorize(Roles = Roles.AppAdministrator)]
public record UpdateGymPassProductTemplateCommand
(
    string GymPassProductTemplateId,
    GymTier GymTier,
    PassType PassType,
    int? TotalUses,
    int? DaysAfterExpiring,
    decimal EurPrice
) : IRequest<Result>;

public class UpdateGymPassProductTemplateCommandValidator : AbstractValidator<UpdateGymPassProductTemplateCommand>
{
    public UpdateGymPassProductTemplateCommandValidator()
    {
        RuleFor(v => v.GymPassProductTemplateId).NotEmptyWithMessage("Gym pass product template id");

        RuleFor(v => v.GymTier).NotEmptyWithMessage("Gym tier");

        RuleFor(v => v.PassType).NotEmptyWithMessage("Pass type");

        When(v => v.PassType == PassType.SingleUse, () =>
        {
            RuleFor(v => v.TotalUses)
                .NotEmptyWithMessage("Total uses")
                .Equal(1).WithMessage("Single use pass type must only have one total use.");

            RuleFor(v => v.DaysAfterExpiring)
                .Null().WithMessage("Multi use pass cannot expire.");
        });

        When(v => v.PassType == PassType.MultiUse, () =>
        {
            RuleFor(v => v.TotalUses)
                .NotEmptyWithMessage("Total uses")
                .GreaterThan(1).WithMessage("Multi use pass type must have at least two total uses.");

            RuleFor(v => v.DaysAfterExpiring).Null().WithMessage("Multi use pass cannot expire.");
        });

        When(v => v.PassType == PassType.Subscription, () =>
        {
            var now = DateTimeOffset.UtcNow;

            RuleFor(v => v.DaysAfterExpiring)
                .NotEmptyWithMessage("Expiration date")
                .GreaterThan(0).WithMessage("Pass must expire after at least 1 day.");

            RuleFor(v => v.TotalUses).Null().WithMessage("Subscription pass type cannot have total uses.");
        });

        RuleFor(v => v.EurPrice)
            .GreaterThan(0).WithMessage("Eur price must be bigger than 0.");
    }
}

public class UpdateGymPassProductTemplateCommandHandler : IRequestHandler<UpdateGymPassProductTemplateCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public UpdateGymPassProductTemplateCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(UpdateGymPassProductTemplateCommand command, CancellationToken cancellationToken)
    {
        var template = await _context.GymPassProductTemplates.FindAsync(command.GymPassProductTemplateId, cancellationToken);

        if (template == null)
        {
            return Result.Failure(["Gym pass product template not found."]);
        }

        var existingTemplate = await _context
            .GymPassProductTemplates
            .AsNoTracking()
            .FirstOrDefaultAsync(gppt =>
                gppt.GymTier == command.GymTier &&
                gppt.PassType == command.PassType &&
                gppt.TotalUses == command.TotalUses &&
                gppt.DaysAfterExpiring == command.DaysAfterExpiring &&
                gppt.EurPrice == command.EurPrice,
                cancellationToken
            );

        if (existingTemplate != null)
        {
            return Result.Failure([$"A pass template like this already exists."]);
        }

        template.GymTier = command.GymTier;
        template.PassType = command.PassType;
        template.TotalUses = command.TotalUses;
        template.DaysAfterExpiring = command.DaysAfterExpiring;
        template.EurPrice = command.EurPrice;

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}