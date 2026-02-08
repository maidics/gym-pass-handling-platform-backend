using FitPass.Infrastructure.Email;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace FitPass.Application.FunctionalTests;

public partial class Testing
{
    public static FileInfo[] GetEmailFolderFileInfos()
    {
        using var scope = _scopeFactory.CreateScope();
        var environment = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();
        var settings = scope.ServiceProvider.GetRequiredService<IOptions<EmailSettings>>().Value;

        var emailFolder = Path.Combine(
            environment.ContentRootPath,
            "..",
            "..",
            settings.EmailPickupFolderName!,
            settings.EmailPickupSubFolderName!
        );

        var dirInfo = new DirectoryInfo(emailFolder);
        return dirInfo.GetFiles();
    }

    public static FileInfo[] EmailFolderShouldContainEmails(int amount = 1)
    {
        var infos = GetEmailFolderFileInfos();

        infos.Length.ShouldBe(amount);

        return infos;
    }
}
