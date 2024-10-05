using System;
using System.Collections.Generic;
using System.Text;

namespace HW3T1
{
    /// <summary>
    /// My Task interface.
    /// </summary>
    public interface IMyTask<out TResult>
    {
        /// <summary>
        /// Is current task completed.
        /// </summary>
        bool IsCompleted { get; }

        /// <summary>
        /// Blocks the caller unteil the end of a calculation.
        /// </summary>
        TResult Result { get; }

        /// <summary>
        /// Apply new function to previous result.
        /// </summary>
        IMyTask<TNewResult> ContinueWith<TNewResult>(Func<TResult, TNewResult> func);
    }
}
