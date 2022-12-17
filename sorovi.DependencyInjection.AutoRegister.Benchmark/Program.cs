// See https://aka.ms/new-console-template for more information

using BenchmarkDotNet.Running;
using sorovi.DependencyInjection.AutoRegister.Benchmark;

var summary = BenchmarkRunner.Run<AddingToServiceCollectionBenchmark>();

