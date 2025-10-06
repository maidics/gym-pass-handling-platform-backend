using FitPass.Application.Common.Models;
using FitPass.Domain.Entities;

namespace FitPass.Application.Common.Interfaces;

public interface IStripePriceService
{
    Task<Result> CreatePrice(GymPassProduct gymPassProduct);
    Task<Result> ArchivePrice(GymPassProduct gymPassProduct);
}
