using FitPass.Application.ApplicationUsers.DTOs;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Extensions;

namespace FitPass.Application.ApplicationUsers.Commands;

public record AddPasswordToUserWithNoPasswordCommand(string Password) : IRequest<TokenResponse>;

public class AddPasswordToUserWithNoPasswordCommandValidator : AbstractValidator<AddPasswordToUserWithNoPasswordCommand>
{
    public AddPasswordToUserWithNoPasswordCommandValidator()
    {
        RuleFor(v => v.Password).StrongPassword();
    }
}

public class AddPasswordToUserWithNoPasswordCommandHandler : IRequestHandler<AddPasswordToUserWithNoPasswordCommand, TokenResponse>
{
    private readonly IIdentityService _identityService;
    private readonly IJwtTokenService _jwtTokenService;

    public AddPasswordToUserWithNoPasswordCommandHandler(IIdentityService identityService, IJwtTokenService jwtTokenService)
    {
        _identityService = identityService;
        _jwtTokenService = jwtTokenService;
    }
    public async Task<TokenResponse> Handle(AddPasswordToUserWithNoPasswordCommand command, CancellationToken cancellationToken)
    {
        var result = await _identityService.AddPasswordToUserWithNoPassword()
    }
}