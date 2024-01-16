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
Results = netcore31

BenchmarkDotNet=v0.13.1 OS=Windows 10.0.19045
Intel Core i7-6700T CPU 2.80GHz (Skylake), 1 CPU(s), 8 logical and 4 physical core(s)
  [HOST] : .NET Core 3.1.32 (CoreCLR 4.700.22.55902, CoreFX 4.700.22.56512), X64 RyuJIT
```

| Project 'thread' Type | Method         | mean (netcore31) | stderr |
|:----------------------|:---------------|-----------------:|-------:|
| ITaskBenchmark        | CompletedITask | 3.38             | 0.01   |
| ITaskBenchmark        | CompletedTask  | 0.29             | 0.00   |
