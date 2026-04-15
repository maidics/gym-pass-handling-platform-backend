using System.Globalization;

namespace FitPass.Application.Common.Scopes;

//credit: Roland Tóth: https://blog.rolandtoth.hu/cultureinfo-scope/
public sealed class CultureInfoScope : IDisposable
{
    private readonly CultureInfo _originalCulture;
    private readonly CultureInfo _originalUICulture;

    public CultureInfoScope(string culture)
    {
        var cultureInfo = new CultureInfo(culture);
        
        _originalCulture = CultureInfo.CurrentCulture;
        _originalUICulture = CultureInfo.CurrentUICulture;
        
        CultureInfo.CurrentCulture = cultureInfo;
        CultureInfo.CurrentUICulture = cultureInfo;
    }

    public void Dispose()
    {
        CultureInfo.CurrentCulture = _originalCulture;
        CultureInfo.CurrentUICulture = _originalUICulture;
    }
}
