using System;
using System.Collections.Immutable;
using System.Linq;

namespace com.tinylabproductions.TLPLib.Editor.AssetReferences {
  public class AssetUpdate {
    public struct Move {
      public readonly string fromPath, toPath;

      public Move(string fromPath, string toPath) {
        this.fromPath = fromPath;
        this.toPath = toPath;
      }
    }

    public readonly ImmutableList<string> newAssets, deletedAssets;
    public readonly ImmutableList<Move> movedAssets;
    public readonly int totalAssets;

    public AssetUpdate(
      ImmutableList<string> newAssets, ImmutableList<string> deletedAssets,
      ImmutableList<Move> movedAssets
    ) {
      this.newAssets = newAssets;
      this.deletedAssets = deletedAssets;
      this.movedAssets = movedAssets;
      totalAssets = newAssets.Count + deletedAssets.Count + movedAssets.Count;
    }

    public static AssetUpdate fromAllAssets(ImmutableList<string> allAssets) => new AssetUpdate(
      allAssets, ImmutableList<string>.Empty, ImmutableList<AssetUpdate.Move>.Empty
    );

    public AssetUpdate filter(
      Func<string, bool> predicate,
      Func<Move, bool> movePredicate
    ) => new AssetUpdate(
      newAssets.Where(predicate).ToImmutableList(),
      deletedAssets.Where(predicate).ToImmutableList(),
      movedAssets.Where(movePredicate).ToImmutableList()
    );
  }
}