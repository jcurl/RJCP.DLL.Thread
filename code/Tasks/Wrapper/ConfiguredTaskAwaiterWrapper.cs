namespace RJCP.Threading.Tasks.Wrapper
{
    using System;
    using System.Runtime.CompilerServices;

    internal readonly struct ConfiguredTaskAwaiterWrapper : ITaskAwaiter
    {
        private readonly ConfiguredTaskAwaitable.ConfiguredTaskAwaiter m_TaskAwaiter;

        public ConfiguredTaskAwaiterWrapper(ConfiguredTaskAwaitable.ConfiguredTaskAwaiter taskAwaiter)
        {
            m_TaskAwaiter = taskAwaiter;
        }

        public bool IsCompleted
        {
            get { return m_TaskAwaiter.IsCompleted; }
        }

        public void OnCompleted(Action continuation)
        {
            m_TaskAwaiter.OnCompleted(continuation);
        }

        public void UnsafeOnCompleted(Action continuation)
        {
            m_TaskAwaiter.UnsafeOnCompleted(continuation);
        }

        public void GetResult()
        {
            m_TaskAwaiter.GetResult();
        }
    }
}
