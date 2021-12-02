namespace RJCP.Threading.Tasks.Covariance
{
    using System.Threading.Tasks;

    public class LineExtendedReader : ILineReader<LineExtended>
    {
        public async ITask<LineExtended> GetLineAsync()
        {
            await Task.Delay(1);
            return new LineExtended();
        }
    }
}
