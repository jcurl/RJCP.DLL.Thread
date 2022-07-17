namespace RJCP.Threading.Tasks
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// Maintains a task group, so that all tasks can be tracked when they're complete.
    /// </summary>
    public class TaskGroup : IEnumerable<Task>
    {
        private readonly object m_GroupLock = new object();
        private readonly HashSet<Task> m_TaskGroup = new HashSet<Task>();
        private readonly TaskCompletionSource<int> m_CompleteSource =
            new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);

        /// <summary>
        /// Registers the task that is added to the list of tasks that should complete.
        /// </summary>
        /// <param name="task">The task that should be added to the list.</param>
        /// <returns>
        /// Is <see langword="true"/> if the task already exists or was added, or <see langword="false"/> otherwise
        /// indicating that all tasks are already complete (and was not added).
        /// </returns>
        public bool RegisterTask(Task task)
        {
            lock (m_GroupLock) {
                if (m_CompleteSource.Task.IsCompleted) return false;

                if (!m_TaskGroup.Contains(task)) {
                    m_TaskGroup.Add(task);
                }
            }

            // We don't await, as we want this to continue running in parallel. We don't need the task, as this method
            // sets the m_CompleteSource when everything is done.
            _ = HandleCompletion(task);
            return true;
        }

        /// <summary>
        /// Registers the task that is added to the list of tasks that should complete..
        /// </summary>
        /// <param name="taskFunc">The task function, that returns a task if we're still running.</param>
        /// <remarks>
        /// Add the task returned by <paramref name="taskFunc"/> if other tasks are still being waited on. This method
        /// avoids a race condition with <see cref="RegisterTask(Task)"/> by not requesting (and potentially starting)
        /// the task if all existing tasks are complete.
        /// </remarks>
        /// <returns>
        /// Is <see langword="true"/> if the task already exists or was added, or <see langword="false"/> otherwise
        /// indicating that all tasks are already complete (and was not added).
        /// </returns>
        public bool RegisterTask(Func<Task> taskFunc)
        {
            Task task;

            lock (m_GroupLock) {
                if (m_CompleteSource.Task.IsCompleted) return false;

                // By getting the task within 'm_GroupLock', we're sure already that we can't be completed at this point
                // in time. Even if a task does complete, the `m_TaskGroup` won't be empty.
                task = taskFunc();
                if (!m_TaskGroup.Contains(task)) {
                    m_TaskGroup.Add(task);
                }
            }

            // We don't await, as we want this to continue running in parallel. We don't need the task, as this method
            // sets the m_CompleteSource when everything is done.
            _ = HandleCompletion(task);
            return true;
        }

        private async Task HandleCompletion(Task task)
        {
            try {
                await task.ConfigureAwait(false);
                lock (m_GroupLock) {
                    MarkCompleted(task, false);
                }
            } catch (OperationCanceledException) {
                lock (m_GroupLock) {
                    MarkCompleted(task, true);
                    m_CompleteSource.TrySetCanceled();
                }
            } catch (Exception ex) {
                lock (m_GroupLock) {
                    MarkCompleted(task, true);
                    m_CompleteSource.TrySetException(ex);
                }
            }

            // We return a Task to quieten some lint tools about fire/forget async methods.
        }

        private void MarkCompleted(Task task, bool exception)
        {
            if (m_CompleteSource.Task.IsCompleted) return;

            // We only remove tasks if we're not yet completed. It might be that a task was cancelled, or an
            // exception was thrown, and the user needs to enumerate through the remaining tasks to cancel
            // them, in which case, we shouldn't be modifying the collection.
            m_TaskGroup.Remove(task);

            // Check if all the tasks are completed and signal the completion source if so.
            if (exception) FaultedTask = task;
            if (m_TaskGroup.Count == 0 && !exception) {
                m_CompleteSource.TrySetResult(0);
            }
        }

        /// <summary>
        /// Gets the faulted task.
        /// </summary>
        /// <value>The faulted task.</value>
        /// <remarks>
        /// This property is <see langword="null"/>, unless a task faulted, in which case this is only the first task
        /// that faulted (assuming all future tasks might fault for similar reasons).
        /// </remarks>
        public Task FaultedTask { get; private set; }

        /// <summary>
        /// Waits for all registered tasks to complete.
        /// </summary>
        /// <remarks>
        /// Tasks can be added while waiting and they're added to the wait list. If a task is added after all previous
        /// tasks have completed, it will <b>not</b> reset the wait list.
        /// </remarks>
        public async Task WaitAll()
        {
            await m_CompleteSource.Task.ConfigureAwait(false);
        }

        private static readonly EmptyTaskList EmptyTasks = new EmptyTaskList();

        private sealed class EmptyTaskList : IEnumerator<Task>
        {
            public Task Current { get { return null; } }

            object IEnumerator.Current { get { return null; } }

            public void Dispose() { /* Nothing to dispose */ }

            public bool MoveNext() { return false; }

            public void Reset() { /* Nothing to reset */ }
        }

        /// <summary>
        /// Returns an enumerator that iterates through the tasks that are registered.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        /// <remarks>
        /// This method is not thread safe, nor is any ordering guaranteed. You should only iterate through
        /// the collection once an exception occurs, or a task has cancelled, to identify the remaining tasks.
        /// If you try to enumerate while running, an empty list is returned.
        /// </remarks>
        public IEnumerator<Task> GetEnumerator()
        {
            lock (m_GroupLock) {
                if (m_CompleteSource.Task.IsCompleted) {
                    return m_TaskGroup.GetEnumerator();
                }

                return EmptyTasks;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
