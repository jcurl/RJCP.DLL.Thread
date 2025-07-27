namespace RJCP.Runtime.CompilerServices
{
    using System;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading.Tasks;
    using Threading.Tasks;

    // This code mostly comes from reference sources AsyncMethodBuilder.cs

    /// <summary>
    /// Provides a builder for asynchronous methods that return <see cref="ITask{TResult}"/>. This type is intended for
    /// compiler use only.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <remarks>
    /// AsyncTaskMethodBuilder is a value type, and thus it is copied by value. Prior to being copied, one of its Task,
    /// SetResult, or SetException members must be accessed, or else the copies may end up building distinct Task
    /// instances.
    /// </remarks>
    [StructLayout(LayoutKind.Auto)]
    public struct AsyncITaskMethodBuilder<TResult>
    {
        // This struct must be public. Else can't `async` on an `ITask`.

        // This must not be marked `readonly` because it is mutable. If marked `readonly`, the first `await` silently
        // freezes everything.
        private AsyncTaskMethodBuilder<TResult> m_Builder;

        AsyncITaskMethodBuilder(AsyncTaskMethodBuilder<TResult> builder) : this()
        {
            m_Builder = builder;
        }

        /// <summary>
        /// Initializes a new <see cref="AsyncITaskMethodBuilder"/>.
        /// </summary>
        /// <returns>The initialized <see cref="AsyncITaskMethodBuilder"/>.</returns>
        public static AsyncITaskMethodBuilder<TResult> Create()
        {
            return new AsyncITaskMethodBuilder<TResult>(AsyncTaskMethodBuilder<TResult>.Create());
        }

        /// <summary>
        /// Gets the <see cref="ITask{TResult}"/> for this builder.
        /// </summary>
        /// <returns>
        /// The <see cref="ITask{TResult}"/> representing the builder's asynchronous operation.
        /// </returns>
        /// <exception cref="InvalidOperationException">The builder is not initialized.</exception>
        public ITask<TResult> Task { get { return m_Builder.Task.AsITask<TResult>(); } }

        /// <summary>
        /// Initiates the builder's execution with the associated state machine.
        /// </summary>
        /// <typeparam name="TStateMachine">Specifies the type of the state machine.</typeparam>
        /// <param name="stateMachine">The state machine instance, passed by reference.</param>
        [DebuggerStepThrough]
        public void Start<TStateMachine>(ref TStateMachine stateMachine) where TStateMachine : IAsyncStateMachine
        {
            m_Builder.Start(ref stateMachine);
        }

        /// <summary>
        /// Associates the builder with the state machine it represents.
        /// </summary>
        /// <param name="stateMachine">The heap-allocated state machine object.</param>
        /// <exception cref="ArgumentNullException">
        /// The <paramref name="stateMachine"/> argument was null (Nothing in Visual Basic).
        /// </exception>
        /// <exception cref="InvalidOperationException">The builder is incorrectly initialized.</exception>
        public void SetStateMachine(IAsyncStateMachine stateMachine)
        {
            m_Builder.SetStateMachine(stateMachine); // argument validation handled by AsyncMethodBuilderCore
        }

        /// <summary>
        /// Schedules the specified state machine to be pushed forward when the specified awaiter completes.
        /// </summary>
        /// <typeparam name="TAwaiter">Specifies the type of the awaiter.</typeparam>
        /// <typeparam name="TStateMachine">Specifies the type of the state machine.</typeparam>
        /// <param name="awaiter">The awaiter.</param>
        /// <param name="stateMachine">The state machine.</param>
        public void AwaitOnCompleted<TAwaiter, TStateMachine>(
            ref TAwaiter awaiter, ref TStateMachine stateMachine)
            where TAwaiter : INotifyCompletion
            where TStateMachine : IAsyncStateMachine
        {
            m_Builder.AwaitOnCompleted(ref awaiter, ref stateMachine);
        }

        /// <summary>
        /// Schedules the specified state machine to be pushed forward when the specified awaiter completes.
        /// </summary>
        /// <typeparam name="TAwaiter">Specifies the type of the awaiter.</typeparam>
        /// <typeparam name="TStateMachine">Specifies the type of the state machine.</typeparam>
        /// <param name="awaiter">The awaiter.</param>
        /// <param name="stateMachine">The state machine.</param>
        public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(
            ref TAwaiter awaiter, ref TStateMachine stateMachine)
            where TAwaiter : ICriticalNotifyCompletion
            where TStateMachine : IAsyncStateMachine
        {
            m_Builder.AwaitUnsafeOnCompleted(ref awaiter, ref stateMachine);
        }

        /// <summary>
        /// Completes the <see cref="System.Threading.Tasks.Task"/> in the <see cref="TaskStatus">RanToCompletion</see>
        /// state.
        /// </summary>
        /// <param name="result">The result to use to complete the task.</param>
        /// <exception cref="InvalidOperationException">The builder is not initialized.</exception>
        /// <exception cref="InvalidOperationException">The task has already completed.</exception>
        public void SetResult(TResult result)
        {
            m_Builder.SetResult(result);
        }

        /// <summary>
        /// Completes the <see cref="System.Threading.Tasks.Task"/> in the <see cref="TaskStatus">Faulted</see> state
        /// with the specified exception.
        /// </summary>
        /// <param name="exception">The <see cref="Exception"/> to use to fault the task.</param>
        /// <exception cref="ArgumentNullException">
        /// The <paramref name="exception"/> argument is null (Nothing in Visual Basic).
        /// </exception>
        /// <exception cref="InvalidOperationException">The builder is not initialized.</exception>
        /// <exception cref="InvalidOperationException">The task has already completed.</exception>
        public void SetException(Exception exception)
        {
            m_Builder.SetException(exception);
        }
    }
}
