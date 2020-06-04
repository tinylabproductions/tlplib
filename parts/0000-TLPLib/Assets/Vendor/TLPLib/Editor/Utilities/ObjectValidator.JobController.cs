using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace com.tinylabproductions.TLPLib.Utilities.Editor {
  public partial class ObjectValidator {
    /// <summary>
    /// Manages efficient parallel job execution for object validator.
    /// </summary>
    public sealed class JobController {
      // We batch jobs that are executed in parallel because it's faster to run them in batches rather
      // than running very many small jobs.
      const int BATCH_SIZE = 100;
      
      readonly ConcurrentBag<Action> 
        mainThreadJobs = new ConcurrentBag<Action>(),
        batchedJobs = new ConcurrentBag<Action>();
      readonly ConcurrentBag<Exception> _jobExceptions = new ConcurrentBag<Exception>();
      public IReadOnlyCollection<Exception> jobExceptions => _jobExceptions;

      long runningNonMainThreadJobs, _jobsDone, _jobsMax;

      public long jobsDone => Interlocked.Read(ref _jobsDone);
      public long jobsMax => Interlocked.Read(ref _jobsMax);

      public override string ToString() {
        return $"mainT={mainThreadJobs.Count}, batched={batchedJobs.Count}, " +
               $"jobs={jobsDone}/{jobsMax}, running={Interlocked.Read(ref runningNonMainThreadJobs)}";
      }

      public void enqueueMainThreadJob(Action action) {
        Interlocked.Increment(ref _jobsMax);
        mainThreadJobs.Add(() => {
          action();
          Interlocked.Increment(ref _jobsDone);
        });
      }

      public void enqueueParallelJob(Action action) => batchedJobs.Add(action);

      public bool launchParallelJobs(bool launchUnderBatchSize) {
        var jobsLaunched = false;
        while (
          batchedJobs.Count >= BATCH_SIZE 
          || launchUnderBatchSize
        ) {
          var batch = new List<Action>(BATCH_SIZE);
          while (true) {
            if (batch.Count >= BATCH_SIZE) break;
            if (!batchedJobs.TryTake(out var job)) break;
            batch.Add(job);
          }

          if (batch.Count == 0) break;
          
          launchParallelJob(() => {
            foreach (var job in batch) {
              try {
                job();
              }
              catch (Exception e) {
                _jobExceptions.Add(e);
              }
            }
          });
          jobsLaunched = true;
        }

        return jobsLaunched;
      }
      
      public enum MainThreadAction : byte { RerunImmediately, RerunAfterDelay, Halt }
      
      /// <summary>
      /// Keep calling this from main thread until it instructs you to halt.
      /// </summary>
      public MainThreadAction serviceMainThread(bool launchUnderBatchSize) {
        var parallelJobsLaunched = launchParallelJobs(launchUnderBatchSize);
        if (parallelJobsLaunched) return MainThreadAction.RerunImmediately;

        var mainThreadJobsLaunched = false;
        while (mainThreadJobs.TryTake(out var mainThreadJob)) {
          mainThreadJob();
          mainThreadJobsLaunched = true;
        }
        if (mainThreadJobsLaunched) return MainThreadAction.RerunImmediately;

        return 
          Interlocked.Read(ref runningNonMainThreadJobs) == 0 
          ? MainThreadAction.Halt : MainThreadAction.RerunAfterDelay;
      }

      void launchParallelJob(Action action) {
        Interlocked.Increment(ref runningNonMainThreadJobs);
        Interlocked.Increment(ref _jobsMax);
        Task.Run(() => {
          try {
            action();
          }
          catch (Exception e) {
            _jobExceptions.Add(e);
          }
          finally {
            Interlocked.Decrement(ref runningNonMainThreadJobs);
            Interlocked.Increment(ref _jobsDone);
          }
        });
      }
    }
  }
}