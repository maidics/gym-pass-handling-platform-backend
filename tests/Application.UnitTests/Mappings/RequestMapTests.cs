using System;
using FitPass.Domain.Entities;
using FitPass.Domain.Enums;
using NUnit.Framework;
using FitPass.Application.Requests.DTOs;
using Shouldly;

namespace FitPass.Application.UnitTests.Mappings;

public class RequestMapTests
{
    [Test]
    public void ShouldMapToDto()
    {
        var createdOn = DateTimeOffset.UtcNow.AddHours(-1);
        var now = DateTimeOffset.UtcNow;

        var request = new Request
        {
            Id = "id",
            CreatedOn = createdOn,
            CreatedBy = null,
            LastModifiedOn = now,
            LastModifiedBy = null,
            Title = "title",
            Description = "description",
            PriorityLevel = PriorityLevel.Medium,
            Type = RequestType.Other,
            Status = RequestStatus.Submitted,
            Payload = null
        };

        var dto = request.MapToDto();

        dto.ShouldSatisfyAllConditions(
            () => dto.Id.ShouldBe("id"),
            () => dto.CreatedOn.ShouldBe(createdOn),
            () => dto.CreatedBy.ShouldBeNull(),
            () => dto.LastModifiedOn.ShouldBe(now),
            () => dto.LastModifiedBy.ShouldBeNull(),
            () => dto.Title.ShouldBe("title"),
            () => dto.Description.ShouldBe("description"),
            () => dto.PriorityLevel.ShouldBe(PriorityLevel.Medium),
            () => dto.Type.ShouldBe(RequestType.Other),
            () => dto.Status.ShouldBe(RequestStatus.Submitted),
            () => dto.Payload.ShouldBeNull());
    }
}
