# Benchmark Measurements

The benchmark measurements are done using
[BenchmarkDotNet](https://benchmarkdotnet.org/), a micro-benchmark tool. The
goal of this benchmark is to measure the time required to decode a small set of
messages that are already in memory. It doesn't measure all use cases or how
long it takes to decode files.

## Micro-benchmark Measurements

The parent repository contains the configuration file to make it easier to run
these benchmarks, but are in no way required.

From the parent repository, after building:

```cmd
$ git rj build -c preview --build
$ git rj perf thread
```

## Results

```text
Results = net6

BenchmarkDotNet=v0.13.12 OS=Windows 10 (10.0.19045.4046/22H2/2022Update)
Intel Core i7-6700T CPU 2.80GHz (Skylake), 1 CPU(s), 8 logical and 4 physical core(s)
  [HOST] : .NET 6.0.27 (6.0.2724.6912), X64 RyuJIT
```

```text
Results = net8

BenchmarkDotNet=v0.13.12 OS=Windows 10 (10.0.19045.4046/22H2/2022Update)
Intel Core i7-6700T CPU 2.80GHz (Skylake), 1 CPU(s), 8 logical and 4 physical core(s)
  [HOST] : .NET 8.0.1 (8.0.123.58001), X64 RyuJIT
```

| Project 'thread' Type | Method         | mean (net6) | stderr | mean (net8) | stderr |
|:----------------------|:---------------|------------:|-------:|------------:|-------:|
| ITaskBenchmark        | CompletedITask | 6.25        | 0.01   | 8.33        | 0.02   |
| ITaskBenchmark        | CompletedTask  | 0.28        | 0.01   | 0.29        | 0.00   |
