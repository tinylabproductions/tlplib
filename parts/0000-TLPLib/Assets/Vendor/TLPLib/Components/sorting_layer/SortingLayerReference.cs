
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Components.sorting_layer {
  /// <summary>
  /// This attempts to solve the problem of objects in the project having their sorting
  /// properties scaterred all over the place.
  ///
  /// To properly sort objects we essentially need to know all the relationships between
  /// all the sorting layers and orders in those layers.
  ///
  /// For example if we have two sorting layers - background and foreground, and an object A
  /// in (background, order 0) and we want object B to be in background, but in from of A,
  /// we need to set its order to some number.
  ///
  /// But what number do we use? 1? 10? 100? There is essentially no way to know unless you
  /// inspect in the runtime all the places where object B can appear and pick a number that makes
  /// sure it is displayed correctly.
  ///
  /// And this problem gets worse with each object that needs its own unique position in a sorting
  /// chain.
  ///
  /// So instead of having these (sorting layer, order in layer) pairs scaterred all over the
  /// project we store them as serialized objects in one directory in a project and have a central
  /// location where we can get an overview of all sorting layers that are used.
  ///
  /// Then we can use components like <see cref="CanvasSortingLayer"/> to set them on actual
  /// objects.
  ///
  /// This makes it much easier to edit existing layers or create new layers somewhere in the
  /// sorting chain.
  ///
  /// ... just another thing Unity should have built-in...
  /// </summary>
  [
    CreateAssetMenu,
    // Help(
    //   HelpType.Info, HelpPosition.Before,
    //   "Sorting layer and order of object in that layer bundled together. " +
    //   "See code for more detailed explanation."
    // )
  ]
  public class SortingLayerReference : ScriptableObject {
    #region Unity Serialized Fields

#pragma warning disable 649
    // ReSharper disable NotNullMemberIsNotInitialized, FieldCanBeMadeReadOnly.Local, ConvertToConstant.Local
    [SerializeField, SortingLayer] int _sortingLayer;
    [SerializeField] int _orderInLayer;
    // ReSharper restore NotNullMemberIsNotInitialized, FieldCanBeMadeReadOnly.Local, ConvertToConstant.Local
#pragma warning restore 649

    #endregion

    public int sortingLayer => _sortingLayer;
    public int orderInLayer => _orderInLayer;

    public void applyTo(Canvas canvas) {
      canvas.sortingLayerID = sortingLayer;
      canvas.sortingOrder = orderInLayer;
    }

    public void applyTo(Renderer renderer) {
      renderer.sortingLayerID = sortingLayer;
      renderer.sortingOrder = orderInLayer;
    }
  }
}
