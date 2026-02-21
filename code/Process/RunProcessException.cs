namespace RJCP.Threading.Process
{
    using System;
    using System.Collections.Generic;
#if NETFRAMEWORK
    using System.Runtime.Serialization;
    using System.Security.Permissions;
#endif

#if NETFRAMEWORK
    [Serializable]
#endif
    /// <summary>
    /// An exception when a process cannot be executed properly.
    /// </summary>
    public class RunProcessException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RunProcessException"/> class.
        /// </summary>
        public RunProcessException()
            : base() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="RunProcessException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public RunProcessException(string message)
            : base(message) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="RunProcessException"/> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">
        /// The exception that is the cause of the current exception, or a <see langword="null"/> reference, if no inner
        /// exception is specified.
        /// </param>
        public RunProcessException(string message, Exception innerException)
            : base(message, innerException) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="RunProcessException"/> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="process">The process that caused the exception.</param>
        public RunProcessException(string message, RunProcess process)
            : base(message)
        {
            Initialize(process);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RunProcessException"/> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="process">The process that caused the exception.</param>
        /// <param name="innerException">
        /// The exception that is the cause of the current exception, or a <see langword="null"/> reference, if no inner
        /// exception is specified.
        /// </param>
        public RunProcessException(string message, RunProcess process, Exception innerException)
            : base(message, innerException)
        {
            Initialize(process);
        }

#if NETFRAMEWORK
        protected RunProcessException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            // Deserialize our new property
            ExitCode = info.GetInt32(nameof(ExitCode));
            Command = info.GetString(nameof(Command));
            WorkingDirectory = info.GetString(nameof(WorkingDirectory));

            // We only record the last line
            StdOut = DeserializeList(info, nameof(StdOut));
            StdErr = DeserializeList(info, nameof(StdErr));
        }
#endif

        private void Initialize()
        {
            Command = string.Empty;
            WorkingDirectory = string.Empty;
            StdOut = Array.Empty<string>();
            StdErr = Array.Empty<string>();
        }

        private void Initialize(RunProcess process)
        {
            if (process is null) {
                Initialize();
                return;
            }

            ExitCode = process.ExitCode;
            Command = process.Command ?? string.Empty;
            WorkingDirectory = process.WorkingDirectory ?? string.Empty;
            StdOut = process.StdOut;
            StdErr = process.StdErr;
        }

        /// <summary>
        /// Gets the exit code of the process.
        /// </summary>
        /// <value>The exit code of the process.</value>
        public int ExitCode { get; private set; }

        /// <summary>
        /// Gets the command that was executed.
        /// </summary>
        /// <value>The command that was executed.</value>
        public string Command { get; private set; }

        /// <summary>
        /// Gets the working directory of the process.
        /// </summary>
        /// <value>The working directory of the process.</value>
        public string WorkingDirectory { get; private set; }

        /// <summary>
        /// Gets the standard output.
        /// </summary>
        /// <value>The standard output.</value>
        public IReadOnlyList<string> StdOut { get; private set; }

        /// <summary>
        /// Gets the standard error.
        /// </summary>
        /// <value>The standard error.</value>
        public IReadOnlyList<string> StdErr { get; private set; }

#if NETFRAMEWORK
        /// <summary>
        /// Gets the object data.
        /// </summary>
        /// <param name="info">The information.</param>
        /// <param name="context">The context.</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            // Serialize our new property, call the base
            info.AddValue(nameof(ExitCode), ExitCode);
            info.AddValue(nameof(Command), Command);
            info.AddValue(nameof(WorkingDirectory), WorkingDirectory);
            SerializeList(info, nameof(StdOut), StdOut);
            SerializeList(info, nameof(StdErr), StdErr);
            base.GetObjectData(info, context);
        }

        private static IReadOnlyList<string> DeserializeList(SerializationInfo info, string name)
        {
            string line = info.GetString(name);
            if (string.IsNullOrWhiteSpace(line))
                return Array.Empty<string>();

            return new List<string>() { line };
        }

        private static void SerializeList(SerializationInfo info, string name, IReadOnlyList<string> list)
        {
            if (list.Count == 0) {
                info.AddValue(name, string.Empty);
                return;
            }

            info.AddValue(name, list[list.Count - 1]);
        }
#endif
    }
}
