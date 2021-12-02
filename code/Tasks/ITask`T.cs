namespace RJCP.Threading.Tasks
{
    using System.Runtime.CompilerServices;
    using Runtime.CompilerServices;

    /// <summary>
    /// An async method extension interface to allow covariance in your interfaces.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    [AsyncMethodBuilder(typeof(AsyncITaskMethodBuilder<>))]
    public interface ITask<out TResult> : ITask
    {
        /// <summary>
        /// Gets the result of the <see cref="ITask{TResult}"/>.
        /// </summary>
        /// <value>The result of the <see cref="ITask{TResult}"/>.</value>
        TResult Result { get; }

        /// <summary>
        /// Gets an awaiter used to await this <see cref="ITask{TResult}"/>.
        /// </summary>
        /// <returns>An awaiter instance.</returns>
        /// <remarks>This method is intended for compiler user rather than use directly in code.</remarks>
        new ITaskAwaiter<TResult> GetAwaiter();

        /// <summary>
        /// Configures an awaiter used to await this <see cref="ITask{TResult}"/>.
        /// </summary>
        /// <param name="continueOnCapturedContext">
        /// true to attempt to marshal the continuation back to the original context captured; otherwise, false.
        /// </param>
        /// <returns>An object used to await this task.</returns>
        new IConfiguredTaskAwaitable<TResult> ConfigureAwait(bool continueOnCapturedContext);
    }
}
