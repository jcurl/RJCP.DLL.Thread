namespace RJCP.Threading.Process
{
    using System;
    using System.Threading;

    public class SleepSimProcess : RunProcess
    {
        private static int SleepSim(RunProcess process, string command, string[] arguments, CancellationToken token)
        {
            SleepSimProcess p = (SleepSimProcess)process;
            if (arguments[0].Equals("init", StringComparison.InvariantCulture)) {
                p.LogStdOut("stimeout version 1.0");
                return 0;
            }

            if (!int.TryParse(arguments[0], out int _)) {
                p.LogStdOut("Invalid time argument");
                return 1;
            }

            // We don't actually sleep for the simulation.
            return 0;
        }

        public SleepSimProcess(string[] arguments) : base(SleepSim, null, null, arguments) { }
    }
}
