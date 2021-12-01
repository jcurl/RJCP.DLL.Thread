namespace RJCP.Threading.Tasks
{
    using System.Threading.Tasks;
    using BenchmarkDotNet.Attributes;

    public class ITaskBenchmark
    {
        private static ITask GetCompleted()
        {
            return Task.CompletedTask.AsITask();
        }

        [Benchmark]
        public ITask CompletedITask() { return GetCompleted(); }

        [Benchmark]
        public Task CompletedTask() { return Task.CompletedTask; }
    }
}
