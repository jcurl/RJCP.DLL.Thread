namespace RJCP.Threading.Process
{
    using System;

    public partial class RunProcess
    {
        /// <summary>
        /// Command line algorithms.
        /// </summary>
        public static class CommandLine
        {
            /// <summary>
            /// Splits the command line into its consitituents.
            /// </summary>
            /// <param name="arguments">The arguments string that needs to be split.</param>
            /// <returns>The list of arguments</returns>
            /// <exception cref="PlatformNotSupportedException">Only works on Windows and Linux.</exception>
            public static string[] Split(string arguments)
            {
                if (Platform.IsWinNT()) return Windows.SplitCommandLine(arguments);
                if (Platform.IsUnix()) return Linux.SplitCommandLine(arguments);
                throw new PlatformNotSupportedException();
            }

            /// <summary>
            /// Joins the command line elements together that can be passed to start a process.
            /// </summary>
            /// <param name="arguments">The arguments to join.</param>
            /// <returns>The resulting string with all arguments joined.</returns>
            public static string Join(params string[] arguments)
            {
                if (Platform.IsWinNT()) return Windows.JoinCommandLine(arguments);
                if (Platform.IsUnix()) return Linux.JoinCommandLine(arguments);
                throw new PlatformNotSupportedException();
            }
        }
    }
}
