using System;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Logger;

namespace com.tinylabproductions.TLPLib.Concurrent {
  /**
   * Lets you to make sure only one concurrent calculation is being executed 
   * at a time.
   * 
   * Imagine that you have a long taking operation that returns a future and only 
   * the latest result is useful.
   * 
   * For example you have map data and need to regenerate a texture based on that data.
   * 
   * When the data changes you start regenerating. However while it is in progress you get 2 more
   * map data changes. That would produce 2 more calculations, but the middle one is redundant, 
   * because we only need to show the latest map data.
   * 
   * So given the following timeline, where:
   * | marks a submit to executor.
   * # marks regeneration.
   * > marks actual code that is executed after regeneration is complete.
   * <pre>
   * data1: |########>
   * data2:    |########>
   * data3:        |########>
   * </pre>
   * 
   * We can see that data2 can be eliminated. This class allows you to do that.   * 
   **/
  public class LatestCalculationExecutor<A> {
    readonly LogI log;

    Option<Future<A>> current = F.none<Future<A>>();
    Option<Fn<Future<A>>> next = F.none<Fn<Future<A>>>();

    public LatestCalculationExecutor() { log = Log.logger; }
    public LatestCalculationExecutor(LogI log) { this.log = log; }

    public Future<A> execute(Fn<Future<A>> createFuture) {
      return current.fold(
        () => {
          log.debug("No current future, executing current: " + stateStr);
          return executeCurrent(createFuture);
        },
        currentFuture => {
          next = createFuture.some();
          log.debug("Current future executing, stored next: " + stateStr);
          return currentFuture.flatMap(_ => {
            log.debug("Current future executed, executing next: " + stateStr);
            return executeNext();
          });
        }
      );
    }

    public void cancelNext() { next = F.none<Fn<Future<A>>>(); }

    Future<A> executeCurrent(Fn<Future<A>> createFuture) {
      var future = createFuture();
      current = future.some();
      return future.tapComplete(_ => {
        current = F.none<Future<A>>();
        log.debug("Current future executed, cleared current:" + stateStr);
      });
    }

    Future<A> executeNext() {
      var nextFuture = next.fold(Future.unfullfiled<A>, executeCurrent);
      cancelNext();
      return nextFuture;
    }

    string stateStr { get { return string.Format("current={0} next={1}", current, next); } }
  }
}
