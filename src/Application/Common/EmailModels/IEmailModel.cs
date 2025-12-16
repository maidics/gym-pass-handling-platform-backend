namespace FitPass.Application.Common.EmailModels;

//marker interface
//inheritor has to be class because of Language property mutability
public interface IEmailModel
{
    string? Language { get; set; } //nullable - if null EmailService uses ILocalizer.DefaultCulture
}
