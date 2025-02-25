﻿using LuminaBrain.Application.Service;
using LuminaBrain.EntityFrameCore.PostgreSql;
using LuminaBrain.EntityFramework.SqlServer;
using LuminaBrain.EntityFrameworkCore.DBContext;
using LuminaBrain.HttpApi.Extensions;
using LuminaBrain.HttpApi.Middleware;

namespace LuminaBrain.HostApi.Host.Extensions;

public static class ServiceExtensions
{
    private const string CorsPolicy = "CorsPolicy";

    public static IServiceCollection AddLuminaBrain(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<HandlingExceptionMiddleware>();

        services.AddHttpApi();

        services.AddLuminaBrainService(configuration);

        services.AddResponseCompression();

        var dbType = configuration["DbType"];
        if (dbType.Equals("PostgreSql", StringComparison.OrdinalIgnoreCase))
        {
            services.AddPostgreSqlDatabase(configuration);
        }
        else if (dbType.Equals("SqlServer", StringComparison.OrdinalIgnoreCase))
        {
            services.AddSqlServerDatabase(configuration);
        }
        
        return services;
    }

    public static async Task<IApplicationBuilder> UseLuminaBrain(this IApplicationBuilder builder,
        IConfiguration configuration)
    {
        builder.UseHttpApi();


        builder.UseAuthentication();
        builder.UseAuthorization();

        builder.UseStaticFiles();

        builder.UseResponseCompression();

        // 启动时自动迁移数据库
        var startMigrate = configuration["StartRunMigrations"];
        if (startMigrate?.Equals("true", StringComparison.OrdinalIgnoreCase) == true)
        {
            var scopeFactory = builder.ApplicationServices.GetRequiredService<IServiceScopeFactory>();

            using var scope = scopeFactory.CreateScope();

            var context = scope.ServiceProvider.GetRequiredService<IContext>();

            await context.RunMigrationsAsync(scope.ServiceProvider);
        }

        return builder;
    }
}