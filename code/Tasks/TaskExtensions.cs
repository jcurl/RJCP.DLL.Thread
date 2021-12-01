namespace RJCP.Threading.Tasks
{
    using System.Threading.Tasks;

    /// <summary>
    /// Extensions for converting Tasks.
    /// </summary>
    public static class TaskExtensions
    {
        /// <summary>
        /// Gets the <see cref="Task"/> as an <see cref="ITask"/> object.
        /// </summary>
        /// <param name="task">The task to return.</param>
        /// <returns>An <see cref="ITask"/> that can be awaited on.</returns>
        public static ITask AsITask(this Task task)
        {
            return new Wrapper.TaskWrapper(task);
        }
    }
}
