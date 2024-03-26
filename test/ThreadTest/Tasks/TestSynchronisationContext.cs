namespace RJCP.Threading.Tasks
{
    using System.Threading;

    internal class TestSynchronizationContext : SynchronizationContext
    {
        private volatile int m_PostCalled = 0;

        public int PostCalled { get { return m_PostCalled; } }

        public override void Post(SendOrPostCallback d, object state)
        {
            Interlocked.Increment(ref m_PostCalled);
            base.Post(d, state);
        }
    }
}
