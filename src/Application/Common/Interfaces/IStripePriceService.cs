using FitPass.Domain.Entities;

namespace FitPass.Application.Common.Interfaces;

public interface IStripePriceService
{
    Task CreatePrice(GymPassProduct gymPassProduct, CancellationToken cancellationToken);
}
