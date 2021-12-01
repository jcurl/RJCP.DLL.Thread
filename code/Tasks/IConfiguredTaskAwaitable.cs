namespace RJCP.Threading.Tasks
{
    /// <summary>
    /// Provides an awaitable object that allows for configured awaits on <see cref="ITask"/>.
    /// </summary>
    /// <remarks>This type is intended for compiler use only.</remarks>
    public interface IConfiguredTaskAwaitable
    {
        /// <summary>
        /// Creates an awaiter used to await this <see cref="IConfiguredTaskAwaitable"/>.
        /// </summary>
        /// <returns>An awaiter instance.</returns>
        /// <remarks>This method is intended for compiler user rather than use directly in code.</remarks>
        ITaskAwaiter GetAwaiter();
    }
}
