using FitPass.Application.Common.Models;
using FitPass.Domain.Entities;

namespace FitPass.Application.Common.Interfaces;

public interface IStripePriceService
{
    Task CreatePrice(GymPassProduct gymPassProduct);
    Task ArchivePrice(GymPassProduct gymPassProduct);
}
