using Fitpass.Application.Common.Exceptions;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Models;
using FitPass.Application.Common.Security;
using FitPass.Application.Extensions;
using FitPass.Domain.Constants;
using FitPass.Domain.Enums;
using FitPass.Domain.Strings;

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
) : IRequest;

public class UpdateGymPassProductTemplateCommandValidator : AbstractValidator<UpdateGymPassProductTemplateCommand>
{
    public UpdateGymPassProductTemplateCommandValidator()
    {
        RuleFor(v => v.GymPassProductTemplateId).NotEmptyWithMessage(nameof(UpdateGymPassProductTemplateCommand.GymPassProductTemplateId));

        RuleFor(v => v.GymTier).NotEmptyWithMessage(nameof(UpdateGymPassProductTemplateCommand.GymTier));

        RuleFor(v => v.PassType).NotEmptyWithMessage(nameof(UpdateGymPassProductTemplateCommand.PassType));

        When(v => v.PassType == PassType.SingleUse, () =>
        {
            RuleFor(v => v.TotalUses)
                .NotEmptyWithMessage(nameof(UpdateGymPassProductTemplateCommand.TotalUses))
                .Equal(1).WithMessage(ErrorMessages.SingleUsePassTypeOnlyOneUse());

            RuleFor(v => v.DaysAfterExpiring)
                .Null().WithMessage(ErrorMessages.SingleUsePassCannotExpire());
        });

        When(v => v.PassType == PassType.MultiUse, () =>
        {
            RuleFor(v => v.TotalUses)
                .NotEmptyWithMessage(nameof(UpdateGymPassProductTemplateCommand.TotalUses))
                .GreaterThan(1).WithMessage(ErrorMessages.MultiUsePassTypeAtLeastTwoUses());

            RuleFor(v => v.DaysAfterExpiring).Null().WithMessage(ErrorMessages.MultiUsePassCannotExpire());
        });

        When(v => v.PassType == PassType.Unlimited, () =>
        {
            var now = DateTimeOffset.UtcNow;

            RuleFor(v => v.DaysAfterExpiring)
                .NotEmptyWithMessage(nameof(UpdateGymPassProductTemplateCommand.DaysAfterExpiring))
                .GreaterThan(0).WithMessage(ErrorMessages.UnlimitedPassTypeExpirationDayAtleastOne());

            RuleFor(v => v.TotalUses).Null().WithMessage(ErrorMessages.UnlimitedPassTypeNoUses());
        });

        RuleFor(v => v.EurPrice)
            .GreaterThan(0).WithMessage(ErrorMessages.PriceMustBePositive(nameof(UpdateGymPassProductTemplateCommand.EurPrice)));
    }
}

public class UpdateGymPassProductTemplateCommandHandler : IRequestHandler<UpdateGymPassProductTemplateCommand>
{
    private readonly IApplicationDbContext _context;

    public UpdateGymPassProductTemplateCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(UpdateGymPassProductTemplateCommand command, CancellationToken cancellationToken)
    {
        var template = await _context.GymPassProductTemplates.FindAsync(command.GymPassProductTemplateId, cancellationToken);

        Guard.Against.NotFound(command.GymPassProductTemplateId, template, "Id");

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
            throw new ConflictException("A pass template like this already exists.");
        }

        template.GymTier = command.GymTier;
        template.PassType = command.PassType;
        template.TotalUses = command.TotalUses;
        template.DaysAfterExpiring = command.DaysAfterExpiring;
        template.EurPrice = command.EurPrice;

        await _context.SaveChangesAsync();
    }
}
