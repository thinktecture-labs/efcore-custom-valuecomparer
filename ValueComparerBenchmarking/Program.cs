using BenchmarkDotNet.Running;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using ValueComparerBenchmarking;
using ValueComparerBenchmarking.Database;


BenchmarkRunner.Run<ReferenceEqualityValueComparer>();
