using com.tinylabproductions.TLPLib.Utilities;
using UnityEngine;

[AddComponentMenu("Utilities/HUDFPS")]
public class HUDFPS : MonoBehaviour
{
  // Attach this to any object to make a frames/second indicator.
  //
  // It calculates frames/second over each updateInterval,
  // so the display does not keep changing wildly.
  //
  // It is also fairly accurate at very low FPS counts (<10).
  // We do this not by simply counting frames per interval, but
  // by accumulating FPS for each frame. This way we end up with
  // corstartRect overall FPS even if the interval renders something like
  // 5.5 frames.

  public Rect startRect = new Rect(10, 10, 75, 50); // The rect the window is initially displayed at.
  public bool updateColor = true; // Do you want the color to change if the FPS gets low
  public bool allowDrag = true; // Do you want to allow the dragging of the FPS window
  public int nbDecimal = 1; // How many decimal do you want to display

  private Color color = Color.white; // The color of the GUI, depending of the FPS ( R < 10, Y < 30, G >= 30 )
  private string sFPS = ""; // The fps formatted into a string.
  private GUIStyle style; // The style the text will be displayed at, based en defaultSkin.label.

  void Start() {
    FPS.fps.subscribe(fps => {
      sFPS = fps.ToString("f" + Mathf.Clamp(nbDecimal, 0, 10));

      //Update the color
      color = (fps >= 30) ? Color.green : ((fps > 10) ? Color.red : Color.yellow);
    });
  }

  void OnGUI()
  {
    // Copy the default label skin, change the color and the alignement
    if (style == null)
    {
      style = new GUIStyle(GUI.skin.label);
      style.normal.textColor = Color.white;
      style.alignment = TextAnchor.MiddleCenter;
    }

    GUI.color = updateColor ? color : Color.white;
    startRect = GUI.Window(0, startRect, DoMyWindow, "");
  }

  void DoMyWindow(int windowID)
  {
    GUI.Label(new Rect(0, 0, startRect.width, startRect.height), sFPS + " FPS", style);
    if (allowDrag) GUI.DragWindow(new Rect(0, 0, Screen.width, Screen.height));
  }
}