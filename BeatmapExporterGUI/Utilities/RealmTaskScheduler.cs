using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BeatmapExporterGUI.Utilities
{
    /// <summary>
    /// Single thread scheduler used for accessing the Realm database in a synchronous manner.
    /// </summary>
    public sealed class RealmTaskScheduler : TaskScheduler
    {
        [ThreadStatic]
        private static bool isExecuting;
        private readonly CancellationToken cancellationToken;

        private readonly BlockingCollection<Task> taskQueue;

        public RealmTaskScheduler(CancellationToken cancellationToken)
        {
            this.cancellationToken = cancellationToken;
            this.taskQueue = new BlockingCollection<Task>();
        }

        /// <summary>
        /// Begins the RealmTaskScheduler thread.
        /// </summary>
        public void Start()
        {
            new Thread(RunOnCurrentThread)
            {
                Name = "Realm Thread",
                IsBackground = true // this thread should not keep the process alive
            }.Start();
        }

        /// <summary>
        /// Schedules an Task with no return on the Realm thread.
        /// </summary>
        public Task Schedule(Action action)
        {
            return Task.Factory.StartNew(
                action,
                CancellationToken.None,
                TaskCreationOptions.None,
                this
                );
        }

        /// <summary>
        /// Schedules a Task returning a value on the Realm thread.
        /// </summary>
        public Task<T> Schedule<T>(Func<T> func)
        {
            return Task.Factory.StartNew(
                func,
                CancellationToken.None,
                TaskCreationOptions.None,
                this
                );
        }

        private void RunOnCurrentThread()
        {
            isExecuting = true;

            try
            {
                foreach (var task in taskQueue.GetConsumingEnumerable(cancellationToken))
                {
                    TryExecuteTask(task);
                }
            }
            catch (OperationCanceledException)
            { }
            finally
            {
                isExecuting = false;
            }
        }

        public void Complete() { taskQueue.CompleteAdding(); }
        protected override IEnumerable<Task> GetScheduledTasks() { return null; }

        protected override void QueueTask(Task task)
        {
            try
            {
                taskQueue.Add(task, cancellationToken);
            }
            catch (OperationCanceledException)
            { }
        }

        protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
        {
            if (taskWasPreviouslyQueued) return false;

            return isExecuting && TryExecuteTask(task);
        }
    }
}
