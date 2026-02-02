using FitPass.Application.Common.Interfaces;
using FitPass.Domain.Entities;

namespace FitPass.Application.GymMemberships.Commands;

//Application Layer use only
public record GetOrCreateGymMembershipCommand(string UserId, string GymId)
    : IRequest<GymMembership>;

public class GetOrCreateGymMembershipCommandHandler
    : IRequestHandler<GetOrCreateGymMembershipCommand, GymMembership>
{
    private readonly IApplicationDbContext _context;

    public GetOrCreateGymMembershipCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<GymMembership> Handle(
        GetOrCreateGymMembershipCommand command,
        CancellationToken cancellationToken
    )
    {
        var gymMembership = await _context
            .GymMemberships.AsNoTracking()
            .FirstOrDefaultAsync(
                ge => ge.GymId == command.GymId && ge.UserId == command.UserId,
                cancellationToken
            );

        if (gymMembership is null)
        {
            gymMembership = new GymMembership { UserId = command.UserId, GymId = command.GymId };

            await _context.GymMemberships.AddAsync(gymMembership, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        return gymMembership;
    }
}
