using FitPass.Infrastructure.Email;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace FitPass.Application.FunctionalTests;

public partial class Testing
{
    public FileInfo[] GetEmailFolderFileInfos()
    {
        using var scope = _scopeFactory.CreateScope();
        var environment = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();
        var settings = scope.ServiceProvider.GetRequiredService<IOptions<EmailSettings>>().Value;

        var emailFolder = Path.Combine(
            environment.ContentRootPath,
            "..",
            "..",
            settings.EmailPickupFolderName!,
            settings.EmailPickupSubFolderName!);
        
        var dirInfo = new DirectoryInfo(emailFolder);
        return dirInfo.GetFiles();
    }
}
