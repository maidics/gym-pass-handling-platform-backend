namespace FitPass.Application.Common.Interfaces;

public interface ILocalizer
{
    string DefaultCulture { get; }
    string[] SupportedCultures { get; }
    bool IsSupported(string culture);
    string GetForCulture(string culture, string key);
    string GetForCulture(string culture, string key, params object [] args);
    string Get(string key);
    string Get(string key, params object[] args);
    string GetNotFound(string key);
    string GetExternalServiceNotAvailable(string serviceName);
    string GetWithParamsLocalized(string key, params string[] args);
    string GetPropertyOfEntityIsRequired(string propertyKey, string entityKey);
    string GetNewValueIsRequired(string updatedValueKey);
}
