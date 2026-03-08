namespace RJCP.Threading.Process
{
    using System;

    /// <summary>
    /// Event before process is started.
    /// </summary>
    public class ProcessExecEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessExecEventArgs"/> class.
        /// </summary>
        /// <param name="command">The command that is about to be executed.</param>
        /// <param name="arguments">The argument list for the command.</param>
        /// <param name="workdir">The working directory.</param>
        public ProcessExecEventArgs(string command, string[] arguments, string workdir)
        {
            Command = command ?? string.Empty;
            WorkDir = workdir ?? string.Empty;
            Arguments = arguments ?? Array.Empty<string>();
        }

        /// <summary>
        /// Gets the command that is about to be executed.
        /// </summary>
        /// <value>The command that is about to be executed.</value>
        public string Command { get; }

        /// <summary>
        /// Gets the argument list for the command.
        /// </summary>
        /// <value>The argument list for the command.</value>
        public string[] Arguments { get; }

        /// <summary>
        /// Gets the work dir.
        /// </summary>
        /// <value>The work dir.</value>
        public string WorkDir { get; }
    }
}
