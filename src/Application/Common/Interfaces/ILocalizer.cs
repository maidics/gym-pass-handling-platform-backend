namespace FitPass.Application.Common.Interfaces;

public interface ILocalizer
{
    string Get(string key);
    string Get(string key, params object[] args);
    string GetNotFound(string key);
    string GetWithParamsLocalized(string key, params object[] args);
    string GetPropertyOfEntityIsRequired(string propertyKey, string entityKey);
    string GetNewValueIsRequired(string updatedValueKey);
}
