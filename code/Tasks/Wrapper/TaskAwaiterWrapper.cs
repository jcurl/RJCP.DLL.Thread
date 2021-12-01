namespace RJCP.Threading.Tasks.Wrapper
{
    using System;
    using System.Runtime.CompilerServices;

    internal readonly struct TaskAwaiterWrapper : ITaskAwaiter
    {
        private readonly TaskAwaiter m_TaskAwaiter;

        public TaskAwaiterWrapper(TaskAwaiter taskAwaiter)
        {
            m_TaskAwaiter = taskAwaiter;
        }

        public bool IsCompleted
        {
            get { return m_TaskAwaiter.IsCompleted; }
        }

        public void GetResult()
        {
            m_TaskAwaiter.GetResult();
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
