using FitPass.Application.Common.Interfaces;
using FitPass.Application.FunctionalTests.TestData.Common;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FitPass.Application.FunctionalTests.TestData.EntityBuilders;

public class GymEmploymentBuilder : TestEntityBuilderBase<GymEmployment>
{
    private string? _applicationUserId;
    private string? _gymId;
    private string? _escalationEmail;
    private string _role = Roles.PendingGymEmployee;
    private DateTimeOffset _employmentStart;
    private DateTimeOffset? _employmentEnd;

    public GymEmploymentBuilder(IServiceScopeFactory scopeFactory) : base(scopeFactory) { }

    public GymEmploymentBuilder WithApplicationUserId(string applicationUserId)
    {
        AssertId(applicationUserId);

        _applicationUserId = applicationUserId;

        return this;
    }

    public GymEmploymentBuilder WithGymId(string gymId)
    {
        AssertId(gymId);

        _gymId = gymId;

        return this;
    }

    public GymEmploymentBuilder WithEscalationEmail(string escalationEmail)
    {
        AssertEmail(escalationEmail);

        _escalationEmail = escalationEmail;

        return this;
    }
    
    public GymEmploymentBuilder WithRole(string role)
    {
        AssertRole(role);

        _role = role;

        return this;
    }

    public GymEmploymentBuilder WithEmploymentStart(DateTimeOffset employmentStart)
    {
        _employmentStart = employmentStart;

        return this;
    }

    public GymEmploymentBuilder WithEmploymentEnd(DateTimeOffset? employmentEnd)
    {
        if (employmentEnd != null && _employmentStart > _employmentEnd)
        {
            throw new InvalidOperationException("Employment end must be later than employment start.");
        }

        _employmentEnd= employmentEnd;

        return this;
    }

    public override GymEmployment Build()
    {
        var gymEmployment = new GymEmployment
        {
            UserId = _applicationUserId,
            GymId = _gymId,
            EscalationEmail = _escalationEmail,
            Role = _role,
            EmploymentStart = _employmentStart,
            EmploymentEnd = _employmentEnd
        };

        return gymEmployment;
    }

    public override async Task<GymEmployment> BuildAsync()
    {
        var gymEmployment = Build();

        using var scope = _scopeFactory.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();

        await context.GymEmployments.AddAsync(gymEmployment);
        await context.SaveChangesAsync();

        var createdGymEmployment = await context
            .GymEmployments
            .Include(ge => ge.Gym)
            .FirstOrDefaultAsync(ge => ge.Id == gymEmployment.Id);

        Guard.Against.Null(createdGymEmployment);

        return createdGymEmployment;
    }

    protected override void AssertEntity()
    {
        throw new NotImplementedException();
    }
}
