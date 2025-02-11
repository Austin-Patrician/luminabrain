using LuminaBrain.EntityFrameworkCore;
using LuminaBrain.EntityFrameworkCore.DBContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LuminaBrain.EntityFramework.SqlServer;

public static class SqlServerEntityFrameworkCoreExtensions
{
    //暴露服务接口给host 使用来决定使用哪个 database provider.
    public static IServiceCollection AddSqlServerDatabase(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<SqlServerDbContext>((builder =>
        {
            builder.UseSqlServer(configuration.GetConnectionString("Default"));
        }));

        services.AddLuminaBrainContext();

        services.AddScoped<IContext, SqlServerDbContext>();

        return services;
    }
}