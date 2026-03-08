namespace RJCP.Threading.Process
{
    using System;

    /// <summary>
    /// Event when a process is exited.
    /// </summary>
    public class ProcessExitedEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessExitedEventArgs"/> class.
        /// </summary>
        /// <param name="result">The result code of the process when it exited.</param>
        public ProcessExitedEventArgs(int result)
        {
            Result = result;
        }

        /// <summary>
        /// Gets the result code of the process when it exited.
        /// </summary>
        /// <value>The result code of the process when it exited.</value>
        public int Result { get; }
    }
}
