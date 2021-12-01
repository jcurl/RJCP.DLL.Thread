namespace RJCP.Threading.Tasks
{
    using System.Threading;
    using System.Threading.Tasks;
    using NUnit.Framework;

    [TestFixture]
    public class TaskTest
    {
        private static ITask Delay(int milliseconds)
        {
            return Task.Delay(milliseconds).AsITask();
        }

        private static ITask Completed()
        {
            return Task.CompletedTask.AsITask();
        }

        private static async Task Yield()
        {
            await Task.Yield();
        }

        [Test]
        public async ITask CompletedITask()
        {
            await Completed();
        }

        [TestCase(false)]
        [TestCase(true)]
        public async ITask CompletedITask(bool continueOnCapturedContext)
        {
            await Completed().ConfigureAwait(continueOnCapturedContext);
        }

        [Test]
        public async ITask DelayITask()
        {
            ITask delayTask = Delay(1);
            await delayTask;
        }

        [TestCase(false)]
        [TestCase(true)]
        public async ITask DelayITask(bool continueOnCapturedContext)
        {
            await Delay(1).ConfigureAwait(continueOnCapturedContext);
        }

        [TestCase(false)]
        [TestCase(true)]
        public async ITask ThreadSleepITask(bool continueOnCapturedContext)
        {
            TaskFactory tf = new TaskFactory();
            await tf.StartNew(() => { Thread.Sleep(1); }).AsITask().ConfigureAwait(continueOnCapturedContext);
        }

        [Test]
        public async ITask YieldITask()
        {
            await Yield().AsITask();
        }

        [TestCase(false)]
        [TestCase(true)]
        public async ITask YieldITask(bool continueOnCapturedContext)
        {
            await Yield().AsITask().ConfigureAwait(continueOnCapturedContext);
        }
    }
}
