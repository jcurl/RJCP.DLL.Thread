namespace RJCP.Threading.Tasks.Covariance
{
    using System.Threading.Tasks;

    public class LineReader : ILineReader<Line>
    {
        public async ITask<Line> GetLineAsync()
        {
            await Task.Delay(1);
            return new Line();
        }
    }
}
