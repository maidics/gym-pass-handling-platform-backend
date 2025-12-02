using FitPass.Application.ApplicationUsers.DTOs;
using FitPass.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace FitPass.Application.Users.Commands;

/*
public record GenericRegisterUserCommand<TRole>(
    string FirstName,
    string LastName,
    string Email,
    string Password,
    string PasswordConfirm
) : IRequest<JwtToken>;

public class GenericRegisterUserCommandValidator : AbstractValidator<GenericRegisterUserCommand< >>
{
    public GenericRegisterUserCommandValidator()
    {
        throw new NotImplementedException();
    }
}

public class GenericRegisterUserCommandHandler<TRole> : IRequestHandler<GenericRegisterUserCommand<TRole>, JwtToken>
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityService _identityService;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly ILogger<GenericRegisterUserCommandHandler<TRole>> _logger;

    public GenericRegisterUserCommandHandler(
        IApplicationDbContext context,
        IIdentityService identityService,
        IJwtTokenService jwtTokenService,
        ILogger<GenericRegisterUserCommandHandler<TRole>> logger)
    {
        _context = context;
        _identityService = identityService;
        _jwtTokenService = jwtTokenService;
        _logger = logger;
    }

    public async Task<JwtToken> Handle(GenericRegisterUserCommand<TRole> request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
*/