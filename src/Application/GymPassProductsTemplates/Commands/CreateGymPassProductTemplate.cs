using Fitpass.Application.Common.Exceptions;
using FitPass.Application.Common.Extensions;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Models;
using FitPass.Application.Common.Security;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;
using FitPass.Domain.Enums;
using FitPass.Domain.Strings;

namespace Fitpass.Application.GymPassProductsTemplates.Commands;

[Authorize(Roles = $"{Roles.AppAdministrator}")]
public record CreateGymPassProductTemplateCommand
(
    GymTier GymTier,
    PassType PassType,
    int? TotalUses,
    int? DaysAfterExpiring,
    decimal EurPrice
) : IRequest<Result>;

public class CreateGymPassProductTemplateCommandValidator : AbstractValidator<CreateGymPassProductTemplateCommand>
{
    public CreateGymPassProductTemplateCommandValidator()
    {
        RuleFor(v => v.GymTier).NotEmptyWithMessage(nameof(CreateGymPassProductTemplateCommand.GymTier));

        RuleFor(v => v.PassType).NotEmptyWithMessage(nameof(CreateGymPassProductTemplateCommand.PassType));

        When(v => v.PassType == PassType.SingleUse, () =>
        {
            RuleFor(v => v.TotalUses)
                .NotEmptyWithMessage(nameof(CreateGymPassProductTemplateCommand.TotalUses))
                .Equal(1).WithMessage(ErrorMessages.SingleUsePassTypeOnlyOneUse());

            RuleFor(v => v.DaysAfterExpiring)
                .Null().WithMessage(ErrorMessages.SingleUsePassCannotExpire());
        });

        When(v => v.PassType == PassType.MultiUse, () =>
        {
            RuleFor(v => v.TotalUses)
                .NotEmptyWithMessage(nameof(CreateGymPassProductTemplateCommand.TotalUses))
                .GreaterThan(1).WithMessage(ErrorMessages.MultiUsePassTypeAtLeastTwoUses());

            RuleFor(v => v.DaysAfterExpiring).Null().WithMessage(ErrorMessages.MultiUsePassCannotExpire());
        });

        When(v => v.PassType == PassType.Unlimited, () =>
        {
            var now = DateTimeOffset.UtcNow;

            RuleFor(v => v.DaysAfterExpiring)
                .NotEmptyWithMessage(nameof(CreateGymPassProductTemplateCommand.DaysAfterExpiring))
                .GreaterThan(0).WithMessage(ErrorMessages.UnlimitedPassTypeExpirationDayAtleastOne());

            RuleFor(v => v.TotalUses).Null().WithMessage(ErrorMessages.UnlimitedPassTypeNoUses());
        });

        RuleFor(v => v.EurPrice)
            .GreaterThan(0).WithMessage(ErrorMessages.PriceMustBePositive(nameof(CreateGymPassProductTemplateCommand.EurPrice)));
    }
}

public class CreateGymPassProductTemplateCommandHandler : IRequestHandler<CreateGymPassProductTemplateCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public CreateGymPassProductTemplateCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(CreateGymPassProductTemplateCommand command, CancellationToken cancellationToken)
    {
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

        var newTemplate = new GymPassProductTemplate
        {
            Id = Guid.NewGuid().ToString(),
            GymTier = command.GymTier,
            PassType = command.PassType,
            TotalUses = command.TotalUses,
            DaysAfterExpiring = command.DaysAfterExpiring,
            EurPrice = command.EurPrice
        };

        await _context.GymPassProductTemplates.AddAsync(newTemplate, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
