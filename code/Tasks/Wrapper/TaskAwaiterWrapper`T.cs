namespace RJCP.Threading.Tasks.Wrapper
{
    using System;
    using System.Runtime.CompilerServices;

    internal readonly struct TaskAwaiterWrapper<TResult> : ITaskAwaiter<TResult>
    {
        private readonly TaskAwaiter<TResult> m_TaskAwaiter;

        public TaskAwaiterWrapper(TaskAwaiter<TResult> taskAwaiter)
        {
            m_TaskAwaiter = taskAwaiter;
        }

        public bool IsCompleted
        {
            get { return m_TaskAwaiter.IsCompleted; }
        }

        public TResult GetResult()
        {
            return m_TaskAwaiter.GetResult();
        }

        public void OnCompleted(Action continuation)
        {
            m_TaskAwaiter.OnCompleted(continuation);
        }

        public void UnsafeOnCompleted(Action continuation)
        {
            m_TaskAwaiter.UnsafeOnCompleted(continuation);
        }
    }
}
