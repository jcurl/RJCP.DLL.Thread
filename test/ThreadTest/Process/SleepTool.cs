namespace RJCP.Threading.Process
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using RJCP.CodeQuality.NUnitExtensions;

    public class SleepTool : Executable
    {
        protected SleepTool() { }

        protected SleepTool(int maxDegreeOfParallelism) : base(maxDegreeOfParallelism) { }

        /// <summary>
        /// Searches asynchronously for the 'stimeout' tool.
        /// </summary>
        /// <returns>The sleep tool if found and it works as expected.</returns>
        /// <exception cref="InvalidOperationException">The tool 'stimeout' could not be found.</exception>
        public static async Task<SleepTool> CreateAsync()
        {
            SleepTool tool = new();
            await tool.FindExecutableAsync(true);
            return tool;
        }

        /// <summary>
        /// Searches asynchronously for the 'stimeout' tool.
        /// </summary>
        /// <param name="maxDegreeOfParallelism">The maximum degree of parallelism.</param>
        /// <returns>The sleep tool if found and it works as expected.</returns>
        /// <exception cref="InvalidOperationException">The tool 'stimeout' could not be found.</exception>
        public static async Task<SleepTool> CreateAsync(int maxDegreeOfParallelism)
        {
            SleepTool tool = new(maxDegreeOfParallelism);
            await tool.FindExecutableAsync(true);
            return tool;
        }

        /// <summary>
        /// When overridden, provides the name of this executable tool.
        /// </summary>
        /// <value>The name of the executable tool.</value>
        protected override string ToolName { get { return "stimeout"; } }

        /// <summary>
        /// Performs initialization, looking for the tool and checking that it is compatible.
        /// </summary>
        /// <returns>The path to the binary, if it is valid and usable.</returns>
        /// <remarks>
        /// This method should search for the binary and return a string that contains the path to the binary. It should
        /// already check that the path is valid, and possibly run the executable to ensure that it reports the correct
        /// version.
        /// <para>
        /// If the executable cannot be found, or it is deemed unusable, an exception may be be thrown, or the
        /// <see langword="null"/> string can be returned.
        /// </para>
        /// </remarks>
        protected override async Task<string> FindToolAsync()
        {
            string stimeBinary;
            if (IntPtr.Size == 4) {
                stimeBinary = Path.Combine(Deploy.WorkDirectory, "Resources", "stimeout", "x86", "Release", "stimeout.exe");
            } else {
                stimeBinary = Path.Combine(Deploy.WorkDirectory, "Resources", "stimeout", "x64", "Release", "stimeout.exe");
            }

            if (!File.Exists(stimeBinary))
                return null;

            if (await CheckToolAsync(stimeBinary)) return stimeBinary;
            return null;
        }

        private static async Task<bool> CheckToolAsync(string path)
        {
            if (!CheckFileExists(path)) return false;

            RunProcess sleep = await RunProcess.RunAsync(path, new[] { "info" });
            return sleep.ExitCode == 0;
        }

        /// <summary>
        /// Runs the tool with a timeout.
        /// </summary>
        /// <param name="timeout">The timeout to sleep for.</param>
        /// <returns>The task for the sleep tool.</returns>
        public Task SleepAsync(int timeout)
        {
            return RunAsync(timeout.ToString());
        }
    }
}
