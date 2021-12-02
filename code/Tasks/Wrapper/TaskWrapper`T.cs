namespace RJCP.Threading.Tasks.Wrapper
{
    using System.Threading.Tasks;

    internal readonly struct TaskWrapper<TResult> : ITask<TResult>
    {
        private readonly Task<TResult> m_Task;

        public TaskWrapper(Task<TResult> task)
        {
            m_Task = task;
        }

        public TResult Result
        {
            get { return m_Task.Result; }
        }

        IConfiguredTaskAwaitable ITask.ConfigureAwait(bool continueOnCapturedContext)
        {
            return new ConfiguredTaskAwaitableWrapper(m_Task, continueOnCapturedContext);
        }

        public IConfiguredTaskAwaitable<TResult> ConfigureAwait(bool continueOnCapturedContext)
        {
            return new ConfiguredTaskAwaitableWrapper<TResult>(m_Task, continueOnCapturedContext);
        }

        ITaskAwaiter ITask.GetAwaiter()
        {
            return new TaskAwaiterWrapper(((Task)m_Task).GetAwaiter());
        }

        public ITaskAwaiter<TResult> GetAwaiter()
        {
            return new TaskAwaiterWrapper<TResult>(m_Task.GetAwaiter());
        }
    }
}
