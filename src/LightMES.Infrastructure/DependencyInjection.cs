using LightMES.Application.Common.Behaviors;
using LightMES.Application.Common.Interfaces;
using LightMES.Infrastructure.Security;
using LightMES.Infrastructure.Services;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace LightMES.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IPermissionService, PermissionService>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(AuthorizationBehavior<,>));

        return services;
    }
}
