using FitPass.Application.Common.EmailModels.Users;
using FitPass.Application.Common.Extensions;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Models;
using FitPass.Application.Common.Resources;
using FitPass.Application.Common.Settings;
using FitPass.Domain.Strings;
using Microsoft.Extensions.Options;

namespace FitPass.Application.Users.Commands.Emails;

public record SendPasswordResetEmailCommand(string Email) : IRequest<Result>; 

public class RequestPasswordResetEmailCommandValidator : AbstractValidator<SendPasswordResetEmailCommand>
{
    public RequestPasswordResetEmailCommandValidator(ILocalizer localizer)
    {
        RuleFor(v => v.Email)
            .EmailAddressWithMessageLocalized(localizer);
    }
}

public class SendPasswordResetEmailCommandHandler : IRequestHandler<SendPasswordResetEmailCommand, Result>
{
    private readonly IIdentityService _identityService;
    private readonly IApplicationDbContext _context;
    private readonly ClientAppSettings _clientAppSettings;
    private readonly ILocalizer _localizer;
    private readonly IEmailService _emailService;

    public SendPasswordResetEmailCommandHandler(
        IIdentityService identityService, 
        IEmailService emailService,
        IOptions<ClientAppSettings> options,
        IApplicationDbContext context,
        ILocalizer localizer)
    {
        _identityService = identityService;
        _emailService = emailService;
        _clientAppSettings = options.Value;
        _context = context;
        _localizer = localizer;
    }

    public async Task<Result> Handle(SendPasswordResetEmailCommand command, CancellationToken cancellationToken)
    {
        var userId = await _identityService.GetUserIdByEmailAsync(command.Email);

        if (userId == null)
        {
            return Result.Success(); //the user will only receive the email if an account exists
        }

        var passwordResetToken = await _identityService.GeneratePasswordResetTokenAsync(userId);

        Guard.Against.Null(passwordResetToken, "Failed to generate password reset token.");
        
        var obj = await _context.UserProfiles
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .Select(x => new { x.PreferredLanguage, x.FirstName })
            .FirstOrDefaultAsync(cancellationToken);
        
        var email = new PasswordResetEmailModel()
        {
            Language = obj is null ? _localizer.DefaultCulture : obj.PreferredLanguage,
            Subject = _localizer.Get(nameof(SharedResource.PasswordResetEmailSubject)),
            Greeting = _localizer.Get(nameof(SharedResource.EmailGreeting), obj?.FirstName ?? _localizer.Get(nameof(SharedResource.User))),
            Body = _localizer.Get(nameof(SharedResource.PasswordResetEmailBody), _clientAppSettings.GetPasswordResetUrl(passwordResetToken, userId)),
            Farewell = _localizer.Get(nameof(SharedResource.EmailFarewell), CommonStrings.AppName)
        };
        
        await _emailService.SendEmailAsync(email, command.Email, cancellationToken);

        return Result.Success();
    }
}
