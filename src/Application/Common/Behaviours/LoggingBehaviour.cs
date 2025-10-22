using FitPass.Application.Common.Interfaces;
using FitPass.Domain.Entities;
using MediatR.Pipeline;
using Microsoft.Extensions.Logging;

namespace FitPass.Application.Common.Behaviours;

public class LoggingBehaviour<TRequest> : IRequestPreProcessor<TRequest>
    where TRequest : notnull
{
    private readonly ILogger _logger;
    private readonly IUser _user;
    private readonly IIdentityService _identityService;

    public LoggingBehaviour(ILogger<TRequest> logger, IUser user, IIdentityService identityService)
    {
        _logger = logger;
        _user = user;
        _identityService = identityService;
    }

    public async Task Process(TRequest request, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        ApplicationUser? user = null;

        if (!string.IsNullOrEmpty(_user.Id))
        {
            user = await _identityService.FindUserByIdAsync(_user.Id);
        }

        _logger.LogInformation("FitPass Request: {Name} {@UserId} {@UserName} {@Request}",
            requestName, user?.Id, user?.Email, request);
    }
}
