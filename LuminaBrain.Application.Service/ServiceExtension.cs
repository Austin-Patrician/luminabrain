using LuminaBrain.Application.Authorization;
using LuminaBrain.Application.Notification;
using LuminaBrain.Application.Powers;
using LuminaBrain.Application.Service.Authorization;
using LuminaBrain.Application.Service.Notification;
using LuminaBrain.Application.Service.Powers;
using LuminaBrain.Application.Service.Users;
using LuminaBrain.Application.Users;
using LuminaBrain.Jwt;
using Mapster;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LuminaBrain.Application.Service;

public static class ServiceExtension
{
    /// <summary>
    /// 注入执行的service
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddLuminaBrainService(this IServiceCollection services,IConfiguration configuration)
    {
        services.AddJwt(configuration);
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IPowersService, PowersService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IAuthorizationService, AuthorizationService>();
        
        services.AddMapster();
        services.AddCaptcha();

        return services;
    }
}