namespace RJCP.Threading.Tasks.Wrapper
{
    using System.Threading.Tasks;

    internal readonly struct TaskWrapper : ITask
    {
        private readonly Task m_Task;

        public TaskWrapper(Task task)
        {
            m_Task = task;
        }

        public IConfiguredTaskAwaitable ConfigureAwait(bool continueOnCapturedContext)
        {
            return new ConfiguredTaskAwaitableWrapper(m_Task, continueOnCapturedContext);
        }

        public ITaskAwaiter GetAwaiter()
        {
            return new TaskAwaiterWrapper(m_Task.GetAwaiter());
        }
    }
}
