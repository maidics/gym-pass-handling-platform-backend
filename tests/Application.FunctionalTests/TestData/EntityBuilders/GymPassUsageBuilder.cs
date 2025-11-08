using FitPass.Application.FunctionalTests.TestData.Common;
using FitPass.Domain.Entities;
using FitPass.Domain.Enums;
using FitPass.Infrastructure.Data;
using Microsoft.Extensions.DependencyInjection;

namespace FitPass.Application.FunctionalTests.TestData.EntityBuilders;

public class GymPassUsageBuilder : TestAuditableEntityBuilder<GymPassUsageBuilder, GymPassUsage>
{
    private string _id = Guid.NewGuid().ToString();
    private string _applicationUserId = string.Empty;
    private string _gymId = string.Empty;
    private int? _remainingPassUses = 0;
    private DateOnly? _passExpirationDate;
    private PassUseResult _passUseResult = PassUseResult.Success;
    private string? _lockerNumber;
    private DateTimeOffset? _gymSessionFinishedAt;
    private string _passId = string.Empty;
    private GymMembershipPass? _pass;

    public GymPassUsageBuilder(IServiceScopeFactory scopeFactory) : base(scopeFactory) { }

    public GymPassUsageBuilder WithId(string id)
    {
        AssertId(id);

        _id = id;

        return this;
    }

    public GymPassUsageBuilder WithApplicationUserId(string applicationUserId)
    {
        AssertId(_applicationUserId);

        _applicationUserId = applicationUserId;

        return this;
    }

    public GymPassUsageBuilder WithGymId(string gymId)
    {
        AssertId(_gymId);

        _gymId = gymId;

        return this;
    }

    public GymPassUsageBuilder ForPass(GymMembershipPass pass)
    {
        _passId = pass.Id;
        _pass = pass;

        _remainingPassUses = pass.RemainingUses;
        _passExpirationDate = pass.ExpirationDate;

        return this;
    }

    public GymPassUsageBuilder WithPassUseResult(PassUseResult passUseResult)
    {
        _passUseResult = passUseResult;

        return this;
    }

    public GymPassUsageBuilder WithLockerNumber(string lockerNumber)
    {
        _lockerNumber = lockerNumber;

        return this;
    }

    public GymPassUsageBuilder WithGymSessionFinishedAt(DateTimeOffset gymSessionFinishedAt)
    {
        _gymSessionFinishedAt = gymSessionFinishedAt;

        return this;
    }

    public override GymPassUsage Build()
    {
        return new GymPassUsage
        {
            Id = _id,
            ApplicationUserId = _applicationUserId,
            GymId = _gymId,
            RemainingPassUses = _remainingPassUses,
            PassExpirationDate = _passExpirationDate,
            PassUseResult = _passUseResult,
            LockerNumber = _lockerNumber,
            PassId = _passId,
            GymSessionFinishedAt = _gymSessionFinishedAt,
        };
    }

    public override async Task<GymPassUsage> BuildAsync()
    {
        var gymPassUsage = Build();

        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        Guard.Against.NullOrEmpty(_passId);
        Guard.Against.NullOrEmpty(_gymId);

        await context.GymPassUsages.AddAsync(gymPassUsage);
        await context.SaveChangesAsync();

        var createdPassUsage = await context.GymPassUsages.FindAsync(gymPassUsage.Id);

        Guard.Against.Null(createdPassUsage);

        return createdPassUsage;
    }

    protected override void AssertEntity()
    {
        throw new NotImplementedException();
    }
}
