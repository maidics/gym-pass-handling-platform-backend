using FitPass.Domain.Common;
using Microsoft.Extensions.DependencyInjection;

namespace FitPass.Application.FunctionalTests.TestData.Common;

public abstract class TestAuditableEntityBuilder<TInheritor, TAuditableEntity> : TestEntityBuilderBase<TAuditableEntity> 
    where TInheritor : TestAuditableEntityBuilder<TInheritor, TAuditableEntity>
    where TAuditableEntity : BaseAuditableEntity
{
    protected DateTimeOffset _createdOn = DateTimeOffset.UtcNow;
    protected string? _createdBy = null;
    protected DateTimeOffset _lastModifiedOn = DateTimeOffset.UtcNow;
    protected string? _lastModifiedBy = null;

    protected TestAuditableEntityBuilder(IServiceScopeFactory scopeFactory) : base(scopeFactory) { }

    public TInheritor WithCreatedOn(DateTimeOffset createdOn)
    {
        _createdOn = createdOn;

        return (TInheritor)this;
    }

    public TInheritor WithCreatedBy(string? createdBy)
    {
        _createdBy = createdBy;

        return (TInheritor)this;
    }

    public TInheritor WithLastModifiedOn(DateTimeOffset lastModifiedOn)
    {
        _lastModifiedOn = lastModifiedOn;

        return (TInheritor)this;
    }

    public TInheritor WithLastModifiedBy(string? lastModifiedBy)
    {
        _lastModifiedBy = lastModifiedBy;

        return (TInheritor)this;
    }

    protected void ApplyAuditProperties(TAuditableEntity entity)
    {
        entity.CreatedBy = _createdBy;
        entity.LastModifiedBy = _lastModifiedBy;
        entity.CreatedOn = _createdOn;
        entity.LastModifiedOn = _lastModifiedOn;
    }
}
