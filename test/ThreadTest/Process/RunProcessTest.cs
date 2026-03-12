namespace RJCP.Threading.Process
{
    using System;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using NUnit.Framework;
    using RJCP.CodeQuality.NUnitExtensions;

    public class RunProcessTest
    {
        private static string GetTimeoutBinary()
        {
            string stimeBinary;
            if (IntPtr.Size == 4) {
                stimeBinary = Path.Combine(Deploy.WorkDirectory, "Resources", "stimeout", "x86", "Release", "stimeout.exe");
            } else {
                stimeBinary = Path.Combine(Deploy.WorkDirectory, "Resources", "stimeout", "x64", "Release", "stimeout.exe");
            }

            if (!File.Exists(stimeBinary))
                throw new FileNotFoundException($"Could not find {stimeBinary}");
            return stimeBinary;
        }

        [Test]
        [Platform(Include = "Win32")]
        public void ProcessRunStatic()
        {
            RunProcess process = RunProcess.Run("cmd", new[] { "/c", "dir" });
            Assert.That(process.ExitCode, Is.Zero);
            Assert.That(process.Id, Is.GreaterThan(0));
            Assert.That(process.StdOut, Is.Not.Empty);
            Assert.That(process.Command, Is.EqualTo("cmd /c dir"));

            // Dump the contents for evaluation later
            foreach (string line in process.StdOut) {
                Console.WriteLine("stdout: {0}", line);
            }
            foreach (string line in process.StdErr) {
                Console.WriteLine("stderr: {0}", line);
            }
        }

        [Test]
        [Platform(Include = "Unix")]
        public void ProcessRunStaticLinux()
        {
            Console.WriteLine("Starting test");
            RunProcess process = RunProcess.Run("/bin/sh", new[] { "-c", "ls -l" });
            Assert.That(process.ExitCode, Is.Zero);
            Assert.That(process.Id, Is.GreaterThan(0));
            Assert.That(process.StdOut, Is.Not.Empty);
            Assert.That(process.Command, Is.EqualTo("/bin/sh -c \"ls -l\""));

            // Dump the contents for evaluation later
            foreach (string line in process.StdOut) {
                Console.WriteLine("stdout: {0}", line);
            }
            foreach (string line in process.StdErr) {
                Console.WriteLine("stderr: {0}", line);
            }
        }

        [Test]
        [Platform(Include = "Win32")]
        public void ProcessRunFromStatic()
        {
            string current = Environment.CurrentDirectory;
            RunProcess process = RunProcess.RunFrom("cmd", current, new[] { "/c", "dir" });
            Assert.That(process.ExitCode, Is.Zero);
            Assert.That(process.Id, Is.GreaterThan(0));
            Assert.That(process.StdOut, Is.Not.Empty);

            // Dump the contents for evaluation later
            foreach (string line in process.StdOut) {
                Console.WriteLine("stdout: {0}", line);
            }
            foreach (string line in process.StdErr) {
                Console.WriteLine("stderr: {0}", line);
            }
        }

        [Test]
        [Platform(Include = "Win32")]
        public void ProcessRunFromAsyncResult()
        {
            string current = Environment.CurrentDirectory;
            RunProcess process = new("cmd", current, new[] { "/c", "dir" });
            ProcessRunFromAsyncResult(process);
            Assert.That(process.Id, Is.GreaterThan(0));
        }

        [Test]
        public void ProcessSimRunFromAsyncResult()
        {
            string current = Environment.CurrentDirectory;
            RunProcess process = new GetDirSimProcess("cmd", current, new[] { "/c", "dir" });
            ProcessRunFromAsyncResult(process);
            Assert.That(process.Id, Is.Zero);
        }

        private static void ProcessRunFromAsyncResult(RunProcess process)
        {
            IAsyncResult ar = process.BeginExecute(null, null);
            process.EndExecute(ar);

            Assert.That(process.ExitCode, Is.Zero);
            Assert.That(process.StdOut, Is.Not.Empty);

            // Dump the contents for evaluation later
            foreach (string line in process.StdOut) {
                Console.WriteLine("stdout: {0}", line);
            }
            foreach (string line in process.StdErr) {
                Console.WriteLine("stderr: {0}", line);
            }
        }

        [Test]
        [Platform(Include = "Win32")]
        public void ProcessRunFromAsyncResultTerminate()
        {
            string current = Environment.CurrentDirectory;
            RunProcess process = new(GetTimeoutBinary(), current, new[] { "10" });
            ProcessRunFromAsyncResultTerminate(process);
            Assert.That(process.Id, Is.GreaterThan(0));
        }

        [Test]
        public void ProcessSimRunFromAsyncResultTerminate()
        {
            string current = Environment.CurrentDirectory;
            RunProcess process = new TimeoutSimProcess(GetTimeoutBinary(), current, new[] { "10" });
            ProcessRunFromAsyncResultTerminate(process);
            Assert.That(process.Id, Is.Zero);
        }

        private static void ProcessRunFromAsyncResultTerminate(RunProcess process)
        {
            IAsyncResult ar = process.BeginExecute(null, null);
            Thread.Sleep(100);
            process.Terminate();

            // Note that here, we might not get the exit code, as the process may not have immediately exited
            try {
                int exitCode = process.ExitCode;
                Assert.That(exitCode, Is.EqualTo(-1));
            } catch (InvalidOperationException) { /* Ignore exception */ }

            process.EndExecute(ar);
            Assert.That(process.ExitCode, Is.EqualTo(-1));
        }

        [Test]
        [Platform(Include = "Win32")]
        public void ProcessRunFromAsyncResultTerminateBefore()
        {
            string current = Environment.CurrentDirectory;
            RunProcess process = new(GetTimeoutBinary(), current, new[] { "1" });
            ProcessRunFromAsyncResultTerminateBefore(process);
        }

        [Test]
        public void ProcessSimRunFromAsyncResultTerminateBefore()
        {
            string current = Environment.CurrentDirectory;
            RunProcess process = new TimeoutSimProcess(GetTimeoutBinary(), current, new[] { "1" });
            ProcessRunFromAsyncResultTerminateBefore(process);
        }

        private static void ProcessRunFromAsyncResultTerminateBefore(RunProcess process)
        {
            process.Terminate();
            IAsyncResult ar = process.BeginExecute(null, null);

            // Note that here, we might not get the exit code, as the process may not have immediately exited
            try {
                int exitCode = process.ExitCode;
                Assert.That(exitCode, Is.EqualTo(-1));
            } catch (InvalidOperationException) { /* Ignore exception */ }

            process.EndExecute(ar);
            Assert.That(process.ExitCode, Is.EqualTo(-1));

            // The process was terminated before started, so the process wasn't executed.
            Assert.That(process.Id, Is.Zero);
        }

        [Test]
        [Platform(Include = "Win32")]
        public void ProcessRunFromAsyncResultTerminateAfter()
        {
            string current = Environment.CurrentDirectory;
            RunProcess process = new(GetTimeoutBinary(), current, new[] { "1" });
            ProcessRunFromAsyncResultTerminateAfter(process);
            Assert.That(process.Id, Is.GreaterThan(0));
        }

        [Test]
        public void ProcessSimRunFromAsyncResultTerminateAfter()
        {
            string current = Environment.CurrentDirectory;
            RunProcess process = new TimeoutSimProcess(GetTimeoutBinary(), current, new[] { "1" });
            ProcessRunFromAsyncResultTerminateAfter(process);
            Assert.That(process.Id, Is.Zero);
        }

        private static void ProcessRunFromAsyncResultTerminateAfter(RunProcess process)
        {
            IAsyncResult ar = process.BeginExecute(null, null);
            Assert.That(() => { _ = process.ExitCode; }, Throws.TypeOf<InvalidOperationException>());
            Thread.Sleep(2000);
            process.Terminate();

            process.EndExecute(ar);
            Assert.That(process.ExitCode, Is.Zero);
        }

        [Test]
        [Platform(Include = "Win32")]
        public void ProcessRunFromAsyncResultTerminateTwice()
        {
            string current = Environment.CurrentDirectory;
            RunProcess process = new(GetTimeoutBinary(), current, new[] { "10" });
            ProcessRunFromAsyncResultTerminateTwice(process);
            Assert.That(process.Id, Is.GreaterThan(0));
        }

        [Test]
        public void ProcessSimRunFromAsyncResultTerminateTwice()
        {
            string current = Environment.CurrentDirectory;
            RunProcess process = new TimeoutSimProcess(GetTimeoutBinary(), current, new[] { "10" });
            ProcessRunFromAsyncResultTerminateTwice(process);
            Assert.That(process.Id, Is.Zero);
        }

        private static void ProcessRunFromAsyncResultTerminateTwice(RunProcess process)
        {
            IAsyncResult ar = process.BeginExecute(null, null);
            Thread.Sleep(100);
            process.Terminate();
            process.Terminate();
            process.EndExecute(ar);
            Assert.That(process.ExitCode, Is.EqualTo(-1));
        }

        [Test]
        [Platform(Include = "Win32")]
        public void ProcessRunFromGetStdOut()
        {
            string current = Environment.CurrentDirectory;
            RunProcess process = new("cmd", current, new[] { "/c", "dir" });
            ProcessRunFromGetStdOut(process);
            Assert.That(process.Id, Is.GreaterThan(0));
        }

        [Test]
        public void ProcessSimRunFromGetStdOut()
        {
            string current = Environment.CurrentDirectory;
            RunProcess process = new GetDirSimProcess("cmd", current, new[] { "/c", "dir" });
            ProcessRunFromGetStdOut(process);
            Assert.That(process.Id, Is.Zero);
        }

        private static void ProcessRunFromGetStdOut(RunProcess process)
        {
            int lines = 0;
            process.OutputDataReceived += (s, e) => { lines++; };

            IAsyncResult ar = process.BeginExecute(null, null);
            process.EndExecute(ar);

            Assert.That(process.ExitCode, Is.Zero);
            Assert.That(process.StdOut, Is.Not.Empty);

            // Dump the contents for evaluation later
            foreach (string line in process.StdOut) {
                Console.WriteLine("stdout: {0}", line);
            }
            foreach (string line in process.StdErr) {
                Console.WriteLine("stderr: {0}", line);
            }

            Assert.That(lines, Is.EqualTo(process.StdOut.Count));
        }

        [Test]
        [Platform(Include = "Win32")]
        public async Task ProcessRunFromAsync()
        {
            string current = Environment.CurrentDirectory;
            RunProcess process = new("cmd", current, new[] { "/c", "dir" });
            await ProcessRunFromAsync(process);
            Assert.That(process.Id, Is.GreaterThan(0));
        }

        [Test]
        public async Task ProcessSimRunFromAsync()
        {
            string current = Environment.CurrentDirectory;
            RunProcess process = new GetDirSimProcess("cmd", current, new[] { "/c", "dir" });
            await ProcessRunFromAsync(process);
            Assert.That(process.Id, Is.Zero);
        }

        private static async Task ProcessRunFromAsync(RunProcess process)
        {
            await process.ExecuteAsync();
            Assert.That(process.ExitCode, Is.Zero);
            Assert.That(process.StdOut, Is.Not.Empty);

            // Dump the contents for evaluation later
            foreach (string line in process.StdOut) {
                Console.WriteLine("stdout: {0}", line);
            }
            foreach (string line in process.StdErr) {
                Console.WriteLine("stderr: {0}", line);
            }
        }

        [Test]
        [Platform(Include = "Win32")]
        public void ProcessRunFromAsyncCancel()
        {
            string current = Environment.CurrentDirectory;
            RunProcess process = new(GetTimeoutBinary(), current, new[] { "10" });
            ProcessRunFromAsyncCancel(process);
            Assert.That(process.Id, Is.GreaterThan(0));
        }

        [Test]
        public void ProcessSimRunFromAsyncCancel()
        {
            string current = Environment.CurrentDirectory;
            RunProcess process = new TimeoutSimProcess(GetTimeoutBinary(), current, new[] { "10" });
            ProcessRunFromAsyncCancel(process);
            Assert.That(process.Id, Is.Zero);
        }

        private static void ProcessRunFromAsyncCancel(RunProcess process)
        {
            using (CancellationTokenSource tokens = new()) {
                Task t = Task.Delay(100).ContinueWith((_) => { tokens.Cancel(); });
                Assert.That(() => {
                    Task.WaitAll(process.ExecuteAsync(tokens.Token), t);
                }, Throws.TypeOf<AggregateException>().With.InnerException.TypeOf<TaskCanceledException>());
                Assert.That(process.ExitCode, Is.EqualTo(-1));
            }
        }

        [Test]
        [Platform(Include = "Win32")]
        public async Task ProcessRunFromAsyncCancelTerminateBefore()
        {
            string current = Environment.CurrentDirectory;
            RunProcess process = new(GetTimeoutBinary(), current, new[] { "10" });
            await ProcessRunFromAsyncCancelTerminateBefore(process);
        }

        [Test]
        public async Task ProcessSimRunFromAsyncCancelTerminateBefore()
        {
            string current = Environment.CurrentDirectory;
            RunProcess process = new TimeoutSimProcess(GetTimeoutBinary(), current, new[] { "10" });
            await ProcessRunFromAsyncCancelTerminateBefore(process);
        }

        private static async Task ProcessRunFromAsyncCancelTerminateBefore(RunProcess process)
        {
            using (CancellationTokenSource tokens = new()) {
                process.Terminate();
                await process.ExecuteAsync(tokens.Token);
                Assert.That(process.ExitCode, Is.EqualTo(-1));

                // Terminated before starting, so the PID is zero.
                Assert.That(process.Id, Is.Zero);
            }
        }

        [Test]
        [Platform(Include = "Win32")]
        [Repeat(10)]
        public void ProcessRunFromAsyncCancelImmediatelyBefore()
        {
            string current = Environment.CurrentDirectory;
            RunProcess process = new(GetTimeoutBinary(), current, new[] { "10" });
            ProcessRunFromAsyncCancelImmediatelyBefore(process);
        }

        [Test]
        [Repeat(10)]
        public void ProcessSimRunFromAsyncCancelImmediatelyBefore()
        {
            string current = Environment.CurrentDirectory;
            RunProcess process = new TimeoutSimProcess(GetTimeoutBinary(), current, new[] { "10" });
            ProcessRunFromAsyncCancelImmediatelyBefore(process);
        }

        private static void ProcessRunFromAsyncCancelImmediatelyBefore(RunProcess process)
        {
            using (CancellationTokenSource tokens = new()) {
                tokens.Cancel();
                Assert.That(() => {
                    process.ExecuteAsync(tokens.Token).Wait();
                }, Throws.TypeOf<AggregateException>().With.InnerException.TypeOf<TaskCanceledException>());
                Assert.That(process.ExitCode, Is.EqualTo(-1));

                // It never started, so the PID is zero.
                Assert.That(process.Id, Is.Zero);
            }
        }

        [Test]
        [Platform(Include = "Win32")]
        [Repeat(10)]
        public void ProcessRunFromAsyncCancelImmediatelyAfter()
        {
            string current = Environment.CurrentDirectory;
            RunProcess process = new(GetTimeoutBinary(), current, new[] { "10" });
            ProcessRunFromAsyncCancelImmediatelyAfter(process);
            Assert.That(process.Id, Is.GreaterThan(0));
        }

        [Test]
        [Repeat(10)]
        public void ProcessSimRunFromAsyncCancelImmediatelyAfter()
        {
            string current = Environment.CurrentDirectory;
            RunProcess process = new TimeoutSimProcess(GetTimeoutBinary(), current, new[] { "10" });
            ProcessRunFromAsyncCancelImmediatelyAfter(process);
            Assert.That(process.Id, Is.Zero);
        }

        private static void ProcessRunFromAsyncCancelImmediatelyAfter(RunProcess process)
        {
            using (CancellationTokenSource tokens = new()) {
                Task t = process.ExecuteAsync(tokens.Token);
                tokens.Cancel();
                Assert.That(() => {
                    t.Wait();
                }, Throws.TypeOf<AggregateException>().With.InnerException.TypeOf<TaskCanceledException>());
                Assert.That(process.ExitCode, Is.EqualTo(-1));
            }
        }

        [Test]
        [Platform(Include = "Win32")]
        [Repeat(10)]
        public void ProcessRunFromAsyncCancelImmediatelyParallel()
        {
            string current = Environment.CurrentDirectory;
            RunProcess process = new(GetTimeoutBinary(), current, new[] { "10" });
            ProcessRunFromAsyncCancelImmediatelyParallel(process);
        }

        [Test]
        [Repeat(10)]
        public void ProcessSimRunFromAsyncCancelImmediatelyParallel()
        {
            string current = Environment.CurrentDirectory;
            RunProcess process = new TimeoutSimProcess(GetTimeoutBinary(), current, new[] { "10" });
            ProcessRunFromAsyncCancelImmediatelyParallel(process);
        }

        private static void ProcessRunFromAsyncCancelImmediatelyParallel(RunProcess process)
        {
            using (CancellationTokenSource tokens = new()) {
                Task c = Task.Run(() => { tokens.Cancel(); });
                Task p = process.ExecuteAsync(tokens.Token);
                Assert.That(() => {
                    Task.WaitAll(c, p);
                }, Throws.TypeOf<AggregateException>().With.InnerException.TypeOf<TaskCanceledException>());
                Assert.That(process.ExitCode, Is.EqualTo(-1));

                // We don't check the PID, as we don't know if it started or not. The test case contains a race
                // condition.
            }
        }

        [Test]
        [Platform(Include = "Win32")]
        public void ProcessRunFromAsyncTerminate()
        {
            string current = Environment.CurrentDirectory;
            RunProcess process = new(GetTimeoutBinary(), current, new[] { "10" });
            ProcessRunFromAsyncTerminate(process);
            Assert.That(process.Id, Is.GreaterThan(0));
        }

        [Test]
        public void ProcessSimRunFromAsyncTerminate()
        {
            string current = Environment.CurrentDirectory;
            RunProcess process = new TimeoutSimProcess(GetTimeoutBinary(), current, new[] { "10" });
            ProcessRunFromAsyncTerminate(process);
            Assert.That(process.Id, Is.Zero);
        }

        private static void ProcessRunFromAsyncTerminate(RunProcess process)
        {
            Task t = Task.Delay(100).ContinueWith((_) => { process.Terminate(); });
            Task.WaitAll(process.ExecuteAsync(), t);
            Assert.That(process.ExitCode, Is.EqualTo(-1));
        }

        [Test]
        [Platform(Include = "Win32")]
        public async Task ProcessRunFromAsyncTerminateBefore()
        {
            string current = Environment.CurrentDirectory;
            RunProcess process = new(GetTimeoutBinary(), current, new[] { "10" });
            await ProcessRunFromAsyncTerminateBefore(process);
        }

        [Test]
        public async Task ProcessSimRunFromAsyncTerminateBefore()
        {
            string current = Environment.CurrentDirectory;
            RunProcess process = new TimeoutSimProcess(GetTimeoutBinary(), current, new[] { "10" });
            await ProcessRunFromAsyncTerminateBefore(process);
        }

        private static async Task ProcessRunFromAsyncTerminateBefore(RunProcess process)
        {
            process.Terminate();
            await process.ExecuteAsync();
            Assert.That(process.ExitCode, Is.EqualTo(-1));

            // Process was terminated before it started, so there can be no PID.
            Assert.That(process.Id, Is.Zero);
        }

        [Test]
        [Platform(Include = "Win32")]
        public async Task ProcessRunFromAsyncStatic()
        {
            string current = Environment.CurrentDirectory;

            RunProcess process = await RunProcess.RunFromAsync("cmd", current, new[] { "/c", "dir" });
            Assert.That(process.ExitCode, Is.Zero);
            Assert.That(process.Id, Is.GreaterThan(0));
            Assert.That(process.StdOut, Is.Not.Empty);

            // Dump the contents for evaluation later
            foreach (string line in process.StdOut) {
                Console.WriteLine("stdout: {0}", line);
            }
            foreach (string line in process.StdErr) {
                Console.WriteLine("stderr: {0}", line);
            }
        }

        [Test]
        [Platform(Include = "Win32")]
        public void ProcessRunFromAsyncStaticCancel()
        {
            string current = Environment.CurrentDirectory;

            using (CancellationTokenSource tokens = new()) {
                Task<RunProcess> process = RunProcess.RunFromAsync(GetTimeoutBinary(), current, new[] { "10" }, tokens.Token);
                Task t = Task.Delay(100).ContinueWith((_) => { tokens.Cancel(); });

                Assert.That(() => {
                    Task.WaitAll(process, t);
                }, Throws.TypeOf<AggregateException>().With.InnerException.TypeOf<TaskCanceledException>());

                Assert.That(() => {
                    // It's not possible to get the exit code here.
                    _ = process.Result.ExitCode;
                }, Throws.TypeOf<AggregateException>().With.InnerException.TypeOf<TaskCanceledException>());

                Assert.That(process.Id, Is.GreaterThan(0));
            }
        }

        [Test]
        [Platform(Include = "Win32")]
        public async Task ProcessRunAsyncStatic()
        {
            RunProcess process = await RunProcess.RunAsync("cmd", new[] { "/c", "dir" });
            Assert.That(process.ExitCode, Is.Zero);
            Assert.That(process.Id, Is.GreaterThan(0));
            Assert.That(process.StdOut, Is.Not.Empty);

            // Dump the contents for evaluation later
            foreach (string line in process.StdOut) {
                Console.WriteLine("stdout: {0}", line);
            }
            foreach (string line in process.StdErr) {
                Console.WriteLine("stderr: {0}", line);
            }
        }

        [Test]
        [Platform(Include = "Win32")]
        public void ProcessRunAsyncStaticCancel()
        {
            using (CancellationTokenSource tokens = new()) {
                Task<RunProcess> process = RunProcess.RunAsync(GetTimeoutBinary(), new[] { "10" }, tokens.Token);
                Task t = Task.Delay(100).ContinueWith((_) => { tokens.Cancel(); });

                Assert.That(() => {
                    Task.WaitAll(process, t);
                }, Throws.TypeOf<AggregateException>().With.InnerException.TypeOf<TaskCanceledException>());

                Assert.That(() => {
                    // It's not possible to get the exit code here.
                    _ = process.Result.ExitCode;
                }, Throws.TypeOf<AggregateException>().With.InnerException.TypeOf<TaskCanceledException>());

                Assert.That(process.Id, Is.GreaterThan(0));
            }
        }

        [Test]
        [Platform(Include = "Win32")]
        public void ProcessExecuteTwice()
        {
            string currentDir = Environment.CurrentDirectory;
            RunProcess process = new("cmd", currentDir, new string[] { "/c", "dir" });
            ProcessExecuteTwice(process);
        }

        [Test]
        public void ProcessSimExecuteTwice()
        {
            string currentDir = Environment.CurrentDirectory;
            RunProcess process = new GetDirSimProcess("cmd", currentDir, new[] { "/c", "dir" });
            ProcessExecuteTwice(process);
        }

        private static void ProcessExecuteTwice(RunProcess process)
        {
            process.Execute();
            Assert.That(() => {
                process.Execute();
            }, Throws.TypeOf<InvalidOperationException>());
        }

        [Test]
        [Platform(Include = "Win32")]
        public async Task ProcessExecuteAsyncTwice()
        {
            string currentDir = Environment.CurrentDirectory;
            RunProcess process = new("cmd", currentDir, new string[] { "/c", "dir" });
            await ProcessExecuteAsyncTwice(process);
        }

        [Test]
        public async Task ProcessSimExecuteAsyncTwice()
        {
            string currentDir = Environment.CurrentDirectory;
            RunProcess process = new GetDirSimProcess("cmd", currentDir, new[] { "/c", "dir" });
            await ProcessExecuteAsyncTwice(process);
        }

        private static async Task ProcessExecuteAsyncTwice(RunProcess process)
        {
            await process.ExecuteAsync();
            Assert.That(async () => {
                await process.ExecuteAsync();
            }, Throws.TypeOf<InvalidOperationException>());
        }

        [Test]
        [Platform(Include = "Win32")]
        public async Task ProcessExecuteAsyncWithTokenTwice()
        {
            string currentDir = Environment.CurrentDirectory;
            RunProcess process = new("cmd", currentDir, new string[] { "/c", "dir" });
            await ProcessExecuteAsyncWithTokenTwice(process);
        }

        [Test]
        public async Task ProcessSimExecuteAsyncWithTokenTwice()
        {
            string currentDir = Environment.CurrentDirectory;
            RunProcess process = new GetDirSimProcess("cmd", currentDir, new[] { "/c", "dir" });
            await ProcessExecuteAsyncWithTokenTwice(process);
        }

        private static async Task ProcessExecuteAsyncWithTokenTwice(RunProcess process)
        {
            using (CancellationTokenSource tokens = new()) {
                await process.ExecuteAsync(tokens.Token);
                Assert.That(async () => {
                    await process.ExecuteAsync(tokens.Token);
                }, Throws.TypeOf<InvalidOperationException>());
            }
        }

        [Test]
        [Platform(Include = "Win32")]
        public void ProcessBeginExecuteTwice()
        {
            string currentDir = Environment.CurrentDirectory;
            RunProcess process = new("cmd", currentDir, new string[] { "/c", "dir" });
            ProcessBeginExecuteTwice(process);
        }

        [Test]
        public void ProcessSimBeginExecuteTwice()
        {
            string currentDir = Environment.CurrentDirectory;
            RunProcess process = new GetDirSimProcess("cmd", currentDir, new[] { "/c", "dir" });
            ProcessBeginExecuteTwice(process);
        }

        private static void ProcessBeginExecuteTwice(RunProcess process)
        {
            var ar = process.BeginExecute(null, null);
            process.EndExecute(ar);

            Assert.That(() => {
                _ = process.BeginExecute(null, null);
            }, Throws.TypeOf<InvalidOperationException>());
        }

        [Test]
        public async Task EventOverrideTypes()
        {
            SleepProcess sleep = new(GetTimeoutBinary(), "info");
            await sleep.ExecuteAsync();

            using (Assert.EnterMultipleScope()) {
                Assert.That(sleep.CheckOnProcessExecEventType.Received, Is.True);
                Assert.That(sleep.CheckOnProcessExitEventType.Received, Is.True);
                Assert.That(
                    sleep.CheckOnOutputDataReceivedType.Received |
                    sleep.CheckOnErrorDataReceivedType.Received, Is.True);
            }

            using (Assert.EnterMultipleScope()) {
                Assert.That(sleep.CheckOnProcessExecEventType.SenderTypeCorrect, Is.True);
                Assert.That(sleep.CheckOnProcessExitEventType.SenderTypeCorrect, Is.True);
                if (sleep.CheckOnOutputDataReceivedType.Received)
                    Assert.That(sleep.CheckOnOutputDataReceivedType.SenderTypeCorrect, Is.True);
                if (sleep.CheckOnErrorDataReceivedType.Received)
                    Assert.That(sleep.CheckOnErrorDataReceivedType.SenderTypeCorrect, Is.True);
            }
        }

        [Test]
        public async Task ProcessExecEventArgs()
        {
            bool command = false;
            bool args = false;

            SleepProcess sleep = new(GetTimeoutBinary(), "info");
            sleep.ProcessExecEvent += (sender, e) => {
                command = !string.IsNullOrWhiteSpace(e.Command);
                args = (e.Arguments is not null) && e.Arguments.Length > 0;
            };

            await sleep.ExecuteAsync();

            // The working directory may be null or empty, if it was given to us null or empty.
            using (Assert.EnterMultipleScope()) {
                Assert.That(command, Is.True);
                Assert.That(args, Is.True);
            }
        }

        [Test]
        public async Task ExceptionInOutputEvent()
        {
            SleepProcess sleep = new(GetTimeoutBinary(), "info") {
                RaiseExceptionOnOutputEvent = true
            };

            await sleep.ExecuteAsync();

            using (Assert.EnterMultipleScope()) {
                Assert.That(sleep.CheckOnProcessExecEventType.Received, Is.True);
                Assert.That(sleep.CheckOnProcessExitEventType.Received, Is.True);
                Assert.That(
                    sleep.CheckOnOutputDataReceivedType.Received |
                    sleep.CheckOnErrorDataReceivedType.Received, Is.True);
            }

            using (Assert.EnterMultipleScope()) {
                Assert.That(sleep.CheckOnProcessExecEventType.SenderTypeCorrect, Is.True);
                Assert.That(sleep.CheckOnProcessExitEventType.SenderTypeCorrect, Is.True);
                if (sleep.CheckOnOutputDataReceivedType.Received)
                    Assert.That(sleep.CheckOnOutputDataReceivedType.SenderTypeCorrect, Is.True);
                if (sleep.CheckOnErrorDataReceivedType.Received)
                    Assert.That(sleep.CheckOnErrorDataReceivedType.SenderTypeCorrect, Is.True);
            }
        }

        [Test]
        public async Task ExceptionInErrorEvent()
        {
            SleepProcess sleep = new(GetTimeoutBinary(), "info") {
                RaiseExceptionOnErrorEvent = true
            };

            await sleep.ExecuteAsync();

            using (Assert.EnterMultipleScope()) {
                Assert.That(sleep.CheckOnProcessExecEventType.Received, Is.True);
                Assert.That(sleep.CheckOnProcessExitEventType.Received, Is.True);
                Assert.That(
                    sleep.CheckOnOutputDataReceivedType.Received |
                    sleep.CheckOnErrorDataReceivedType.Received, Is.True);
            }

            using (Assert.EnterMultipleScope()) {
                Assert.That(sleep.CheckOnProcessExecEventType.SenderTypeCorrect, Is.True);
                Assert.That(sleep.CheckOnProcessExitEventType.SenderTypeCorrect, Is.True);
                if (sleep.CheckOnOutputDataReceivedType.Received)
                    Assert.That(sleep.CheckOnOutputDataReceivedType.SenderTypeCorrect, Is.True);
                if (sleep.CheckOnErrorDataReceivedType.Received)
                    Assert.That(sleep.CheckOnErrorDataReceivedType.SenderTypeCorrect, Is.True);
            }
        }

        [Test]
        public async Task ExceptionInExecEvent()
        {
            SleepProcess sleep = new(GetTimeoutBinary(), "info") {
                RaiseExceptionOnExecEvent = true
            };

            await sleep.ExecuteAsync();

            using (Assert.EnterMultipleScope()) {
                Assert.That(sleep.CheckOnProcessExecEventType.Received, Is.True);
                Assert.That(sleep.CheckOnProcessExitEventType.Received, Is.True);
                Assert.That(
                    sleep.CheckOnOutputDataReceivedType.Received |
                    sleep.CheckOnErrorDataReceivedType.Received, Is.True);
            }

            using (Assert.EnterMultipleScope()) {
                Assert.That(sleep.CheckOnProcessExecEventType.SenderTypeCorrect, Is.True);
                Assert.That(sleep.CheckOnProcessExitEventType.SenderTypeCorrect, Is.True);
                if (sleep.CheckOnOutputDataReceivedType.Received)
                    Assert.That(sleep.CheckOnOutputDataReceivedType.SenderTypeCorrect, Is.True);
                if (sleep.CheckOnErrorDataReceivedType.Received)
                    Assert.That(sleep.CheckOnErrorDataReceivedType.SenderTypeCorrect, Is.True);
            }
        }

        [Test]
        public async Task ExceptionInExitEvent()
        {
            SleepProcess sleep = new(GetTimeoutBinary(), "info") {
                RaiseExceptionOnExitEvent = true
            };

            await sleep.ExecuteAsync();

            using (Assert.EnterMultipleScope()) {
                Assert.That(sleep.CheckOnProcessExecEventType.Received, Is.True);
                Assert.That(sleep.CheckOnProcessExitEventType.Received, Is.True);
                Assert.That(
                    sleep.CheckOnOutputDataReceivedType.Received |
                    sleep.CheckOnErrorDataReceivedType.Received, Is.True);
            }

            using (Assert.EnterMultipleScope()) {
                Assert.That(sleep.CheckOnProcessExecEventType.SenderTypeCorrect, Is.True);
                Assert.That(sleep.CheckOnProcessExitEventType.SenderTypeCorrect, Is.True);
                if (sleep.CheckOnOutputDataReceivedType.Received)
                    Assert.That(sleep.CheckOnOutputDataReceivedType.SenderTypeCorrect, Is.True);
                if (sleep.CheckOnErrorDataReceivedType.Received)
                    Assert.That(sleep.CheckOnErrorDataReceivedType.SenderTypeCorrect, Is.True);
            }
        }

        [Test]
        public async Task NonZeroExitCode()
        {
            SleepProcess sleep = new(GetTimeoutBinary());
            await sleep.ExecuteAsync();

            // No arguments results in an error.
            Assert.That(sleep.ExitCode, Is.EqualTo(1));
        }

        [Test]
        public async Task NonZeroExitCodeEvent()
        {
            int exitCode = -1;

            SleepProcess sleep = new(GetTimeoutBinary());
            sleep.ProcessExitEvent += (s, e) => {
                exitCode = e.Result;
            };
            await sleep.ExecuteAsync();

            // No arguments results in an error.
            Assert.That(sleep.ExitCode, Is.EqualTo(1));
            Assert.That(sleep.OverrideExitCode, Is.EqualTo(1));
            Assert.That(exitCode, Is.EqualTo(1));
        }
    }
}
