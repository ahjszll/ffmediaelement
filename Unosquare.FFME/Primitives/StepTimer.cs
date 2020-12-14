namespace Unosquare.FFME.Primitives
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Defines a timer for discrete event firing.
    /// Execution of callbacks is ensured non re-entrant.
    /// A single thread is used to execute callbacks in <see cref="ThreadPool"/> threads
    /// for all registered <see cref="StepTimer"/> instances. This effectively reduces
    /// the amount <see cref="Timer"/> instances when many of such objects are required.
    /// </summary>
    public sealed class StepTimer : IDisposable
    {
        private static readonly List<StepTimer> RegisteredTimers = new List<StepTimer>();
        private static readonly ConcurrentQueue<StepTimer> PendingAddTimers = new ConcurrentQueue<StepTimer>();
        private static readonly ConcurrentQueue<StepTimer> PendingRemoveTimers = new ConcurrentQueue<StepTimer>();

        private static readonly Thread TimerThread = new Thread(ExecuteCallbacks)
        {
            IsBackground = true,
            Name = nameof(StepTimer),
            Priority = ThreadPriority.AboveNormal
        };


        private readonly Action UserCallback;
        private int m_IsDisposing;
        private int m_IsRunningCycle;

        /// <summary>
        /// Initializes static members of the <see cref="StepTimer"/> class.
        /// </summary>
        static StepTimer()
        {
            TimerThread.Start();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StepTimer"/> class.
        /// </summary>
        /// <param name="callback">The callback.</param>
        public StepTimer(Action callback)
        {
            UserCallback = callback;
            PendingAddTimers.Enqueue(this);
        }


        /// <summary>
        /// Gets or sets a value indicating whether this instance is running cycle to prevent reentrancy.
        /// </summary>
        private bool IsRunningCycle
        {
            get => Interlocked.CompareExchange(ref m_IsRunningCycle, 0, 0) != 0;
            set => Interlocked.Exchange(ref m_IsRunningCycle, value ? 1 : 0);
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is disposing.
        /// </summary>
        private bool IsDisposing
        {
            get => Interlocked.CompareExchange(ref m_IsDisposing, 0, 0) != 0;
            set => Interlocked.Exchange(ref m_IsDisposing, value ? 1 : 0);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        public void Dispose()
        {
            IsRunningCycle = true;
            if (IsDisposing) return;
            IsDisposing = true;
            PendingRemoveTimers.Enqueue(this);
        }

        /// <summary>
        /// Implements the execute-wait cycles of the thread.
        /// </summary>
        /// <param name="state">The state.</param>
        private static void ExecuteCallbacks(object state)
        {
            while (true)
            {
                Parallel.ForEach(RegisteredTimers, (t) =>
                {
                    if (t.IsRunningCycle || t.IsDisposing)
                        return;

                    t.IsRunningCycle = true;

                    Task.Run(() =>
                    {
                        try
                        {
                            t.UserCallback?.Invoke();
                        }
                        finally
                        {
                          t.IsRunningCycle = false;
                        }
                    });
                });

                while (PendingAddTimers.TryDequeue(out var addTimer))
                    RegisteredTimers.Add(addTimer);

                while (PendingRemoveTimers.TryDequeue(out var remTimer))
                    RegisteredTimers.Remove(remTimer);

                Task.Delay(Constants.DefaultTimingPeriod).Wait();
            }
        }
    }
}
