namespace RJCP.Threading.Tasks
{
    using System.Runtime.CompilerServices;
    using Runtime.CompilerServices;

    /// <summary>
    /// An async method extension interface to allow covariance in your interfaces.
    /// </summary>
    [AsyncMethodBuilder(typeof(AsyncITaskMethodBuilder))]
    public interface ITask
    {
        /// <summary>
        /// Gets an awaiter used to await this <see cref="ITask"/>.
        /// </summary>
        /// <returns>An awaiter instance.</returns>
        /// <remarks>This method is intended for compiler user rather than use directly in code.</remarks>
        ITaskAwaiter GetAwaiter();

        /// <summary>
        /// Configures an awaiter used to await this <see cref="ITask"/>.
        /// </summary>
        /// <param name="continueOnCapturedContext">
        /// true to attempt to marshal the continuation back to the original context captured; otherwise, false.
        /// </param>
        /// <returns>An object used to await this task.</returns>
        IConfiguredTaskAwaitable ConfigureAwait(bool continueOnCapturedContext);
    }
}
