namespace RJCP.Threading.Tasks
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using NUnit.Framework;
    using NUnit.Framework.Constraints;

    [TestFixture(UsageType.Direct)]
    [TestFixture(UsageType.Delegates)]
    [Timeout(5000)]
    public class TaskGroupTest
    {
        public enum UsageType
        {
            Direct,
            Delegates
        }

        private readonly UsageType m_UsageType;

        public TaskGroupTest(UsageType usageType)
        {
            m_UsageType = usageType;
        }

        [Test]
        public void DefaultTaskGroup()
        {
            TaskGroup group = new TaskGroup();
            Assert.That(group.WaitAll().IsCompleted, Is.False);
        }

        public enum EventAction
        {
            None,
            Cancel,
            Exception
        }

        private static async Task WaitForEvent(string name, ManualResetEventSlim signal, EventAction eventAction = EventAction.None)
        {
            await Task.Run(() => {
                Console.WriteLine($"{Environment.TickCount}: {name} Started");
                signal.Wait();
                Console.WriteLine($"{Environment.TickCount}: {name} Finished");
                switch (eventAction) {
                case EventAction.Cancel:
                    throw new OperationCanceledException();
                case EventAction.Exception:
                    throw new InvalidOperationException();
                }
            });
        }

        [Test]
        public async Task AddSingleTask()
        {
            TaskGroup group = new TaskGroup();

            using (ManualResetEventSlim signal = new ManualResetEventSlim(false)) {
                Task sigTask = null;
                if (m_UsageType == UsageType.Direct) {
                    sigTask = WaitForEvent("sigTask", signal);
                    Assert.That(group.RegisterTask(sigTask), Is.True);
                } else {
                    Assert.That(group.RegisterTask(() => {
                        sigTask = WaitForEvent("sigTask", signal);
                        return sigTask;
                    }), Is.True);
                }
                Assert.That(group, Is.Empty);  // Empty, because we're running.

                Task sigTriggerTask = Task.Run(() => {
                    Thread.Sleep(100);
                    signal.Set();
                });

                await group.WaitAll();
                Assert.That(sigTask.IsCompleted, Is.True);

                await sigTriggerTask;
                Assert.That(group, Is.Empty);  // Empty because we're finished.
            }
        }

        [Test]
        public async Task AddTasks()
        {
            TaskGroup group = new TaskGroup();

            using (ManualResetEventSlim signal1 = new ManualResetEventSlim(false))
            using (ManualResetEventSlim signal2 = new ManualResetEventSlim(false)) {
                Task sigTask1 = null;
                Task sigTask2 = null;
                if (m_UsageType == UsageType.Direct) {
                    sigTask1 = WaitForEvent("sigTask1", signal1);
                    Assert.That(group.RegisterTask(sigTask1), Is.True);
                    sigTask2 = WaitForEvent("sigTask2", signal2);
                    Assert.That(group.RegisterTask(sigTask2), Is.True);
                } else {
                    Assert.That(group.RegisterTask(() => {
                        sigTask1 = WaitForEvent("sigTask1", signal1);
                        return sigTask1;
                    }), Is.True);
                    Assert.That(group.RegisterTask(() => {
                        sigTask2 = WaitForEvent("sigTask2", signal2);
                        return sigTask2;
                    }), Is.True);
                }

                Task sigTriggerTask = Task.Run(() => {
                    Thread.Sleep(100);
                    signal1.Set();
                    Thread.Sleep(100);
                    signal2.Set();
                });

                await group.WaitAll();
                Assert.That(sigTask1.IsCompleted, Is.True);
                Assert.That(sigTask2.IsCompleted, Is.True);

                await sigTriggerTask;
            }
        }

        [Test]
        public async Task AddTaskWhileWaiting()
        {
            TaskGroup group = new TaskGroup();

            using (ManualResetEventSlim signal1 = new ManualResetEventSlim(false))
            using (ManualResetEventSlim signal2 = new ManualResetEventSlim(false)) {
                Task sigTask1 = null;
                if (m_UsageType == UsageType.Direct) {
                    sigTask1 = WaitForEvent("sigTask", signal1);
                    Assert.That(group.RegisterTask(sigTask1), Is.True);
                } else {
                    Assert.That(group.RegisterTask(() => {
                        sigTask1 = WaitForEvent("sigTask", signal1);
                        return sigTask1;
                    }), Is.True);
                }

                Task sigTask2 = null;
                bool registered = false;

                Task sigTriggerTask = Task.Run(() => {
                    Thread.Sleep(100);

                    // Must always register, as we register before sigTask1 is completed.
                    if (m_UsageType == UsageType.Direct) {
                        sigTask2 = WaitForEvent("sigTask2", signal2);
                        registered = group.RegisterTask(sigTask2);
                    } else {
                        registered = group.RegisterTask(() => {
                            sigTask2 = WaitForEvent("sigTask2", signal2);
                            return sigTask2;
                        });
                    }
                    signal1.Set();

                    Thread.Sleep(100);
                    signal2.Set();
                });

                await group.WaitAll();
                Assert.That(sigTask1.IsCompleted, Is.True);
                Assert.That(sigTask2.IsCompleted, Is.True);
                Assert.That(registered, Is.True);

                await sigTriggerTask;
            }
        }

        [Test]
        public async Task AddWhenComplete()
        {
            TaskGroup group = new TaskGroup();

            using (ManualResetEventSlim signal = new ManualResetEventSlim(false)) {
                Task sigTask = null;
                if (m_UsageType == UsageType.Direct) {
                    sigTask = WaitForEvent("sigTask", signal);
                    Assert.That(group.RegisterTask(sigTask), Is.True);
                } else {
                    Assert.That(group.RegisterTask(() => {
                        sigTask = WaitForEvent("sigTask", signal);
                        return sigTask;
                    }), Is.True);
                }

                Task sigTriggerTask = Task.Run(() => {
                    Thread.Sleep(100);
                    signal.Set();
                });

                await group.WaitAll();
                Assert.That(sigTask.IsCompleted, Is.True);

                await sigTriggerTask;

                if (m_UsageType == UsageType.Direct) {
                    Task delay = Task.Delay(10000);
                    Assert.That(group.RegisterTask(delay), Is.False);
                    Assert.That(delay.IsCompleted, Is.False);
                } else {
                    Task delay = null;
                    Assert.That(group.RegisterTask(() => {
                        delay = Task.Delay(10000);
                        return delay;
                    }), Is.False);
                    Assert.That(delay, Is.Null);
                }
            }
        }

        [TestCase(EventAction.Cancel)]
        [TestCase(EventAction.Exception)]
        public async Task AddTasksWithError(EventAction action)
        {
            TaskGroup group = new TaskGroup();

            using (ManualResetEventSlim signal1 = new ManualResetEventSlim(false))
            using (ManualResetEventSlim signal2 = new ManualResetEventSlim(false)) {
                Task sigTask1 = null;
                Task sigTask2 = null;
                if (m_UsageType == UsageType.Direct) {
                    sigTask1 = WaitForEvent("sigTask1", signal1, action);
                    Assert.That(group.RegisterTask(sigTask1), Is.True);
                    sigTask2 = WaitForEvent("sigTask2", signal2);
                    Assert.That(group.RegisterTask(sigTask2), Is.True);
                } else {
                    Assert.That(group.RegisterTask(() => {
                        sigTask1 = WaitForEvent("sigTask1", signal1, action);
                        return sigTask1;
                    }), Is.True);
                    Assert.That(group.RegisterTask(() => {
                        sigTask2 = WaitForEvent("sigTask2", signal2);
                        return sigTask2;
                    }), Is.True);
                }

                Assert.That(group, Is.Empty);  // Empty, because we're running.

                Task sigTriggerTask = Task.Run(() => {
                    Thread.Sleep(100);
                    signal1.Set();
                    Thread.Sleep(100);
                    signal2.Set();
                });

                ExactTypeConstraint constraint;
                switch (action) {
                case EventAction.Cancel:
                    constraint = Throws.TypeOf<TaskCanceledException>();
                    break;
                case EventAction.Exception:
                    constraint = Throws.TypeOf<InvalidOperationException>();
                    break;
                default:
                    throw new NotSupportedException();
                }

                Assert.That(async () => {
                    await group.WaitAll();
                }, constraint);

                Assert.That(sigTask1.IsCompleted, Is.True);
                Assert.That(group.Count, Is.EqualTo(1));  // One task was not completed
                Assert.That(group.FaultedTask, Is.EqualTo(sigTask1));

                // The first task cancelled, the second task is in the group. The group doesn't wait for the first task
                // to complete anymore. It is up to the user to cancel the remaining tasks.
                Assert.That(sigTask2.IsCompleted, Is.False);

                await sigTriggerTask;
            }
        }
    }
}
