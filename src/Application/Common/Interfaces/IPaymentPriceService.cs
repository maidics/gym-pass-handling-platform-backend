using FitPass.Domain.Entities;

namespace FitPass.Application.Common.Interfaces;

public interface IPaymentPriceService
{
    Task CreatePrice(GymPassProduct gymPassProduct);
    Task ArchivePrice(GymPassProduct gymPassProduct);
}
