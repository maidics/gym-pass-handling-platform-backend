using FitPass.Application.Common.Exceptions;
using FitPass.Application.Requests.Commands;
using FitPass.Application.Requests.DTOs;
using FitPass.Domain.Constants;
using FitPass.Domain.Enums;
using FitPass.Domain.Strings;

namespace FitPass.Application.FunctionalTests.Tests.FluentValidationTests;

using static Testing;

public class FluentValidationTests : BaseTestFixture
{
    [Test]
    public async Task ShoudDenyInvalidInput()
    {
        await RunAsPendingGymEmployeeAsync();

        var gymName = new string('a', 400);

        var command = new CreateGymCreationRequestCommand(
            string.Empty,
            PriorityLevel.Low,
            new CreateGymDto
            {
                GymName = gymName,
                GymAddress = "Address",
                GymStatus = GymStatus.Suspended,
                GymTier = GymTier.Premium,
                EscalationEmail = string.Empty
            });

        var action = () => SendAsync(command);

        var ex = action.ShouldThrow<ValidationException>();

        ex.Errors.ShouldContainKey(nameof(CreateGymCreationRequestCommand.RequestDescription));
        ex.Errors[nameof(CreateGymCreationRequestCommand.RequestDescription)].ShouldContain(ErrorMessages.PropertyIsRequired(nameof(CreateGymCreationRequestCommand.RequestDescription)));

        ex.Errors.ShouldContainKey("CreateGymDTO.GymName");
        ex.Errors["CreateGymDTO.GymName"].ShouldContain(ErrorMessages.PropertyCannotBeLongerThan(nameof(CreateGymCreationRequestCommand.CreateGymDTO.GymName), MaxStringLengths.Name));

        ex.Errors.ShouldContainKey("CreateGymDTO.EscalationEmail");
        ex.Errors["CreateGymDTO.EscalationEmail"].ShouldContain(ErrorMessages.PropertyIsRequired(nameof(CreateGymCreationRequestCommand.CreateGymDTO.EscalationEmail)));
        ex.Errors["CreateGymDTO.EscalationEmail"].ShouldContain(ErrorMessages.InvalidEmailAddress(nameof(CreateGymCreationRequestCommand.CreateGymDTO.EscalationEmail)));        
    }

    public override void AuthorizeAttributeCheck()
    {
        throw new InvalidOperationException();
    }
}
