using LuminaBrain.EntityFrameworkCore.DBContext;
using Microsoft.EntityFrameworkCore;

namespace LuminaBrain.EntityFramework.SqlServer;

public class SqlServerDbContext(DbContextOptions<SqlServerDbContext> options, IServiceProvider serviceProvider)
    : LuminaBrainContext<SqlServerDbContext>(options, serviceProvider)
{
}