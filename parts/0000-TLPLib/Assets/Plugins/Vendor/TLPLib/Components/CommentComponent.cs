using com.tinylabproductions.TLPLib.Extensions;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Components {
  /// <summary>
  /// The only use of this component is to comment some terrible behaviour when you do it from code,
  /// so you would not keep wondering what the heck is happening (e.g. why my properties are changed)
  /// in the editor when inspecting an object.
  /// </summary>
  public class CommentComponent : MonoBehaviour {
    [TextArea]
    public string comment;
  }

  public static class CommentComponentExts {
    public static void addCommentComponent(this GameObject go, string comment) {
      var c = go.EnsureComponent<CommentComponent>();
      c.comment = string.IsNullOrEmpty(c.comment) ? comment : $"{c.comment}\n\n${comment}";
    }

    public static void addCommentComponent(this MonoBehaviour bh, string comment) => 
      bh.gameObject.addCommentComponent(comment);
  }
}