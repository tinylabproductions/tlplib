using System;
using GenerationAttributes;
using JetBrains.Annotations;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Tween.fun_tween {
  /// <summary>
  /// Describes for how long the managed tween (<see cref="TweenManager"/>) should run.
  ///
  /// See:
  /// - <see cref="fromGameObject"/> 
  /// - <see cref="fromComponent"/> 
  /// - <see cref="unbounded"/> 
  /// </summary>
  [Record] public sealed partial class TweenManagerLifetime {
    public readonly Func<bool> keepRunning;

    /// <summary>
    /// The lifetime is unbounded - run until the <see cref="TweenManager"/> is explicitly stopped.
    ///
    /// Usually you probably want <see cref="fromGameObject"/>.
    /// </summary>
    [PublicAPI] public static readonly TweenManagerLifetime unbounded = 
      new TweenManagerLifetime(keepRunning: () => true);

    /// <summary>
    /// The lifetime is bound to a given <see cref="GameObject"/> - if that game object is destroyed the
    /// <see cref="TweenManager"/> won't be updated anymore.
    ///
    /// This prevents problems with getting <see cref="NullReferenceException"/> on
    /// <see cref="TweenManager.update"/> when the <see cref="GameObject"/> is destroyed.
    /// </summary>
    [PublicAPI] public static TweenManagerLifetime fromGameObject(GameObject gameObject) =>
      new TweenManagerLifetime(keepRunning: () => gameObject);

    /// <summary>Convenience method so you could pass <see cref="Component"/> directly.</summary>
    [PublicAPI] public static TweenManagerLifetime fromComponent(Component component) =>
      fromGameObject(component.gameObject);
    
    // This seems like a good idea, but it is actually not. Talk to Evaldas & Artūras about it.
    // /// <summary>
    // /// The lifetime is bound to a given <see cref="IDisposableTracker"/> - when that tracker is disposed the
    // /// <see cref="TweenManager"/> won't be updated anymore.
    // /// </summary>
    // [PublicAPI] public static TweenManagerLifetime fromDisposableTracker(IDisposableTracker tracker) {
    //   var keepRunning = true;
    //   tracker.track(() => keepRunning = false);
    //   return new TweenManagerLifetime(keepRunning: () => keepRunning);
    // }

    /// <summary>
    /// Allows you to simply pass a <see cref="GameObject"/> where <see cref="TweenManagerLifetime"/> is expected.
    /// </summary>
    [PublicAPI] public static implicit operator TweenManagerLifetime(GameObject gameObject) => 
      fromGameObject(gameObject);

    /// <summary>
    /// Allows you to simply pass a <see cref="Component"/> where <see cref="TweenManagerLifetime"/> is expected.
    /// </summary>
    [PublicAPI] public static implicit operator TweenManagerLifetime(Component component) => 
      fromComponent(component);

    // /// <summary>
    // /// Allows you to simply pass a <see cref="DisposableTracker"/> where <see cref="TweenManagerLifetime"/> is
    // /// expected.
    // /// </summary>
    // [PublicAPI] public static implicit operator TweenManagerLifetime(DisposableTracker tracker) =>
    //   fromDisposableTracker(tracker);
  }

  // [PublicAPI] public static class TweenManagerLifetimeExts {
  //   /// <summary>We would like to have an implicit conversion, but C# disallows that :(</summary>
  //   public static TweenManagerLifetime asTMLifetime(this IDisposableTracker tracker) => 
  //     TweenManagerLifetime.fromDisposableTracker(tracker);
  // }
}