using System.Text.Json;
using FitPass.Application.FunctionalTests.TestData.Common;
using FitPass.Domain.Entities;
using FitPass.Domain.Enums;
using FitPass.Infrastructure.Data;
using Microsoft.Extensions.DependencyInjection;

namespace FitPass.Application.FunctionalTests.TestData.EntityBuilders;

public class RequestBuilder : TestAuditableEntityBuilder<RequestBuilder, Request>
{
    private string _id = Guid.NewGuid().ToString();
    private string _title = string.Empty;
    private string _description = string.Empty;
    private PriorityLevel _priorityLevel = PriorityLevel.High;
    private RequestType _requestType = RequestType.Other;
    private RequestStatus _requestStatus = RequestStatus.Submitted;
    private string? _payload;

    public RequestBuilder(IServiceScopeFactory scopeFactory) : base(scopeFactory) { }

    public RequestBuilder WithId(string id)
    {
        AssertId(id);

        _id = id;

        return this;
    }

    public RequestBuilder WithTitle(string title)
    {
        _title = title;

        return this;
    }

    public RequestBuilder WithDescription(string description)
    {
        _description = description;

        return this;
    }

    public RequestBuilder WithPriorityLevel(PriorityLevel priorityLevel)
    {
        _priorityLevel = priorityLevel;

        return this;
    }

    public RequestBuilder WithRequestType(RequestType requestType)
    {
        _requestType = requestType;

        return this;
    }

    public RequestBuilder WithRequestStatus(RequestStatus requestStatus)
    {
        _requestStatus = requestStatus;

        return this;
    }

    public RequestBuilder WithPayload<TPayload>(TPayload payload) where TPayload : class
    {
        _payload = JsonSerializer.Serialize(payload);

        return this;
    }

    public RequestBuilder WithPayload(string payload)
    {
        _payload = payload;

        return this;
    }

    public override Request Build()
    {
        var request = new Request
        {
            Id = _id,
            Title = _title,
            Description = _description,
            PriorityLevel = _priorityLevel,
            Type = _requestType,
            Status = _requestStatus,
            Payload = _payload
        };

        ApplyAuditProperties(request);

        return request;
    }

    public override async Task<Request> BuildAsync()
    {
        var request = Build();

        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        await context.Requests.AddAsync(request);
        await context.SaveChangesAsync();

        var createdRequest = await context.Requests.FindAsync(request.Id);

        Guard.Against.Null(createdRequest);

        return createdRequest;
    }

    protected override void AssertEntity()
    {
        throw new NotImplementedException();
    }
}
