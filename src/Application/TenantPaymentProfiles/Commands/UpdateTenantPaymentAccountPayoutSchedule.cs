
using FitPass.Application.Common.Extensions;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Interfaces.Payment;
using FitPass.Application.Common.Models;
using FitPass.Application.Common.Security;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;
using FitPass.Domain.Enums;

namespace FitPass.Application.TenantPaymentProfiles.Commands;

[Authorize(Roles = Roles.GymAdministrator)]
public record UpdateTenantPaymentAccountPayoutScheduleCommand(
    TimeIntervals Interval,
    int? MonhtlyAnchor = null,
    DayOfWeek? WeeklyAnchor = null,
    int? DelayDays = null //only applicable when using daily interval
) : IRequest<Result>;

public class UpdateTenantPaymentAccountPayoutScheduleCommandValidator : AbstractValidator<UpdateTenantPaymentAccountPayoutScheduleCommand>
{
    public UpdateTenantPaymentAccountPayoutScheduleCommandValidator()
    {
        RuleFor(v => v.Interval).NotEmptyWithMessage(nameof(UpdateTenantPaymentAccountPayoutScheduleCommand.Interval));

        When(v => v.Interval == TimeIntervals.Daily, () =>
        {
            RuleFor(v => v.DelayDays).NotNull().WithMessage("Delay days cannot be null when time interval is daily");
        });

        When(v => v.Interval == TimeIntervals.Weekly, () =>
        {
            RuleFor(v => v.WeeklyAnchor).NotNull().WithMessage("Weekly anchor cannot be null when time interval is weekly.");
        });

        When(v => v.Interval == TimeIntervals.Monthly, () =>
        {
            RuleFor(v => v.MonhtlyAnchor).NotNull().WithMessage("Monthly anchor cannot be null when time interval is monthly.");
        });
    }
}

public class UpdateTenantPaymentAccountPayoutScheduleCommandHandler : IRequestHandler<UpdateTenantPaymentAccountPayoutScheduleCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    private readonly IPaymentTenantService _paymentTenantService;

    public UpdateTenantPaymentAccountPayoutScheduleCommandHandler(
        IApplicationDbContext context,
        IUser user,
        IPaymentTenantService paymentTenantService)
    {
        _context = context;
        _user = user;
        _paymentTenantService = paymentTenantService;
    }
     
    public async Task<Result> Handle(UpdateTenantPaymentAccountPayoutScheduleCommand command, CancellationToken cancellationToken)
    {
        var gymEmployment = await _context.GymEmployments
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserId == _user.Id);

        Guard.Against.NullParameterRelatedToCurrentUser(gymEmployment, nameof(GymEmployment), _user.Id);

        var tenantPaymentProfile = await _context.TenantPaymentProfiles
            .FirstOrDefaultAsync(x => x.GymId == gymEmployment.GymId);

        if (tenantPaymentProfile is null)
        {
            return Result.BusinessRuleViolation("Gym has no payment account created.");
        }

        return await _paymentTenantService.UpdateTenantPaymentAccountPayoutIntervalAsync(
            tenantPaymentProfile.PaymentAccountId,
            command.Interval,
            command.MonhtlyAnchor,
            command.WeeklyAnchor,
            command.DelayDays);
    }
}