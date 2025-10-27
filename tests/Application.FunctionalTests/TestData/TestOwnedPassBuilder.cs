using FitPass.Application.FunctionalTests.TestData.Common;
using FitPass.Domain.Entities;
using FitPass.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;

namespace FitPass.Application.FunctionalTests.TestData;

using static Testing;

public class TestOwnedPassBuilder : TestEntityBuilderBase<OwnedPass>
{
    private UserGymMembership? _userGymMembership = null;
    private PassType? _passType = null;
    private int? _totalUses = null;
    private int? _remainingUses = null;
    private DateOnly? _expirationDate = null;
    private decimal _hufPrice = -1;
    private ApplicationUser? _applicationUser = null;
    private NonRegisteredUser? _nonRegisteredUser = null;
    private Gym? _gym = null;

    private readonly TestUserGymMembershipBuilder _testUserGymMembershipBuilder;

    public TestOwnedPassBuilder(IServiceScopeFactory scopeFactory) : base(scopeFactory)
    {
        _testUserGymMembershipBuilder = new(scopeFactory);
    }

    public TestOwnedPassBuilder ForUserGymMembership(UserGymMembership userGymMembership)
    {
        _userGymMembership = userGymMembership;

        return this;
    }

    public TestOwnedPassBuilder WithUserGymMembership(bool forNonRegisteredUser = false)
    {
        if (forNonRegisteredUser)
        {
            _userGymMembership = await _testUserGymMembershipBuilder
                .WithNavigationProperties(false, true, true)
                .build
        }
    }

    public TestOwnedPassBuilder UseBased(int totalUses, int remainingUses)
    {
        if (totalUses < 1)
        {
            throw new InvalidOperationException("Use based passes must have 1 or more total uses.");
        }

        if (totalUses == 1)
        {
            _passType = PassType.SingleUse;
            _totalUses = 1;

            if (remainingUses != 0 && remainingUses != 1)
            {
                throw new InvalidOperationException("Single use pass can only have 0 or 1 remaining use.");
            }

            _remainingUses = remainingUses;
            _expirationDate = null;

            return this;
        }
        else
        {
            _passType = PassType.MultiUse;
            _totalUses = totalUses;

            if (remainingUses < 0)
            {
                throw new InvalidOperationException("Remaining uses for multi use pass must be 0 or bigger.");
            }

            _remainingUses = remainingUses;
            _expirationDate = null;

            return this;
        }
    }

    public TestOwnedPassBuilder WithPrice(decimal hufPrice)
    {
        _hufPrice = hufPrice;

        return this;
    }
    

}
