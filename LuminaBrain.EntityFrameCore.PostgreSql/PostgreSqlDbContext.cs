using LuminaBrain.EntityFrameworkCore.DBContext;
using Microsoft.EntityFrameworkCore;

namespace LuminaBrain.EntityFrameCore.PostgreSql;

public class PostgreSqlDbContext(DbContextOptions<PostgreSqlDbContext> options, IServiceProvider serviceProvider)
    : LuminaBrainContext<PostgreSqlDbContext>(options, serviceProvider)
{
}