namespace RJCP.Threading.Process
{
    using System.Threading.Tasks;

    public class SleepToolMock : SleepTool
    {
        protected SleepToolMock() { }

        /// <summary>
        /// Returns immediately a simulated tool for sleeping.
        /// </summary>
        /// <returns>The sleep tool if found and it works as expected.</returns>
        public static async Task<SleepToolMock> CreateSimAsync()
        {
            SleepToolMock tool = new();
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
            return string.Empty;
        }

        /// <summary>
        /// Creates the process instance to execute.
        /// </summary>
        /// <param name="workDir">The working directory to execute from.</param>
        /// <param name="arguments">The arguments to give to the binary.</param>
        /// <returns>An instance of a run process that has not yet been started.</returns>
        /// <remarks>Override this method to provide your own simulated tool.</remarks>
        protected override RunProcess CreateRunProcess(string workDir, params string[] arguments)
        {
            return new SleepSimProcess(arguments);
        }
    }
}
