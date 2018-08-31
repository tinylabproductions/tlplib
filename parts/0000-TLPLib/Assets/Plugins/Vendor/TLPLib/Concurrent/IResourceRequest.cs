using System.Collections;
using JetBrains.Annotations;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Concurrent {
  public interface IResourceRequest : IAsyncOperation {
    [PublicAPI] Object asset { get; }
  }

  public class WrappedResourceRequest : IResourceRequest {
    public readonly ResourceRequest request;

    public WrappedResourceRequest(ResourceRequest request) { this.request = request; }

    public int priority {
      get => request.priority;
      set => request.priority = value;
    }

    public float progress => request.progress;
    public bool isDone => request.isDone;

    public Object asset => request.asset;
    public IEnumerator yieldInstruction { get { yield return request; } }
  }
}