using FitPass.Application.Common.Interfaces;
using FitPass.Infrastructure.Localization.Resources;
using Microsoft.Extensions.Localization;

namespace FitPass.Infrastructure.Localization;

public class Localizer : ILocalizer
{
    private readonly IStringLocalizer<SharedResource> _localizer;

    public Localizer(IStringLocalizer<SharedResource> localizer)
    {
        _localizer = localizer;
    }

    public string Get(string key) => _localizer[key];

    public string Get(string key, params object[] args) => _localizer[key, args];
}
