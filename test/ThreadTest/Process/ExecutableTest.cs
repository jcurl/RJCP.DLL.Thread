namespace RJCP.Threading.Process
{
    using System.Diagnostics;
    using System.Threading.Tasks;
    using NUnit.Framework;

    [TestFixture]
    public class ExecutableTest
    {
        [Test]
        public async Task CreateSleepToolAsync()
        {
            SleepTool sleep = await SleepTool.CreateAsync();
            Assert.That(sleep.IsAvailable, Is.True);
            Assert.That(sleep.BinaryPath, Is.Not.Null);
        }

        [Test]
        public async Task SleepToolSleepAsync()
        {
            SleepTool sleep = await SleepTool.CreateAsync();
            Stopwatch sw = Stopwatch.StartNew();
            await sleep.SleepAsync(1);
            Assert.That(sw.ElapsedMilliseconds, Is.GreaterThan(800));
        }

        [Test]
        public async Task SleepToolSleepAsyncParallel1()
        {
            SleepTool sleep = await SleepTool.CreateAsync(1);
            Stopwatch sw = Stopwatch.StartNew();

            // Run both tasks in parallel. It will only allow one to run at any single time.
            Task t1 = sleep.SleepAsync(1);
            Task t2 = sleep.SleepAsync(1);
            await t1;
            await t2;

            Assert.That(sw.ElapsedMilliseconds, Is.GreaterThan(1800).And.LessThan(2800));
        }

        [Test]
        public async Task SleepToolSleepAsyncParallel2()
        {
            SleepTool sleep = await SleepTool.CreateAsync(2);
            Stopwatch sw = Stopwatch.StartNew();

            // Run both tasks in parallel. It will allow both to run at any single time.
            Task t1 = sleep.SleepAsync(1);
            Task t2 = sleep.SleepAsync(1);
            await t1;
            await t2;

            Assert.That(sw.ElapsedMilliseconds, Is.GreaterThan(800).And.LessThan(1800));
        }

        [Test]
        public async Task SleepToolSleepAsyncParallel3()
        {
            SleepTool sleep = await SleepTool.CreateAsync(2);
            Stopwatch sw = Stopwatch.StartNew();

            // Run both tasks in parallel. It will allow two to run at any single time.
            Task t1 = sleep.SleepAsync(1);
            Task t2 = sleep.SleepAsync(1);
            Task t3 = sleep.SleepAsync(1);
            await t1;
            await t2;
            await t3;

            Assert.That(sw.ElapsedMilliseconds, Is.GreaterThan(1800).And.LessThan(2800));
        }

        [Test]
        public async Task CreateMockSleepToolAsync()
        {
            SleepTool sleep = await SleepToolMock.CreateSimAsync();
            Assert.That(sleep.IsAvailable, Is.True);
            Assert.That(sleep.BinaryPath, Is.Not.Null);
        }

        [Test]
        public async Task SleepToolMockSleepAsync()
        {
            SleepTool sleep = await SleepToolMock.CreateSimAsync();
            Stopwatch sw = Stopwatch.StartNew();
            await sleep.SleepAsync(1);
            Assert.That(sw.ElapsedMilliseconds, Is.LessThan(500));
        }
    }
}
