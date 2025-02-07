using LuminaBrain.EntityFrameworkCore.Repositories;
using LuminadBrain.Entity.Powers.Repositories;
using LuminadBrain.Entity.Users.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace LuminaBrain.EntityFrameworkCore;

public static class ServiceExtensions
{
    public static IServiceCollection AddFastWikiDbContext(this IServiceCollection services)
    {
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();

        return services;
    }
}