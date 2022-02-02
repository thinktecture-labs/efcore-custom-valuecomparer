using Microsoft.EntityFrameworkCore;

namespace ValueComparerBenchmarking.Database;

public class SqlServerBenchmarkDbContext : BenchmarkDbContext
{
   public SqlServerBenchmarkDbContext(DbContextOptions<SqlServerBenchmarkDbContext> options)
      : base(options)
   {
   }

   protected override void OnModelCreating(ModelBuilder modelBuilder)
   {
      base.OnModelCreating(modelBuilder);
   }
}
