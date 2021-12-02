namespace RJCP.Threading.Tasks
{
    /// <summary>
    /// Provides an awaitable object that allows for configured awaits on <see cref="ITask{TResult}"/>.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <remarks>This type is intended for compiler use only.</remarks>
    public interface IConfiguredTaskAwaitable<out TResult>
    {
        /// <summary>
        /// Creates an awaiter used to await this <see cref="IConfiguredTaskAwaitable{TResult}"/>.
        /// </summary>
        /// <returns>An awaiter instance.</returns>
        /// <remarks>This method is intended for compiler user rather than use directly in code.</remarks>
        ITaskAwaiter<TResult> GetAwaiter();
    }
}
