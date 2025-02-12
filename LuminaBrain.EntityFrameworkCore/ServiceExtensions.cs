using LuminaBrain.Domain.Powers.Repositories;
using LuminaBrain.Domain.Users.Repositories;
using LuminaBrain.EntityFrameworkCore.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace LuminaBrain.EntityFrameworkCore;

public static class ServiceExtensions
{
    public static IServiceCollection AddLuminaBrainContext(this IServiceCollection services)
    {
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();

        return services;
    }
}