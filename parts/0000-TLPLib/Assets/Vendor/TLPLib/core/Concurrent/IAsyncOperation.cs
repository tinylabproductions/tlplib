using System.Collections;
using System.Collections.Generic;
using System.Linq;
using com.tinylabproductions.TLPLib.Extensions;
using GenerationAttributes;
using JetBrains.Annotations;
using pzd.lib.exts;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Concurrent {
  public interface IAsyncOperation {
    [PublicAPI] int priority { get; set; }
    [PublicAPI] float progress { get; }
    [PublicAPI] bool isDone { get; }
    [PublicAPI] IEnumerator yieldInstruction { get; }
  }

  [PublicAPI]
  public class WrappedAsyncOperation : IAsyncOperation {
    public readonly AsyncOperation op;
    
    public WrappedAsyncOperation(AsyncOperation op) { this.op = op; }
    public int priority { get => op.priority; set => op.priority = value; }
    public float progress => op.isDone ? 1f : op.progress;
    public bool isDone => op.isDone;
    public IEnumerator yieldInstruction { get { yield return op; } }
  }
  
  /// <summary>
  /// <see cref="IAsyncOperation"/> operation which is not really an operation.
  /// </summary>
  [PublicAPI, Singleton] public sealed partial class ASyncOperationCompleted : IAsyncOperation {
    public int priority { get => 0; set { } }
    public float progress => 1;
    public bool isDone => true;
    public IEnumerator yieldInstruction { get { yield break; } }
  }
  
  [PublicAPI, Singleton] public sealed partial class ASyncOperationUnfulfilled : IAsyncOperation {
    public int priority { get => 0; set { } }
    public float progress => 0;
    public bool isDone => false;
    public IEnumerator yieldInstruction { get { yield break; } }
  }

  public static class IAsyncOperationExts {
    [PublicAPI] public static IAsyncOperation join(this IList<IAsyncOperation> operations) => 
      new JoinedAsyncOperation(operations);
    
    [PublicAPI] public static IAsyncOperation join(this IEnumerable<IAsyncOperation> operations) => 
      new JoinedAsyncOperation(operations.ToArray());
  }

  [PublicAPI]
  public class JoinedAsyncOperation : IAsyncOperation {
    readonly IList<IAsyncOperation> operations;
    public float progress => operations.Sum(_ => _.isDone ? 1 : _.progress) / operations.Count;
    public bool isDone => operations.All(_ => _.isDone);
    public IEnumerator yieldInstruction { get; }

    public JoinedAsyncOperation(IList<IAsyncOperation> operations) {
      this.operations = operations;

      IEnumerator createEnumerator() {
        // ReSharper disable once ForCanBeConvertedToForeach
        for (var idx = 0; idx < operations.Count; idx++) {
          yield return operations[idx];
        }
      }
      
      yieldInstruction = createEnumerator();
    }

    public int priority {
      get => operations.headOption().fold(0, _ => _.priority);
      set {
        // ReSharper disable once ForCanBeConvertedToForeach
        for (var idx = 0; idx < operations.Count; idx++) {
          operations[idx].priority = value;
        }
      }
    }
  }
}