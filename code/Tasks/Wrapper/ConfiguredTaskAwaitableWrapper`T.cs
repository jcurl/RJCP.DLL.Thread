namespace RJCP.Threading.Tasks.Wrapper
{
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;

    internal readonly struct ConfiguredTaskAwaitableWrapper<TResult> : IConfiguredTaskAwaitable<TResult>
    {
        private readonly ConfiguredTaskAwaitable<TResult> m_Task;

        public ConfiguredTaskAwaitableWrapper(Task<TResult> task, bool continueOnCapturedContext)
        {
            m_Task = task.ConfigureAwait(continueOnCapturedContext);
        }

        public ITaskAwaiter<TResult> GetAwaiter()
        {
            return new ConfiguredTaskAwaiterWrapper<TResult>(m_Task.GetAwaiter());
        }
    }
}
