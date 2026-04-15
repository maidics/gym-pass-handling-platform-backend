
namespace FitPass.Application.Common.EmailModels;

//marker interface
public interface IEmailModel 
{
    string Language { get; init; }
    string Subject { get; init; }
    string Greeting { get; init; }
    string Body { get; init; }
    string Farewell { get; init; }
}
