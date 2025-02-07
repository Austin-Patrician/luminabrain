
using FastWiki.Domain.Powers.Aggregates;
using FastWiki.Domain.Users.Aggregates;
using LuminadBrain.Entity.Powers.Aggregates;
using LuminadBrain.Entity.Users.Aggregates;

namespace LuminaBrain.EntityFrameworkCore.DBContext;

public interface IContext
{
    DbSet<TEntity> Set<TEntity>() where TEntity : class;
    

    DbSet<Role> Roles { get; set; }

    DbSet<User> Users { get; set; }

    DbSet<UserAuthExtensions> UserAuthExtensions { get; set; }

    DbSet<UserRole> UserRoles { get; set; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 运行迁移
    /// </summary>
    /// <param name="serviceProvider"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task RunMigrationsAsync(IServiceProvider serviceProvider,CancellationToken cancellationToken = default);
}