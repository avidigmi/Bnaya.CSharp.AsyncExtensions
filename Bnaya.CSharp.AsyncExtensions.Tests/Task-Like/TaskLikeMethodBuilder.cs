﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

// See: https://github.com/Microsoft/referencesource/blob/master/Microsoft.Bcl/System.Threading.Tasks.v1.5/System/Runtime/CompilerServices/AsyncTaskMethodBuilder.cs

using System.Runtime.InteropServices;
using System.Security;
using System.Threading.Tasks;

namespace System.Runtime.CompilerServices
{
    /// <summary>Represents a builder for asynchronous methods that returns a <see cref="TaskLike{TResult}"/>.</summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    [StructLayout(LayoutKind.Auto)]
    public struct TaskLikeMethodBuilder<TResult>
    {
        #region Fields

        /// <summary>
        /// The method builder.
        /// the actual building is delegate to the <see cref="AsyncTaskMethodBuilder"/>.
        /// When state machine is needed it use the underline task's state machine builder
        /// </summary>
        private AsyncTaskMethodBuilder<TResult> _methodBuilder;
        /// <summary>
        /// The result for this builder, 
        /// if it's completed before any awaits occur.
        /// </summary>
        private TResult _result;
        /// <summary>
        /// true if <see cref="_result"/> contains the synchronous result
        /// for the async method; otherwise, false.
        /// </summary>
        private bool _haveResult;
        /// <summary>
        /// true if the builder should be used 
        /// for setting/getting the result; otherwise, false.
        /// </summary>
        private bool _useBuilder;

        #endregion // Fields

        #region Create

        /// <summary>
        /// Creates an instance of the builder.
        /// </summary>
        /// <returns></returns>
        public static TaskLikeMethodBuilder<TResult> Create() =>
            new TaskLikeMethodBuilder<TResult>() { _methodBuilder = AsyncTaskMethodBuilder<TResult>.Create() };

        #endregion // Create

        #region IAsyncStateMachine

        #region Start

        /// <summary>
        /// Begins running the builder with the associated state machine.
        /// </summary>
        /// <typeparam name="TStateMachine">The type of the state machine.</typeparam>
        /// <param name="stateMachine">
        /// The state machine instance, 
        /// passed by reference.
        /// </param>
        public void Start<TStateMachine>(ref TStateMachine stateMachine) 
            where TStateMachine : IAsyncStateMachine
        {
            _methodBuilder.Start(ref stateMachine); // will provide the right ExecutionContext semantics
        }

        #endregion // Start

        #region SetStateMachine

        /// <summary>
        /// Associates the builder with the specified state machine.
        /// </summary>
        /// <param name="stateMachine">The state machine instance to associate with the builder.</param>
        public void SetStateMachine(IAsyncStateMachine stateMachine) =>
            _methodBuilder.SetStateMachine(stateMachine);

        #endregion // SetStateMachine

        #endregion // IAsyncStateMachine

        #region SetResult

        /// <summary>
        /// Marks the task as successfully completed.
        /// </summary>
        /// <param name="result">The result to use to complete the task.</param>
        public void SetResult(TResult result)
        {
            if (_useBuilder)
            {
                _methodBuilder.SetResult(result);
            }
            else
            {
                _result = result;
                _haveResult = true;
            }
        }

        #endregion // SetResult

        #region SetException

        /// <summary>
        /// Marks the task as failed and
        /// binds the specified exception to the task.
        /// </summary>
        /// <param name="exception">The exception to bind to the task.</param>
        public void SetException(Exception exception) =>
            _methodBuilder.SetException(exception);

        #endregion // SetException

        #region Task

        /// <summary>
        /// Gets the task for this builder.
        /// This is what the compiler expect at the async phrase.
        /// </summary>
        public TaskLike<TResult> Task
        {
            get
            {
                if (_haveResult)
                {
                    return new TaskLike<TResult>(_result);
                }
                else
                {
                    _useBuilder = true;
                    return new TaskLike<TResult>(_methodBuilder.Task);
                }
            }
        }

        #endregion // Task

        #region AwaitOnCompleted

        /// <summary>
        /// Schedules the state machine to proceed to 
        /// the next action when the specified awaiter completes.
        /// </summary>
        /// <typeparam name="TAwaiter">The type of the awaiter.</typeparam>
        /// <typeparam name="TStateMachine">The type of the state machine.</typeparam>
        /// <param name="awaiter">the awaiter</param>
        /// <param name="stateMachine">The state machine.</param>
        public void AwaitOnCompleted<TAwaiter, TStateMachine>(
                                    ref TAwaiter awaiter, 
                                    ref TStateMachine stateMachine)
                where TAwaiter : INotifyCompletion
                where TStateMachine : IAsyncStateMachine
        {
            _useBuilder = true;
            _methodBuilder.AwaitOnCompleted(ref awaiter, ref stateMachine);
        }

        #endregion // AwaitOnCompleted

        #region AwaitUnsafeOnCompleted

        /// <summary>
        /// Schedules the state machine to proceed to 
        /// the next action when the specified awaiter completes.
        /// </summary>
        /// <typeparam name="TAwaiter">The type of the awaiter.</typeparam>
        /// <typeparam name="TStateMachine">The type of the state machine.</typeparam>
        /// <param name="awaiter">the awaiter</param>
        /// <param name="stateMachine">The state machine.</param>
        [SecuritySafeCritical]
        public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(
                            ref TAwaiter awaiter,
                            ref TStateMachine stateMachine)
            where TAwaiter : ICriticalNotifyCompletion
            where TStateMachine : IAsyncStateMachine
        {
            _useBuilder = true;
            _methodBuilder.AwaitUnsafeOnCompleted(ref awaiter, ref stateMachine);
        }

        #endregion // AwaitUnsafeOnCompleted
    }
}
