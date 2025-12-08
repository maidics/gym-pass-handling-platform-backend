using System.Reflection;
using FitPass.Application.Common.Behaviours;
using FitPass.Application.Common.Models;
using FitPass.Application.Requests.Commands;
using FitPass.Application.Requests.DTOs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace FitPass.Application;

public static class DependencyInjection
{
    public static void AddApplicationServices(this IHostApplicationBuilder builder)
    {
        builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        builder.Services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            cfg.AddOpenRequestPreProcessor(typeof(LoggingBehaviour<>));
            cfg.AddOpenBehavior(typeof(UnhandledExceptionBehaviour<,>));
            cfg.AddOpenBehavior(typeof(AuthorizationBehaviour<,>));
            cfg.AddOpenBehavior(typeof(ValidationBehaviour<,>));
            cfg.AddOpenBehavior(typeof(PerformanceBehaviour<,>));
        });

        builder.Services.AddTransient(
            typeof(IRequestHandler<DeserializeRequestPayloadCommand<CreateGymDto>, Result<CreateGymDto>>),
            typeof(DeserializeRequestPayloadCommandHandler<CreateGymDto>));

        builder.Services.AddTransient(
            typeof(IRequestHandler<DeserializeRequestPayloadCommand<GymAdminPromotionDto>, Result<GymAdminPromotionDto>>),
            typeof(DeserializeRequestPayloadCommandHandler<GymAdminPromotionDto>));
    }
}
