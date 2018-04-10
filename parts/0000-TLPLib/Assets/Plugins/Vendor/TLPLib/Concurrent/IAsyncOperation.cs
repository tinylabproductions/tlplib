using System.Collections;
using System.Collections.Generic;
using System.Linq;
using com.tinylabproductions.TLPLib.Extensions;
using JetBrains.Annotations;

namespace com.tinylabproductions.TLPLib.Concurrent {
  public interface IAsyncOperation {
    [PublicAPI] int priority { get; set; }
    [PublicAPI] IEnumerator yieldInstruction { get; }
  }
  
  /// <summary>
  /// <see cref="IAsyncOperation"/> operation which is not really an operation.
  /// </summary>
  [PublicAPI]
  public class ASyncOperationFake : IAsyncOperation {
    [PublicAPI] public static readonly IAsyncOperation instance = new ASyncOperationFake();
    ASyncOperationFake() {}
    
    public int priority { get => 0; set { } }
    public IEnumerator yieldInstruction { get { yield return null; } }
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
    public IEnumerator yieldInstruction { get; }

    public JoinedAsyncOperation(IList<IAsyncOperation> operations) {
      this.operations = operations;

      IEnumerator creatEnumerator() {
        // ReSharper disable once ForCanBeConvertedToForeach
        for (var idx = 0; idx < operations.Count; idx++) {
          yield return operations[idx];
        }
      }
      
      yieldInstruction = creatEnumerator();
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