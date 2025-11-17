using System;
using FitPass.Domain.Entities.Payment;

namespace FitPass.Application.Common.Interfaces;

public interface IPaymentTenantService
{
    Task<string> CreateTenantAccount(GymPaymentProfile gymPaymentProfile);
}
