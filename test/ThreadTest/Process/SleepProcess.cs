namespace RJCP.Threading.Process
{
    using System;

    public class SleepProcess : RunProcess
    {
        private static readonly string[] InfoArgs = new[] { "info" };

        public SleepProcess(string command)
            : base(command, null, InfoArgs) { }

        public SleepProcess(string command, int time)
            : base(command, null, new[] { time.ToString() }) { }

        public class CheckEvent
        {
            public bool Received;
            public bool SenderTypeCorrect;

            public void Update(bool typeCorrect)
            {
                if (typeCorrect) {
                    if (Received) {
                        SenderTypeCorrect = SenderTypeCorrect && typeCorrect;
                    } else {
                        SenderTypeCorrect = typeCorrect;
                    }
                } else {
                    SenderTypeCorrect = false;
                }
                Received = true;
            }
        }

        public bool RaiseExceptionOnOutputEvent { get; set; }

        public bool RaiseExceptionOnErrorEvent { get; set; }

        public bool RaiseExceptionOnExitEvent { get; set; }

        public CheckEvent CheckOnOutputDataReceivedType { get; set; } = new();

        public CheckEvent CheckOnErrorDataReceivedType { get; set; } = new();

        public CheckEvent CheckOnProcessExitEventType { get; set; } = new();

        protected override void OnOutputDataReceived(object sender, ConsoleDataEventArgs e)
        {
            CheckOnOutputDataReceivedType.Update(sender is RunProcess);
            if (RaiseExceptionOnOutputEvent)
                throw new InvalidOperationException("Raising error");
            base.OnOutputDataReceived(sender, e);
        }

        protected override void OnErrorDataReceived(object sender, ConsoleDataEventArgs e)
        {
            CheckOnErrorDataReceivedType.Update(sender is RunProcess);
            if (RaiseExceptionOnErrorEvent)
                throw new InvalidOperationException("Raising error");
            base.OnErrorDataReceived(sender, e);
        }

        protected override void OnProcessExitEvent(object sender, ProcessExitedEventArgs e)
        {
            CheckOnProcessExitEventType.Update(sender is RunProcess);
            if (RaiseExceptionOnExitEvent)
                throw new InvalidOperationException("Raising error");
            base.OnProcessExitEvent(sender, e);
        }
    }
}
