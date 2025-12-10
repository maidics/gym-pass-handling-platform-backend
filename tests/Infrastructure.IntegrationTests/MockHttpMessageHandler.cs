using System.Net;

namespace FitPass.Infrastructure.IntegrationTests;

public class MockHttpMessageHandler : HttpMessageHandler
{
    private readonly HttpStatusCode _statusCode;
    private readonly int _tryCount;
    private int _callCount;

    public MockHttpMessageHandler(HttpStatusCode statusCode, int tryCount)
    {
        _statusCode = statusCode;
        _tryCount = tryCount;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        _callCount++;

        if (_callCount <= _tryCount)
        {
            return Task.FromResult(new HttpResponseMessage(_statusCode));
        }

        return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("{\"id\": \"pi_success\"}")
        });
    }

    public int CallCount => _callCount;
}
