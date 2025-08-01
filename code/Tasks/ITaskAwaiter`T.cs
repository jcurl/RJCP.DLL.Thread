﻿namespace RJCP.Threading.Tasks
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;

    /// <summary>
    /// Awaiter interface for awaiting an <see cref="ITask{TResult}"/>.
    /// </summary>
    /// <typeparam name="TResult">The type for the result of the task awaiter.</typeparam>
    /// <remarks>This type is intended for compiler use only.</remarks>
    public interface ITaskAwaiter<out TResult> : ICriticalNotifyCompletion
    {
        /// <summary>
        /// Gets whether the task being awaited is completed.
        /// </summary>
        /// <remarks>This property is intended for compiler user rather than use directly in code.</remarks>
        /// <exception cref="NullReferenceException">The awaiter was not properly initialized.</exception>
        bool IsCompleted { get; }

        /// <summary>
        /// Ends the await on the completed <see cref="ITask{TResult}" />.
        /// </summary>
        /// <returns>The resultant object of type <typeparamref name="TResult"/>.</returns>
        /// <exception cref="NullReferenceException">The awaiter was not properly initialized.</exception>
        /// <exception cref="TaskCanceledException">The task was canceled.</exception>
        /// <exception cref="Exception">The task completed in a Faulted state.</exception>
        TResult GetResult();

        // Inherited interfaces provide OnCompleted and UnsafeOnCompleted
    }
}
