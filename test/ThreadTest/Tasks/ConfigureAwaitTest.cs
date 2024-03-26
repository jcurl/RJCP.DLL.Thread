namespace RJCP.Threading.Tasks
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using NUnit.Framework;

    [TestFixture(false, false)]
    [TestFixture(false, true)]
    [TestFixture(true, false)]
    [TestFixture(true, true)]
    internal class ConfigureAwaitTest
    {
        private readonly bool m_UseITask;
        private readonly bool m_InternalAwait;

        public ConfigureAwaitTest(bool useITask, bool internalAwait)
        {
            m_UseITask = useITask;
            m_InternalAwait = internalAwait;
        }

        private async ITask MyDelay(int milliseconds)
        {
            if (milliseconds == 0) return;
            if (m_UseITask) {
                await Task.Delay(milliseconds).ConfigureAwait(m_InternalAwait);
            } else {
                await Task.Delay(milliseconds).AsITask().ConfigureAwait(m_InternalAwait);
            }
        }

        [TestCase(false)]
        [TestCase(true)]
        public async Task IsConfigureAwaitFalseMyDelay(bool withContext)
        {
            try {
                Console.WriteLine($"Thread ID Before = {Environment.CurrentManagedThreadId}");
                TestSynchronizationContext context = new();
                SynchronizationContext.SetSynchronizationContext(context);
                await MyDelay(10).ConfigureAwait(withContext);
                Console.WriteLine($"Thread ID after = {Environment.CurrentManagedThreadId}");

                int count = 0;
                if (withContext) count++;
                if (m_InternalAwait) count++;
                Assert.That(context.PostCalled, Is.EqualTo(count));
            } finally {
                SynchronizationContext.SetSynchronizationContext(null);
            }
        }

        [TestCase(false)]
        [TestCase(true)]
        public async Task IsConfigureAwaitFalseNoAwaitInMyDelay(bool withContext)
        {
            try {
                Console.WriteLine($"Thread ID Before = {Environment.CurrentManagedThreadId}");
                TestSynchronizationContext context = new();
                SynchronizationContext.SetSynchronizationContext(context);
                await MyDelay(0).ConfigureAwait(withContext);
                Console.WriteLine($"Thread ID after = {Environment.CurrentManagedThreadId}");

                int count = 0;
                Assert.That(context.PostCalled, Is.EqualTo(count));
            } finally {
                SynchronizationContext.SetSynchronizationContext(null);
            }
        }

        [TestCase(false)]
        [TestCase(true)]
        public async Task IsConfigureAwaitFalseMyDelayCompleted(bool withContext)
        {
            try {
                Console.WriteLine($"Thread ID Before = {Environment.CurrentManagedThreadId}");
                TestSynchronizationContext context = new();
                SynchronizationContext.SetSynchronizationContext(context);
                var delay = MyDelay(10).ConfigureAwait(withContext);
                Thread.Sleep(100);
                await delay;
                Console.WriteLine($"Thread ID after = {Environment.CurrentManagedThreadId}");

                int count = 0;
                if (m_InternalAwait) count++;
                Assert.That(context.PostCalled, Is.EqualTo(count));
            } finally {
                SynchronizationContext.SetSynchronizationContext(null);
            }
        }
    }
}
