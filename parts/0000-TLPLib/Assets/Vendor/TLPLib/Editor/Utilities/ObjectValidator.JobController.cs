using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using com.tinylabproductions.TLPLib.Logger;

namespace com.tinylabproductions.TLPLib.Utilities.Editor {
  public partial class ObjectValidator {
    /// <summary>
    /// Manages efficient parallel job execution for object validator.
    /// </summary>
    public sealed class JobController {
      // We batch jobs that are executed in parallel because it's faster to run them in batches rather
      // than running very many small jobs.
      const int BATCH_SIZE = 100;
      
      readonly ILog log = Log.d.withScope(nameof(ObjectValidator) + "." + nameof(JobController));
      readonly ConcurrentQueue<Action> 
        mainThreadJobs = new ConcurrentQueue<Action>(),
        batchedJobs = new ConcurrentQueue<Action>();

      long runningNonMainThreadJobs, _jobsDone, _jobsMax;

      public long jobsDone => Interlocked.Read(ref _jobsDone);
      public long jobsMax => Interlocked.Read(ref _jobsMax);

      public void enqueueMainThreadJob(Action action) {
        Interlocked.Increment(ref _jobsMax);
        mainThreadJobs.Enqueue(() => {
          action();
          Interlocked.Increment(ref _jobsDone);
        });
      }

      public void enqueueParallelJob(Action action) => batchedJobs.Enqueue(action);

      // DateTime lastTime = DateTime.Now;
      public enum MainThreadAction : byte { RerunImmediately, RerunAfterDelay, Halt }
      
      /// <summary>
      /// Keep calling this from main thread until it instructs you to halt.
      /// </summary>
      public MainThreadAction serviceMainThread() {
        var jobsLeft = Interlocked.Read(ref runningNonMainThreadJobs);
        // if (jobsLeft < 0) {
        //   log.error($"Jobs left < 0! ({jobsLeft})");
        //   jobsLeft = 0;
        // }
        //
        // var time = DateTime.Now;
        // if (time - lastTime >= 500.millis()) {
        //   log.info(
        //     $"jobsLeft={jobsLeft}, _jobsDone={jobsDone}, _jobsMax={jobsMax}, " +
        //     $"mainThreadJobs={mainThreadJobs.Count}, " +
        //     $"batchedJobs={batchedJobs.Count}"
        //   );
        //   lastTime = time;
        // }
        
        if (batchedJobs.Count >= BATCH_SIZE || jobsLeft == 0) {
          var batch = new List<Action>(BATCH_SIZE);
          while (true) {
            if (batch.Count >= BATCH_SIZE) break;
            if (!batchedJobs.TryDequeue(out var job)) break;
            batch.Add(job);
          }

          if (batch.Count != 0) {
            launchParallelJob(() => {
              foreach (var job in batch) {
                job();
              }
            });
            jobsLeft = Interlocked.Read(ref runningNonMainThreadJobs);
          }
        }

        var ranMainThreadJobs = false;
        while (mainThreadJobs.TryDequeue(out var highPriorityJob)) {
          highPriorityJob();
          ranMainThreadJobs = true;
        }
        if (ranMainThreadJobs) {
          return MainThreadAction.RerunImmediately;
        }

        return jobsLeft == 0 ? MainThreadAction.Halt : MainThreadAction.RerunAfterDelay;
      }

      void launchParallelJob(Action action) {
        Interlocked.Increment(ref runningNonMainThreadJobs);
        Interlocked.Increment(ref _jobsMax);
        Parallel.Invoke(() => {
          try {
            action();
          }
          catch (Exception e) {
            log.error("Error in enqueued job", e);
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