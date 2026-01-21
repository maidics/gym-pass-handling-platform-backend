using FitPass.Application.Common.Extensions;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Models;
using FitPass.Application.Common.Resources;
using FitPass.Application.Common.Security;
using FitPass.Application.Requests.DTOs;
using FitPass.Domain.Entities;
using FitPass.Domain.Enums;

namespace FitPass.Application.Requests.Commands;

[Authorize]
public record CreatePayloadFreeRequestCommand(
    string Title, string Description, PriorityLevel PriorityLevel, RequestType RequestType) : IRequest<Result<RequestDto>>;

public class CreatePayloadFreeRequestCommandValidator : AbstractValidator<CreatePayloadFreeRequestCommand>
{
    public CreatePayloadFreeRequestCommandValidator(ILocalizer localizer)
    {
        RuleFor(v => v.Title).NotEmptyWithMessageLocalized(localizer, nameof(SharedResource.Title));

        RuleFor(v => v.Description).NotEmptyWithMessageLocalized(localizer, nameof(SharedResource.Description));

        RuleFor(v => v.PriorityLevel).NotEmptyWithMessageLocalized(localizer, nameof(SharedResource.Priority));

        RuleFor(v => v.RequestType).NotEmpty()
            .WithMessage(localizer.GetPropertyOfEntityIsRequired(nameof(SharedResource.Type),
                nameof(SharedResource.Request)));

        RuleFor(v => v.RequestType)
            .Must(v => v != RequestType.GymAdminPromotion && v != RequestType.GymCreation)
            .WithMessage(localizer.Get(nameof(SharedResource.PayloadFreeRequestTypeRules)));
    }
}

public class CreatePayloadFreeRequestCommandHandler : IRequestHandler<CreatePayloadFreeRequestCommand, Result<RequestDto>>
{
    private readonly IApplicationDbContext _context;

    public CreatePayloadFreeRequestCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task<Result<RequestDto>> Handle(CreatePayloadFreeRequestCommand command, CancellationToken cancellationToken)
    {
        var request = new Request
        {
            Title = command.Title,
            Description = command.Description,
            PriorityLevel = command.PriorityLevel,
            Type = command.RequestType,
            Payload = null
        };

        await _context.Requests.AddAsync(request);
        await _context.SaveChangesAsync();

        return Result.Success(request.MapToDto());
    }
}
