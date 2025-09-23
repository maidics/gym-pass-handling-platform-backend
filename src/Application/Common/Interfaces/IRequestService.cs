namespace FitPass.Application.Common.Interfaces;

public interface IRequestService
{
    Task FulfillRequest(string requestId);
}