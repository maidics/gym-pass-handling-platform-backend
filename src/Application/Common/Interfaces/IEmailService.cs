using FitPass.Application.Common.EmailModels;

namespace FitPass.Application.Common.Interfaces;

public interface IEmailService
{
    Task SendEmailAsync(IEmailModel emailModel, string[] to, string[]? cc = null, string[]? bcc = null);
}
