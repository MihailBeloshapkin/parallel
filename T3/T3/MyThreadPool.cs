using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HW3T1
{
    /// <summary>
    /// Thread pool.
    /// </summary>
    public class MyThreadPool
    {
        public MyThreadPool(int countOfThreads)
        {
            this.threads = new Thread[countOfThreads];
            this.cancellationTokenSource = new CancellationTokenSource();
            this.tasks = new ConcurrentQueue<Action>();
            this.Start();
        }

        
        private Thread[] threads = null;

        private readonly CancellationTokenSource cancellationTokenSource = null;

        private ConcurrentQueue<Action> tasks = null;

        private Object locker = new Object();

        private AutoResetEvent taskController = new AutoResetEvent(false);

        /// <summary>
        /// Start all threads.
        /// </summary>
        private void Start()
        {
            for (int iter = 0; iter < threads.Length; iter++)
            {
                threads[iter] = new Thread(() => Execute(cancellationTokenSource.Token));
                threads[iter].Start();
            }
        }

        /// <summary>
        /// Execute current.
        /// </summary>
        public void Execute(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                if (this.tasks.TryDequeue(out Action task))
                {
                    task.Invoke();
                }
                else
                {
                    taskController.WaitOne();
                }
            }
        }

        /// <summary>
        /// Interrupts all executing tasks.
        /// </summary>
        public void Shutdown()
        {
            this.tasks = null;
            cancellationTokenSource.Cancel();
        }

        /// <summary>
        /// Submit new function.
        /// </summary>
        /// <param name="func">Input function</param>
        public IMyTask<TResult> Submit<TResult>(Func<TResult> func)
        {
            if (func == null)
            {
                throw new ArgumentNullException();
            }

            if (this.cancellationTokenSource.IsCancellationRequested)
            {
                throw new InvalidOperationException();
            }

            var newTask = new MyTask<TResult>(func, this);
            this.tasks.Enqueue(newTask.Execute);
            taskController.Set();
            return newTask;
        }

        /// <summary>
        /// Submit new action.
        /// </summary>
        private void SubmitAction<TResult>(Action action)
        {
            if (!cancellationTokenSource.IsCancellationRequested)
            {
                this.tasks.Enqueue(action);
                taskController.Set();
            }
        }

        /// <summary>
        /// Task and its methods.
        /// </summary>
        private class MyTask<TResult> : IMyTask<TResult>
        {
            public MyTask(Func<TResult> func, MyThreadPool threadPool)
            {
                this.threadPool = threadPool;
                this.func = func;
                this.submitFunctionsQueue = new Queue<Action>();
            }

            public TResult Result
            {
                get
                {
                    waitResult.Wait();
                    return this.result;
                }
            }

            public bool IsCompleted { get; private set; } = false;

            private MyThreadPool threadPool = null;

            private Func<TResult> func;

            private AggregateException aggregateException = null;

            private Queue<Action> submitFunctionsQueue = null;

            private TResult result = default(TResult);

            private Object locker = new Object();

            private readonly CountdownEvent waitResult = new CountdownEvent(1);

            /// <summary>
            /// Apply new function to a previous result.
            /// </summary>
            public IMyTask<TNewResult> ContinueWith<TNewResult>(Func<TResult, TNewResult> newFunc)
            {
                if (newFunc == null)
                {
                    throw new ArgumentNullException("Input function equals null.");
                }
                if (this.threadPool.cancellationTokenSource.IsCancellationRequested)
                {
                    throw new ApplicationException("Shutdown method has alredy been called.");
                }

                lock (locker)
                {
                    if (this.IsCompleted)
                    {
                        return threadPool.Submit(() => newFunc(result));
                    }

                    var newTask = new MyTask<TNewResult>(() => newFunc(result), threadPool);
                    this.submitFunctionsQueue.Enqueue(newTask.Execute);
                    return newTask;
                }
            }

            /// <summary>
            /// Apply function to result.
            /// </summary>
            public void Execute()
            {
                try
                {
                    result = this.func();
                }
                catch (Exception e)
                {
                    this.aggregateException = new AggregateException(e);
                }
                finally
                {
                    lock (locker)
                    {
                        this.IsCompleted = true;
                        while (submitFunctionsQueue.Count > 0)
                        {
                            threadPool.SubmitAction<TResult>(submitFunctionsQueue.Dequeue());
                        }
                        waitResult.Signal();
                    }
                    
                }
            }
        }
    }
}

