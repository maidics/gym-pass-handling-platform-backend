using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Security;
using FitPass.Application.Extensions;
using FitPass.Domain.Constants;

namespace FitPass.Application.ApplicationUsers.Commands;

[Authorize]
public record UpdateMyUserProfileCommand(
    string FirstName,
    string LastName
) : IRequest;

public class UpdateMyUserProfileCommandValidator : AbstractValidator<UpdateMyUserProfileCommand>
{
    public UpdateMyUserProfileCommandValidator()
    {
        RuleFor(v => v.FirstName).NotEmptyWithMaxLenghtAndMessage(MaxStringLengths.Name, "first name");

        RuleFor(v => v.LastName).NotEmptyWithMaxLenghtAndMessage(MaxStringLengths.Name, "last name");
    }
}

public class UpdateMyUserProfileCommandHandler : IRequestHandler<UpdateMyUserProfileCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    private readonly IMapper _mapper;

    public UpdateMyUserProfileCommandHandler(IApplicationDbContext context, IUser user, IMapper mapper)
    {
        _context = context;
        _user = user;
        _mapper = mapper;
    }
    
    public async Task Handle(UpdateMyUserProfileCommand command, CancellationToken cancellationToken)
    {
        var user = await _context.ApplicationUsers.FindAsync(_user.Id!);

        if (user == null)
        {
            throw new UnauthorizedAccessException();
        }

        user.FirstName = command.FirstName;
        user.LastName = command.LastName;

        await _context.SaveChangesAsync();
    }
}