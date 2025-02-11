using LuminaBrain.EntityFrameworkCore;
using LuminaBrain.EntityFrameworkCore.DBContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LuminaBrain.EntityFrameCore.PostgreSql;

public static class PostgreSqlEntityFrameworkCoreExtensions
{
    public static IServiceCollection AddPostgreSqlDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<PostgreSqlDbContext>((builder =>
        {
            builder.UseNpgsql(configuration.GetConnectionString("Default"));
        }));

        services.AddLuminaBrainContext();
        
        services.AddScoped<IContext, PostgreSqlDbContext>();

        return services;
    }
}