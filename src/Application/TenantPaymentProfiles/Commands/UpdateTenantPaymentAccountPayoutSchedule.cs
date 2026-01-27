//
// using FitPass.Application.Common.Extensions;
// using FitPass.Application.Common.Interfaces;
// using FitPass.Application.Common.Interfaces.Payment;
// using FitPass.Application.Common.Models;
// using FitPass.Application.Common.Security;
// using FitPass.Domain.Constants;
// using FitPass.Domain.Entities;
// using FitPass.Domain.Enums;
// using FitPass.Application.Common.Resources;
//
// namespace FitPass.Application.TenantPaymentProfiles.Commands;
//
// [Authorize(Roles = Roles.GymAdministrator)]
// public record UpdateTenantPaymentAccountPayoutScheduleCommand(
//     TimeIntervals TimeInterval,
//     int? MonthlyAnchor = null,
//     DayOfWeek? WeeklyAnchor = null,
//     int? DelayDays = null //only applicable when using daily interval
// ) : IRequest<Result>;
//
// public class UpdateTenantPaymentAccountPayoutScheduleCommandValidator : AbstractValidator<UpdateTenantPaymentAccountPayoutScheduleCommand>
// {
//     public UpdateTenantPaymentAccountPayoutScheduleCommandValidator(ILocalizer localizer)
//     {
//         //RuleFor(v => v.TimeInterval)
//             //.NotEmptyWithMessageLocalized(localizer, nameof(SharedResource.TimeInterval));
//
//         When(v => v.TimeInterval == TimeIntervals.Daily, () =>
//         {
//             RuleFor(v => v.DelayDays)
//                 .NotNull()
//                 .WithMessage(localizer.Get(nameof(SharedResource.DelayDaysMustBeNullIfTimeIntervalisDaily)));
//         });
//
//         When(v => v.TimeInterval == TimeIntervals.Weekly, () =>
//         {
//             RuleFor(v => v.WeeklyAnchor)
//                 .NotNull()
//                 .WithMessage(localizer.Get(nameof(SharedResource.WeeklyAnchorCannotBeNullIfTimeIntervalIsWeekly)));
//         });
//
//         When(v => v.TimeInterval == TimeIntervals.Monthly, () =>
//         {
//             RuleFor(v => v.MonthlyAnchor)
//                 .NotNull()
//                 .WithMessage(localizer.Get(nameof(SharedResource.MonthlyAnchorCannotBeEmptyIfTheTimeIntervalIsMonthly)));
//         });
//     }
// }
//
// public class UpdateTenantPaymentAccountPayoutScheduleCommandHandler : IRequestHandler<UpdateTenantPaymentAccountPayoutScheduleCommand, Result>
// {
//     private readonly IApplicationDbContext _context;
//     private readonly IUser _user;
//     private readonly IPaymentTenantService _paymentTenantService;
//     private readonly ILocalizer _localizer;
//
//     public UpdateTenantPaymentAccountPayoutScheduleCommandHandler(
//         IApplicationDbContext context,
//         IUser user,
//         IPaymentTenantService paymentTenantService,
//         ILocalizer localizer)
//     {
//         _context = context;
//         _user = user;
//         _paymentTenantService = paymentTenantService;
//         _localizer = localizer;
//     }
//      
//     public async Task<Result> Handle(UpdateTenantPaymentAccountPayoutScheduleCommand command, CancellationToken cancellationToken)
//     {
//         var gymEmployment = await _context.GymEmployments
//             .AsNoTracking()
//             .FirstOrDefaultAsync(x => x.UserId == _user.Id);
//
//         Guard.Against.NullParameterRelatedToCurrentUser(gymEmployment, nameof(GymEmployment), _user.Id);
//
//         var tenantPaymentProfile = await _context.TenantPaymentProfiles
//             .FirstOrDefaultAsync(x => x.GymId == gymEmployment.GymId);
//
//         if (tenantPaymentProfile is null)
//         {
//             return Result.BusinessRuleViolation(_localizer.Get(nameof(SharedResource.RequiresStripeAccount)));
//         }
//
//         return await _paymentTenantService.UpdateTenantPaymentAccountPayoutIntervalAsync(
//             tenantPaymentProfile.PaymentAccountId,
//             command.TimeInterval,
//             command.MonthlyAnchor,
//             command.WeeklyAnchor,
//             command.DelayDays);
//     }
// }
