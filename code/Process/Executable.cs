namespace RJCP.Threading.Process
{
    using System;
    using System.Collections;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using Concurrency;

    /// <summary>
    /// An abstract class that others can implement for providing an interface to execute a specific tool.
    /// </summary>
    /// <remarks>
    /// <para>
    /// An <see cref="Executable"/> simplifies the process of creating a class per executable process, which underneath
    /// calls <see cref="RunProcess"/>. The abstract class provides boiler plate code, so that an implementor must only
    /// provide a mechanism in finding the most appropriate binary on the file system, and reporting the binary path to
    /// this base class.
    /// </para>
    /// <para>
    /// For example, the user might want to call the <c>git</c> utility. There would be a class <c>Git</c> that derives
    /// from the <see cref="Executable"/> and implements the method <see cref="FindToolAsync"/>. This method can then
    /// search paths for the tool, and itself call <see cref="RunProcess"/> to ensure the version is supported. When the
    /// correct tool is found, a path to the binary that is to be used is returned by the implementation of
    /// <see cref="FindToolAsync"/>. This base class then abstracts away from the user the need to call
    /// <see cref="RunProcess"/>. The <c>Git</c> class can also implement specific methods that executes the commands
    /// necessary, e.g. for creating a branch. The goal is that the user calls methods on the <c>Git</c> class, and the
    /// <c>Git</c> class abstracts away calling the base methods to execute the binary found with
    /// <see cref="FindToolAsync"/>.
    /// </para>
    /// <para>
    /// For batched builds, it is possible to use the constructor <see cref="Executable(int)"/> that can limit the
    /// maximum number of instances for this object to be running. This is useful if the tool is simply run with as many
    /// tasks as possible, but it might be useful to restrict it to not run more than the number of cores on the system,
    /// or if I/O bound, to fewer number of instances, like only 2. Naturally, this only applies to the instance
    /// created. e.g. a single <c>Git</c> class is made, and no matter what the command is the user gives, we can limit
    /// the maximum number of calls to <c>Git</c> so that there is some reasonable responsiveness.
    /// </para>
    /// <para>
    /// Generally, only a single <see cref="Executable"/> should be created for the instance of a program, as the binary
    /// path remains the same for all. This reduces overhead of having to search for the same binary multiople times.
    /// Following this example, the application would have a single static implementation of the <c>Git</c> class
    /// derived from <see cref="Executable"/>, without having to instantiate and call <see cref="RunProcess"/>
    /// themselves.
    /// </para>
    /// <para>When creating your own tool deriving from <see cref="Executable"/>, follow these guidelines:</para>
    /// <list type="bullet">
    /// <item>
    /// Make the default constructor <see langword="protected"/>. This helps later for testing so that a mock can be
    /// created.
    /// </item>
    /// <item>
    /// Provide an asynchronous static <c>CreateAsync()</c> method that constructs via the <see langword="protected"/>
    /// constructor, and <see langword="await"/> s on the <see cref="FindExecutableAsync()"/> method. This allows the
    /// object to be initialised before it is returned.
    /// </item>
    /// <item>
    /// Provide an implementation of <see cref="FindToolAsync()"/> that uses the methods in this class to search the
    /// path for the tool. If the tool must be checked for compatibility, execute <see cref="RunProcess"/> from within
    /// <see cref="FindToolAsync"/> and check the output. If a suitable tool is not found, return
    /// <see langword="null"/>.
    /// </item>
    /// <item>
    /// If the tool isn't found, the default behaviour of <see cref="FindExecutableAsync()"/> is to call
    /// <see cref="ErrorToolNotAvailable"/>. This method raises an exception <see cref="InvalidOperationException"/>
    /// using the tool name from the property <see cref="ToolName"/> if <see cref="FindExecutableAsync(bool)"/> with the
    /// throw on error option is provided. Override that property for the name of the tool, instead of the
    /// <see cref="object.ToString()"/> method.
    /// </item>
    /// <item>
    /// For each invocation type, create your own method that calls the protected member
    /// <see cref="RunAsync(string[])"/>, <see cref="RunFromAsync(string, string[])"/>, or
    /// <see cref="ExecuteProcessAsync(string[])"/> and similar overloads. Don't instantiate the
    /// <see cref="RunProcess"/> yourself in this case. That should only be done from within
    /// <see cref="FindToolAsync"/>.
    /// </item>
    /// </list>
    /// <para>To implement a mock for your tool, you should implement the following:</para>
    /// <list type="bullet">
    /// <item>
    /// Create a simulation run of your tool deriving from <see cref="RunProcess"/>. The derived class might be called
    /// <c>GitSim</c>, derives from <see cref="RunProcess"/>. It uses the constructor that calls a delegate to simulate
    /// the process, without ever having to use Operating System APIs. See
    /// <see cref="RunProcess(RunProcess.SimAction, string, string, string[])"/> for an example of how to do this.
    /// </item>
    /// <item>
    /// <para>
    /// Implement a <c>GitToolMock</c> that derives from the tool <c>GitTool</c> (which derives from
    /// <see cref="Executable"/>). The method <see cref="FindExecutableAsync()"/> would return
    /// <see cref="string.Empty"/> (but not <see langword="null"/>), to indicate that the tool is found. The method
    /// <see cref="CreateRunProcess"/> must be overridden, so that the simulated process, e.g. <c>GitSim</c>, is
    /// instantiated, instead of the base implementation, which instantiates itself for you a <see cref="RunProcess"/>,
    /// that executes the real process.
    /// </para>
    /// <para>
    /// From here it is obvious now why only <see cref="FindToolAsync"/> should instantiate <see cref="RunProcess"/>.
    /// The mock never instantiates any <see cref="RunProcess"/>, and just returns an empty string indicating the
    /// simulation <see cref="IsAvailable"/>. Then the overridden method <see cref="CreateRunProcess"/> creates the
    /// simulation, completely removing the need to call the Operating System to start a process, providing the mock.
    /// </para>
    /// </item>
    /// <item>
    /// In the real tool <c>GitTool</c> and the mock <c>GitToolMock</c>, calls directly to any static methods must be
    /// avoided that mocking works as expected.
    /// </item>
    /// </list>
    /// <para>
    /// The main application has a singleton for <c>GitTool</c> which derives from <see cref="Executable"/>. The main
    /// program would instantiate the real class <c>GitTool</c>, while a test program would inject its own
    /// <c>GitToolMock</c> instead. The main program wouldn't know the difference, because the handling of the results
    /// in <c>GitTool</c> (e.g. calling <tt>git merge</tt>) are the same, it doesn't know if the result came from
    /// <c>Git</c> or <c>GitSim</c>.
    /// </para>
    /// </remarks>
    public abstract class Executable
    {
        private string m_BinaryPath;
        private bool m_Initialized;
        private readonly AsyncSemaphore m_Semaphore;

        /// <summary>
        /// Initializes a new instance of the <see cref="Executable"/> class.
        /// </summary>
        /// <remarks>
        /// There is no maximum degree of parallelism, so every time a binary is started, it will run.
        /// </remarks>
        protected Executable() : this(0) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Executable"/> class.
        /// </summary>
        /// <param name="maxDegreeOfParallelism">The maximum degree of parallelism.</param>
        /// <remarks>
        /// Setting the maximum degree of parallelism will limit the number of concurrent instances of this particular
        /// executable at once. This could be for example, to limit the number of accesses to disk.
        /// </remarks>
        protected Executable(int maxDegreeOfParallelism)
        {
            // For information on how this works, see
            // https://blogs.msdn.microsoft.com/fkaduk/2018/09/02/multiple-ways-how-to-limit-parallel-tasks-processing/
            if (maxDegreeOfParallelism > 0) {
                m_Semaphore = new AsyncSemaphore(maxDegreeOfParallelism);
            }
        }

        /// <summary>
        /// Execute the binary with the given arguments.
        /// </summary>
        /// <param name="arguments">The arguments to give to the binary.</param>
        /// <returns>The <see cref="RunProcess"/> for obtaining results of the binary.</returns>
        protected async Task<RunProcess> RunAsync(params string[] arguments)
        {
            await FindExecutableAsync(true).ConfigureAwait(false);
            try {
                if (m_Semaphore is not null) await m_Semaphore.WaitAsync().ConfigureAwait(false);
                return await ExecuteProcessAsync(arguments).ConfigureAwait(false);
            } finally {
                if (m_Semaphore is not null) m_Semaphore.Release();
            }
        }

        /// <summary>
        /// Execute the binary with the given arguments.
        /// </summary>
        /// <param name="arguments">The arguments to give to the binary.</param>
        /// <param name="token">The cancellation token.</param>
        /// <returns>The <see cref="RunProcess"/> for obtaining results of the binary.</returns>
        protected async Task<RunProcess> RunAsync(string[] arguments, CancellationToken token)
        {
            await FindExecutableAsync(true).ConfigureAwait(false);
            try {
                if (m_Semaphore is not null) await m_Semaphore.WaitAsync().ConfigureAwait(false);
                return await ExecuteProcessAsync(arguments, token).ConfigureAwait(false);
            } finally {
                if (m_Semaphore is not null) m_Semaphore.Release();
            }
        }

        /// <summary>
        /// Execute the binary with the given arguments from a specified working directory.
        /// </summary>
        /// <param name="workDir">The working directory to execute from.</param>
        /// <param name="arguments">The arguments to give to the binary.</param>
        /// <returns>The <see cref="RunProcess"/> for obtaining results of the binary.</returns>
        protected async Task<RunProcess> RunFromAsync(string workDir, params string[] arguments)
        {
            await FindExecutableAsync(true).ConfigureAwait(false);
            try {
                if (m_Semaphore is not null) await m_Semaphore.WaitAsync().ConfigureAwait(false);
                return await ExecuteProcessAsync(workDir, arguments).ConfigureAwait(false);
            } finally {
                if (m_Semaphore is not null) m_Semaphore.Release();
            }
        }

        /// <summary>
        /// Execute the binary with the given arguments from a specified working directory.
        /// </summary>
        /// <param name="workDir">The working directory to execute from.</param>
        /// <param name="arguments">The arguments to give to the binary.</param>
        /// <param name="token">The cancellation token.</param>
        /// <returns>The <see cref="RunProcess"/> for obtaining results of the binary.</returns>
        protected async Task<RunProcess> RunFromAsync(string workDir, string[] arguments, CancellationToken token)
        {
            await FindExecutableAsync(true).ConfigureAwait(false);
            try {
                if (m_Semaphore is not null) await m_Semaphore.WaitAsync().ConfigureAwait(false);
                return await ExecuteProcessAsync(workDir, arguments, token).ConfigureAwait(false);
            } finally {
                if (m_Semaphore is not null) m_Semaphore.Release();
            }
        }

        /// <summary>
        /// Executes the process asynchronously.
        /// </summary>
        /// <param name="arguments">The arguments to give to the binary.</param>
        /// <returns>The <see cref="RunProcess"/> for obtaining results of the binary.</returns>
        /// <remarks>
        /// This method allows customisation on how the process is executed.
        /// <para>
        /// This method call ignores the degree of parallelism that may have been called to the constructor
        /// <see cref="Executable(int)"/>.
        /// </para>
        /// </remarks>
        protected Task<RunProcess> ExecuteProcessAsync(params string[] arguments)
        {
            return ExecuteProcessAsync(null, arguments);
        }

        /// <summary>
        /// Executes the process asynchronously.
        /// </summary>
        /// <param name="arguments">The arguments to give to the binary.</param>
        /// <param name="token">The cancellation token.</param>
        /// <returns>The <see cref="RunProcess"/> for obtaining results of the binary.</returns>
        /// <remarks>This method allows customisation on how the process is executed.
        /// <para>
        /// This method call ignores the degree of parallelism that may have been called to the constructor
        /// <see cref="Executable(int)"/>.
        /// </para>
        /// </remarks>
        protected Task<RunProcess> ExecuteProcessAsync(string[] arguments, CancellationToken token)
        {
            return ExecuteProcessAsync(null, arguments, token);
        }

        /// <summary>
        /// Executes the process asynchronously.
        /// </summary>
        /// <param name="workDir">The working directory to execute from.</param>
        /// <param name="arguments">The arguments to give to the binary.</param>
        /// <returns>The <see cref="RunProcess"/> for obtaining results of the binary.</returns>
        /// <remarks>This method allows customisation on how the process is executed.
        /// <para>
        /// This method call ignores the degree of parallelism that may have been called to the constructor
        /// <see cref="Executable(int)"/>.
        /// </para>
        /// </remarks>
        protected async Task<RunProcess> ExecuteProcessAsync(string workDir, string[] arguments)
        {
            RunProcess process = CreateRunProcess(workDir, arguments);
            await process.ExecuteAsync().ConfigureAwait(false);
            return process;
        }

        /// <summary>
        /// Executes the process asynchronously.
        /// </summary>
        /// <param name="workDir">The working directory to execute from.</param>
        /// <param name="arguments">The arguments to give to the binary.</param>
        /// <param name="token">The cancellation token.</param>
        /// <returns>The <see cref="RunProcess"/> for obtaining results of the binary.</returns>
        /// <remarks>This method allows customisation on how the process is executed
        /// <para>
        /// This method call ignores the degree of parallelism that may have been called to the constructor
        /// <see cref="Executable(int)"/>.
        /// </para>
        /// .</remarks>
        protected async Task<RunProcess> ExecuteProcessAsync(string workDir, string[] arguments, CancellationToken token)
        {
            RunProcess process = CreateRunProcess(workDir, arguments);
            await process.ExecuteAsync(false, token).ConfigureAwait(false);
            return process;
        }

        /// <summary>
        /// Creates the process instance to execute.
        /// </summary>
        /// <param name="workDir">The working directory to execute from.</param>
        /// <param name="arguments">The arguments to give to the binary.</param>
        /// <returns>An instance of a run process that has not yet been started.</returns>
        /// <remarks>
        /// Override this method to provide your own simulated tool.
        /// </remarks>
        protected virtual RunProcess CreateRunProcess(string workDir, string[] arguments)
        {
            return new(m_BinaryPath, workDir, arguments);
        }

        /// <summary>
        /// Search for the binary to be executed and check that it is valid.
        /// </summary>
        /// <returns>Returns the path to the binary, or <see langword="null"/> if no binary path is found.</returns>
        /// <remarks>
        /// This method should not be used. It is recommended to use <see cref="FindExecutable()"/> within an
        /// asynchronous factory method to find the executable. For example, if you have a class <c>Git</c> that derives
        /// from <see cref="Executable"/>, create a static asynchronous factory method <c>Git.CreateAsync()</c> that
        /// returns the <c>Git</c> object. The factory method instantiates the <c>Git</c> object, and then calls
        /// <see cref="FindExecutableAsync()"/> instead, so the object is initialised before giving to the user. Make
        /// the constructor of <c>Git</c> private so that users can't instantiate it. This lets the user of the
        /// <c>Git</c> know that creating the object may take time, rather than doing unexpected behaviour in the
        /// constructor itself.
        /// </remarks>
        protected string FindExecutable()
        {
            Task<string> task = FindExecutableAsync();
            task.Wait();
            return task.Result;
        }

        /// <summary>
        /// Search for the binary to be executed and check that it is valid.
        /// </summary>
        /// <returns>Returns the path to the binary, or <see langword="null"/> if no binary path is found.</returns>
        protected Task<string> FindExecutableAsync()
        {
            return FindExecutableAsync(false);
        }

        /// <summary>
        /// Search for the binary to be executed and check that it is valid.
        /// </summary>
        /// <param name="throwOnError">
        /// Set to <see langword="true"/> if an exception should be thrown if the tool can't be found, instead of
        /// returning <see langword="null"/>.
        /// </param>
        /// <returns>Returns the path to the binary, or <see langword="null"/> if no binary path is found.</returns>
        /// <exception cref="InvalidOperationException">The tool cannot be found.</exception>
        protected async Task<string> FindExecutableAsync(bool throwOnError)
        {
            if (m_Initialized) {
                if (throwOnError && m_BinaryPath is null) {
                    ErrorToolNotAvailable();
                    return null;
                }
                return m_BinaryPath;
            }

            await InitializeAsync().ConfigureAwait(false);
            if (throwOnError && m_BinaryPath is null) {
                ErrorToolNotAvailable();
                return null;
            }
            return m_BinaryPath;
        }

        /// <summary>
        /// Gets a value indicating whether this instance is available.
        /// </summary>
        /// <value>
        /// Returns <see langword="true"/> if this instance is available; otherwise, <see langword="false"/> if not
        /// available, or because <see cref="FindExecutableAsync()"/> has not yet been run.
        /// </value>
        /// <exception cref="InvalidOperationException">
        /// A call must be made prior to <see cref="FindExecutableAsync()"/>.
        /// </exception>
        public bool IsAvailable
        {
            get
            {
                return m_Initialized && m_BinaryPath is not null;
            }
        }

        /// <summary>
        /// Gets the path to the executable binary.
        /// </summary>
        /// <value>
        /// The path to the executable binary. If the tool isn't avaialble (the call to
        /// <see cref="FindExecutableAsync()"/> failed to find the right tool), or <see cref="FindExecutableAsync()"/>
        /// has not been called, the path returned is <see langword="null"/>.
        /// </value>
        public string BinaryPath
        {
            get
            {
                return m_Initialized ? m_BinaryPath : null;
            }
        }

        private async Task InitializeAsync()
        {
            try {
                m_BinaryPath = await FindToolAsync().ConfigureAwait(false);
            } catch (Exception) {
                // An exception was raised when initializing. We don't know what this exception is, except that it
                // can't be initialized.
                m_BinaryPath = null;
            } finally {
                m_Initialized = true;
            }
        }

        /// <summary>
        /// Notifies the user that the tool was not found.
        /// </summary>
        /// <remarks>
        /// When overridden, it should throw the exception <see cref="InvalidOperationException"/> with a localised
        /// string to the user. The default string in the exception is the name of the class (so override the
        /// <see cref="ToolName"/> property to specify the name of the tool (which is by default, the implementation of
        /// the <see cref="object.ToString()"/> method.
        /// </remarks>
        protected virtual void ErrorToolNotAvailable()
        {
            throw new InvalidOperationException(string.Format(Resources.Messages.Executable_ToolNotFound, ToolName));
        }

        /// <summary>
        /// When overridden, provides the name of this executable tool.
        /// </summary>
        /// <value>
        /// The name of the executable tool. The default implementation is to call <see cref="object.ToString()"/>.
        /// </value>
        protected virtual string ToolName
        {
            get { return ToString(); }
        }

        /// <summary>
        /// Performs initialization, looking for the tool and checking that it is compatible. If it is not compatible
        /// </summary>
        /// <returns>The path to the binary, if it is valid and usable.</returns>
        /// <remarks>
        /// This method should search for the binary and return a string that contains the path to the binary. It
        /// should already check that the path is valid, and possibly run the executable to ensure that it reports the
        /// correct version.
        /// <para>
        /// If the executable cannot be found, or it is deemed unusable, an exception may be be thrown, or the
        /// <see langword="null"/> string can be returned.
        /// </para>
        /// <para>When testing the binary, use the <see cref="RunProcess"/> static methods direct.</para>
        /// </remarks>
        protected abstract Task<string> FindToolAsync();

        private static readonly ConcurrentDictionary<string, AsyncValue<bool>> s_Paths =
            new();

        private sealed class FilePathEnumerable : IEnumerable<string>
        {
            private readonly string m_Binary;

            public FilePathEnumerable(string binary)
            {
                m_Binary = binary;
            }

            public IEnumerator<string> GetEnumerator()
            {
                return new FileEnumerator(this);
            }

            IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

            private sealed class FileEnumerator : IEnumerator<string>
            {
                private int m_PathIndex = -1;
                private readonly string[] m_Paths;

                public FileEnumerator(FilePathEnumerable parent)
                {
                    string pathVar = Environment.GetEnvironmentVariable("PATH");
                    if (string.IsNullOrWhiteSpace(pathVar)) {
                        m_Paths = Array.Empty<string>();
                    } else {
                        string[] paths = pathVar.Split(Path.PathSeparator);
                        m_Paths = new string[paths.Length];
                        for (int i = 0; i < paths.Length; i++) {
                            m_Paths[i] = Path.GetFullPath(Path.Combine(paths[i], parent.m_Binary));
                        }
                    }
                }

                public string Current
                {
                    get
                    {
                        if (m_PathIndex < 0 || m_PathIndex >= m_Paths.Length)
                            throw new InvalidOperationException("Enumeration out of bounds");
                        return m_Paths[m_PathIndex];
                    }
                }

                object IEnumerator.Current { get { return Current; } }

                public bool MoveNext()
                {
                    while (true) {
                        if (m_PathIndex + 1 == m_Paths.Length) return false;
                        m_PathIndex++;

                        if (CheckFileExists(m_Paths[m_PathIndex])) return true;
                    }
                }

                public void Reset()
                {
                    m_PathIndex = -1;
                }

                public void Dispose()
                {
                    // Nothing to dispose
                }
            }
        }

        /// <summary>
        /// Gets an enumerable that can be used to check for files that exist.
        /// </summary>
        /// <param name="binary">The binary file name that is being sought for.</param>
        /// <returns>A string enumerable of full paths from the PATH environment variable where the file exists.</returns>
        /// <remarks>This method is useful to call from within <see cref="FindToolAsync"/>.</remarks>
        protected static IEnumerable<string> FindFiles(string binary)
        {
            return new FilePathEnumerable(binary);
        }

        /// <summary>
        /// Check if the path to a binary executable exists.
        /// </summary>
        /// <param name="path">The path to the binary executable to check.</param>
        /// <returns>Returns <see langword="true"/> if the file exists, <see langword="false"/> otherwise.</returns>
        /// <remarks>
        /// Paths that have been searched (for this instance of the class) are cached statically. The assumption is that
        /// the filesystem or the variables where paths are searched for do not change.
        /// <para>This method is useful to call from within <see cref="FindToolAsync"/>.</para>
        /// </remarks>
        protected static async Task<bool> CheckFileExistsAsync(string path)
        {
            path = Path.GetFullPath(path);
            AsyncValue<bool> pathFound;

            // See if we've already started searching for this path. The loop might be that another thread is also
            // searching for the path, and so we want the same instance in both that we don't search twice in parallel.
            while (true) {
                if (s_Paths.TryGetValue(path, out pathFound)) break;

                pathFound = new AsyncValue<bool>();
                if (s_Paths.TryAdd(path, pathFound)) break;
            }

            // If we're already looking for this path, it just waits, else it will run on a background thread to check
            // for the existence of the file.
            return await pathFound.GetSetAsync(() => {
                return Task.Run(() => {
                    if (Directory.Exists(path)) return false;
                    return File.Exists(path);
                });
            }).ConfigureAwait(false);
        }

        /// <summary>
        /// Check if the path to a binary executable exists.
        /// </summary>
        /// <param name="path">The path to the binary executable to check.</param>
        /// <returns>Returns <see langword="true"/> if the file exists, <see langword="false"/> otherwise.</returns>
        /// <remarks>
        /// Paths that have been searched (for this instance of the class) are cached.
        /// <para>This method is useful to call from within <see cref="FindToolAsync"/>.</para>
        /// </remarks>
        protected static bool CheckFileExists(string path)
        {
            AsyncValue<bool> pathFound;

            // See if we've already started searching for this path. The loop might be that another thread is also
            // searching for the path, and so we want the same instance in both that we don't search twice in parallel.
            while (true) {
                if (s_Paths.TryGetValue(path, out pathFound)) break;

                pathFound = new AsyncValue<bool>();
                if (s_Paths.TryAdd(path, pathFound)) break;
            }

            return pathFound.GetSet(() => {
                if (Directory.Exists(path)) return false;
                return File.Exists(path);
            });
        }
    }
}
