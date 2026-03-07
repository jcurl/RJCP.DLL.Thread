namespace RJCP.Threading.Process
{
    using System;

    internal interface IProcessWorker : IDisposable
    {
        int ExitCode { get; }

        int Id { get; }

        void Start();

        bool Wait(int timeout);

        void Terminate();

        event EventHandler<ConsoleDataEventArgs> OutputDataReceived;

        event EventHandler<ConsoleDataEventArgs> ErrorDataReceived;

        event EventHandler<ProcessExitedEventArgs> ProcessExitEvent;
    }
}
