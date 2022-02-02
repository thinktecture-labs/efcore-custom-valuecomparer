using BenchmarkDotNet.Attributes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using ValueComparerBenchmarking.Database;

namespace ValueComparerBenchmarking;

[MemoryDiagnoser]
public class ReferenceEqualityValueComparer : IDisposable
{
   private readonly ServiceProvider _rootServiceProvider;
   private IServiceScope? _scope;
   private SqlServerBenchmarkDbContext? _dbContext;

   private const int _BYTES_LENGTH = 1024;

   private int _counter;
   private readonly byte[] _bytesBestCase = new byte[_BYTES_LENGTH];
   private readonly byte[] _bytesWorstCase = new byte[_BYTES_LENGTH];

   private List<EntityWithByteArray> _entitiesWithDefaultComparer = null!;
   private List<EntityWithByteArrayAndValueComparer> _entitiesWithCustomComparer = null!;

   public ReferenceEqualityValueComparer()
   {
      var connString = "server=localhost;database=CustomComparerDemo;integrated security=true";

      _rootServiceProvider = new ServiceCollection()
                             .AddDbContext<SqlServerBenchmarkDbContext>(builder => builder.UseSqlServer(connString)
                                                                                          .UseLoggerFactory(NullLoggerFactory.Instance))
                             .BuildServiceProvider();
   }

   [GlobalSetup]
   public void Initialize()
   {
      _scope = _rootServiceProvider.CreateScope();
      _dbContext = _scope.ServiceProvider.GetRequiredService<SqlServerBenchmarkDbContext>();

      _dbContext.Database.EnsureDeleted();
      _dbContext.Database.EnsureCreated();

      _dbContext.EntitiesWithByteArray.RemoveRange(_dbContext.EntitiesWithByteArray);
      _dbContext.EntitiesWithByteArrayAndValueComparer.RemoveRange(_dbContext.EntitiesWithByteArrayAndValueComparer);

      _dbContext.SaveChanges();

      var bytes = new byte[_BYTES_LENGTH];

      for (var i = 0; i < 10_000; i++)
      {
         var id = new Guid($"66AFED1B-92EA-4483-BF4F-{i.ToString("X").PadLeft(12, '0')}");

         _dbContext.EntitiesWithByteArray.Add(new EntityWithByteArray(id, bytes));
         _dbContext.EntitiesWithByteArrayAndValueComparer.Add(new EntityWithByteArrayAndValueComparer(id, bytes));
      }

      _dbContext.SaveChanges();
      _dbContext.ChangeTracker.Clear();
   }

   [GlobalCleanup]
   public void Dispose()
   {
      _scope?.Dispose();
      _rootServiceProvider.Dispose();
   }

   [IterationSetup]
   public void IterationSetup()
   {
      _dbContext!.ChangeTracker.Clear();
      _entitiesWithDefaultComparer = _dbContext.EntitiesWithByteArray.ToList();
      _entitiesWithCustomComparer = _dbContext.EntitiesWithByteArrayAndValueComparer.ToList();

      _bytesBestCase[0] = _bytesWorstCase[^1] = (byte)(++_counter % Byte.MaxValue);
   }

   [Benchmark]
   public async Task Default_BestCase()
   {
      _entitiesWithDefaultComparer.ForEach(e => e.Bytes = _bytesBestCase);

      await _dbContext!.SaveChangesAsync();
   }

   [Benchmark]
   public async Task Default_WorstCase()
   {
      _entitiesWithDefaultComparer.ForEach(e => e.Bytes = _bytesWorstCase);

      await _dbContext!.SaveChangesAsync();
   }

   [Benchmark]
   public async Task ReferenceEquality_BestCase()
   {
      _entitiesWithCustomComparer.ForEach(e => e.Bytes = _bytesBestCase);

      await _dbContext!.SaveChangesAsync();
   }

   [Benchmark]
   public async Task ReferenceEquality_WorstCase()
   {
      _entitiesWithCustomComparer.ForEach(e => e.Bytes = _bytesWorstCase);

      await _dbContext!.SaveChangesAsync();
   }
}
