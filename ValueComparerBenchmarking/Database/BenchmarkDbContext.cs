using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace ValueComparerBenchmarking.Database;

public abstract class BenchmarkDbContext : DbContext
{
   public DbSet<EntityWithByteArray> EntitiesWithByteArray { get; set; } = null!;
   public DbSet<EntityWithByteArrayAndValueComparer> EntitiesWithByteArrayAndValueComparer { get; set; } = null!;

   protected BenchmarkDbContext(DbContextOptions options)
      : base(options)
   {
   }

   protected override void OnModelCreating(ModelBuilder modelBuilder)
   {
      modelBuilder.Entity<EntityWithByteArray>(builder =>
                                               {
                                                  builder.ToTable("EntitiesWithByteArray");
                                               });
      modelBuilder.Entity<EntityWithByteArrayAndValueComparer>(builder =>
                                                               {
                                                                  builder.ToTable("EntitiesWithByteArrayAndValueComparer");

                                                                  builder.Property(e => e.Bytes)
                                                                         .Metadata
                                                                         .SetValueComparer(new ValueComparer<byte[]>((obj, otherObj) => ReferenceEquals(obj, otherObj),
                                                                                                                     obj => obj.GetHashCode(),
                                                                                                                     obj => obj));
                                                               });
   }
}
