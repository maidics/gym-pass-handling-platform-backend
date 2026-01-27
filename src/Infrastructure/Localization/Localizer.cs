using System.Globalization;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Resources;
using FitPass.Application.Common.Scopes;
using FitPass.Application.Common.Settings;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;

namespace FitPass.Infrastructure.Localization;

public class Localizer : ILocalizer
{
    private readonly IStringLocalizer<SharedResource> _localizer;
    private readonly CultureSettings _cultureSettings;

    public Localizer(
        IStringLocalizer<SharedResource> localizer,
        IOptions<CultureSettings> options)
    {
        _localizer = localizer;
        _cultureSettings = options.Value;
    }
    
    public string DefaultCulture => _cultureSettings.DefaultCulture;

    public string[] SupportedCultures => _cultureSettings.SupportedCultures;

    public bool IsSupported(string culture)
    {
        if (string.IsNullOrEmpty(culture)) return false;
        
        return _cultureSettings.SupportedCultures.Contains(culture, StringComparer.OrdinalIgnoreCase);
    }

    public string GetForCulture(string culture, string key)
    {
        using var scope = new CultureInfoScope(culture);
        
        return Get(key);
    }

    public string GetForCulture(string culture, string key, params object[] args)
    {
        using var scope = new CultureInfoScope(culture);
        
        return Get(key, args);
    }

    public string Get(string key) => CapitalizeFirstLetter(_localizer[key]);
    
    public string Get(string key, params object[] args) => CapitalizeFirstLetter(_localizer[key, args]);

    public string GetNotFound(string key) =>
        CapitalizeFirstLetter(_localizer[nameof(SharedResource.NotFound), _localizer[key]]);

    public string GetWithParamsLocalized(string key, params string[] args)
    {
        var localizedArgs = args.Select(arg =>
        {
            if (!string.IsNullOrEmpty(arg))
            {
                return _localizer[arg];
            }

            return arg;
        }).ToArray();
        
        return CapitalizeFirstLetter(_localizer[key, localizedArgs]);
    }

    public string GetPropertyOfEntityIsRequired(string propertyKey, string entityKey)
    {
        return CapitalizeFirstLetter(_localizer[
            _localizer[
                nameof(SharedResource.PropertyIsRequired), 
                _localizer[nameof(SharedResource.PropertyOf), 
                    _localizer[propertyKey], 
                    _localizer[entityKey]]]]);
    }

    public string GetNewValueIsRequired(string updatedValueKey)
    {
        return CapitalizeFirstLetter(
            _localizer[nameof(SharedResource.PropertyIsRequired),
                _localizer[nameof(SharedResource.NewValue), _localizer[updatedValueKey]]]);
    }

    private string CapitalizeFirstLetter(string input)
    {
        if (string.IsNullOrEmpty(input)) return input;

        return char.ToUpper(input[0], CultureInfo.CurrentUICulture) + input.Substring(1);
    }

    public string GetExternalServiceNotAvailable(string serviceName)
    {
        return Get(nameof(SharedResource.ExternalServiceUnavailable), serviceName);
    }
}
