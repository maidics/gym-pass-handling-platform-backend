namespace FitPass.Application.Common.Interfaces;

public interface ILocalizer
{
    string Get(string key);
    string Get(string key, params object[] args);
}
