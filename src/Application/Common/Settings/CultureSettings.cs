namespace FitPass.Application.Common.Settings;

public class CultureSettings
{
    public required string DefaultCulture { get; init; }
    public required string[] SupportedCultures { get; init; }
}
