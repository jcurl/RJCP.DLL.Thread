namespace RJCP.Threading.Tasks.Covariance
{
    public interface ILineReader<out T> where T : ILine
    {
        ITask<T> GetLineAsync();
    }
}
