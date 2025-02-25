using LuminaBrain.Core;
using LuminaBrain.HttpApi.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace LuminaBrain.HttpApi.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddHttpApi(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddSingleton<IUserContext, UserContext>();


        return services;
    }

    public static IApplicationBuilder UseHttpApi(this IApplicationBuilder builder)
    {
        builder.UseMiddleware<HandlingExceptionMiddleware>();

        return builder;
    }

    public static IEndpointRouteBuilder MapApis(this IEndpointRouteBuilder endpoint)
    {
        endpoint.MapUserEndpoints();
        endpoint.MapAuthorizationEndpoints();
        endpoint.MapNotificationEndpoints();
        endpoint.MapPowersEndpoints();
        return endpoint;
    }
}