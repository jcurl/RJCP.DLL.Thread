namespace RJCP.Threading.Tasks.Wrapper
{
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;

    internal readonly struct ConfiguredTaskAwaitableWrapper : IConfiguredTaskAwaitable
    {
        private readonly ConfiguredTaskAwaitable m_Task;

        public ConfiguredTaskAwaitableWrapper(Task task, bool continueOnCapturedContext)
        {
            m_Task = task.ConfigureAwait(continueOnCapturedContext);
        }

        public ITaskAwaiter GetAwaiter()
        {
            return new ConfiguredTaskAwaiterWrapper(m_Task.GetAwaiter());
        }
    }
}
