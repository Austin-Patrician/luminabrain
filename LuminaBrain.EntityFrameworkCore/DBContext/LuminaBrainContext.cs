using FastWiki.Domain.Powers.Aggregates;
using FastWiki.Domain.Users.Aggregates;
using LuminaBrain.Core;
using LuminaBrain.Data.Auditing;
using LuminadBrain.Entity.Powers.Aggregates;
using LuminadBrain.Entity.Users.Aggregates;
using Microsoft.Extensions.DependencyInjection;

namespace LuminaBrain.EntityFrameworkCore.DBContext;

public class LuminaBrainContext<TContext>(DbContextOptions<TContext> options, IServiceProvider serviceProvider) : DbContext(options), IContext
    where TContext : DbContext
{
    public DbSet<Role> Roles { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<UserAuthExtensions> UserAuthExtensions { get; set; }
    public DbSet<UserRole> UserRoles { get; set; }
    protected IUserContext UserContext => serviceProvider.GetRequiredService<IUserContext>();

    
    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        BeforeSaveChanges();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    protected void BeforeSaveChanges()
    {
        var entries = ChangeTracker.Entries()
            .Where(x => (x.State == EntityState.Added || x.State == EntityState.Modified));

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                var creationTimeProperty = entry.Entity.GetType().GetProperty(nameof(ICreator.CreationTime));
                if (creationTimeProperty != null && creationTimeProperty.PropertyType == typeof(DateTimeOffset?))
                {
                    creationTimeProperty.SetValue(entry.Entity, DateTimeOffset.Now);
                }

                var creatorProperty = entry.Entity.GetType().GetProperty(nameof(ICreator.Creator));
                if (creatorProperty != null)
                {
                    creatorProperty.SetValue(entry.Entity, UserContext.UserId);
                }
            }
            else if (entry.State == EntityState.Modified)
            {
                var lastModificationTimeProperty = entry.Entity.GetType().GetProperty(nameof(IModifier.ModificationTime));
                if (lastModificationTimeProperty != null && lastModificationTimeProperty.PropertyType == typeof(DateTimeOffset?))
                {
                    lastModificationTimeProperty.SetValue(entry.Entity, DateTimeOffset.Now);
                }
                var modifierProperty = entry.Entity.GetType().GetProperty(nameof(IModifier.Modifier));
                if (modifierProperty != null)
                {
                    modifierProperty.SetValue(entry.Entity, UserContext.UserId);
                }
            }
        }
    }
    
    public async Task RunMigrationsAsync(IServiceProvider serviceProvider,
        CancellationToken cancellationToken = new CancellationToken())
    {
        await Database.MigrateAsync(cancellationToken);

        // 初始化数据
        await using var scope = serviceProvider.CreateAsyncScope();

        var dataInitializer = scope.ServiceProvider.GetRequiredService<IContext>();

        if (dataInitializer is LuminaBrainContext<TContext> context)
        {
            // 判断是否存在初始化数据
            if (await context.Users.AnyAsync(cancellationToken: cancellationToken))
            {
                return;
            }

            // 启动事务
            await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

            // 初始化数据
            var user = new User("admin", "AIDotNet", "Aa123456.", "239573049@qq.com", "13049809673",
                "AIDotNet FastWiki 管理员账号");

            await context.Users.AddAsync(user, cancellationToken);

            #region 初始化角色

            var role = new Role("管理员", "系统管理员", "admin");
            role.SetCreator(user.Id);
            await context.Roles.AddAsync(role, cancellationToken);

            var userRole = new UserRole(user.Id, role.Id);
            await context.UserRoles.AddAsync(userRole, cancellationToken);

            #endregion

            #region 初始化工作空间

            // var workSpace = new WorkSpace("默认个人空间", "默认的个人空间");
            // workSpace.SetCreator(user.Id);
            // workSpace = (await context.WorkSpaces.AddAsync(workSpace, cancellationToken)).Entity;
            //
            // await context.SaveChangesAsync(cancellationToken);
            //
            // var workSpaceMember = new WorkSpaceMember(workSpace.Id, user.Id, WorkSpaceRoleType.Create);
            // await context.WorkSpaceMembers.AddAsync(workSpaceMember, cancellationToken);

            #endregion

            await transaction.CommitAsync(cancellationToken);
        }
    }

}