using FitPass.Application.Common.Security;
using FitPass.Application.Requests.DTOs;
using FitPass.Domain.Constants;

namespace Fitpass.Application.Requests.Queries;

[Authorize(Roles = Roles.AppAdministrator)]
public record GetAllGymCreationRequests : IRequest<List<GymCreationRequestDto>>;

