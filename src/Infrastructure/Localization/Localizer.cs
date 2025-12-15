using System.Globalization;
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

    public string Get(string key) => CapitalizeFirstLetter(_localizer[key]);
    
    public string Get(string key, params object[] args) => CapitalizeFirstLetter(_localizer[key, args]);

    public string GetNotFound(string key) =>
        CapitalizeFirstLetter(_localizer[nameof(SharedResource.NotFound), _localizer[key]]);

    public string GetWithParamsLocalized(string key, params object[] args)
    {
        var localizedArgs = args.Select(arg =>
        {
            if (arg is string stringArg)
            {
                return _localizer[stringArg];
            }

            return arg;
        });
        
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
}
