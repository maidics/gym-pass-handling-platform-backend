using FitPass.Application.Common.Models;
using FitPass.Application.Common.Security;
using FitPass.Domain.Constants;

namespace Fitpass.Application.GymPassProducts.Commands;

[Authorize(Roles = $"{Roles.AppAdministrator}")]
public record CreateGymPassProductTemplateCommand
(
    
) : IRequest<Result>;