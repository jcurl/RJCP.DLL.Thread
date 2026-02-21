namespace RJCP.Threading.Process
{
    using System;

    /// <summary>
    /// Event Arguments when there is console data emitted from a process.
    /// </summary>
    public class ConsoleDataEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConsoleDataEventArgs"/> class.
        /// </summary>
        /// <param name="data">The data that was emitted to the console by the process.</param>
        public ConsoleDataEventArgs(string data)
        {
            Data = data;
        }

        /// <summary>
        /// Gets or sets the data that was emitted to the console by the process.
        /// </summary>
        /// <value>The data that was emitted to the console by the process.</value>
        public string Data { get; set; }
    }
}
