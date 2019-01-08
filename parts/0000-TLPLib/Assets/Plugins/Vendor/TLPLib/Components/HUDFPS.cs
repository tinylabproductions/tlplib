using System;
using System.Text;
using com.tinylabproductions.TLPLib.Components.dispose;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Utilities;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Components {
  [AddComponentMenu("Utilities/HUDFPS")]
  public class HUDFPS : InfoWindow {
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

    Color color = Color.white; // The color of the GUI, depending of the FPS ( R < 10, Y < 30, G >= 30 )
    // Fix the capacity in place.
    readonly StringBuilder sFPS = new StringBuilder(9, 9);

    protected override Rect initialRect() => startRect;
    protected override Fn<string> text => sFPS.ToString;
    protected override Fn<Option<Color>> updateColorOptFn => () => updateColor.opt(color);
    protected override bool dragAllowed => allowDrag;

    public static HUDFPS create() {
      var go = new GameObject(nameof(HUDFPS));
      DontDestroyOnLoad(go);
      return go.AddComponent<HUDFPS>();
    }

    public void Start() {
      // Initialize the string
      sFPS.Append("000.0 FPS");

      FPS.fps.subscribe(gameObject.asDisposableTracker(), fps => {
        if (fps > 999) {
          sFPS[0] = '>';
          sFPS[1] = sFPS[2] = sFPS[3] = '9';
          sFPS[4] = ' ';
        }
        else {
          const int ASCII_NUM_START = 48; // numbers start from this code in ASCII
          sFPS[0] = (char) (ASCII_NUM_START + (int) (fps / 100) % 10);
          sFPS[1] = (char) (ASCII_NUM_START + (int) (fps / 10) % 10);
          sFPS[2] = (char) (ASCII_NUM_START + (int) fps % 10);
          sFPS[4] = (char) (ASCII_NUM_START + (int) (fps * 10) % 10);
        }

        //Update the color
        color =
            fps >= 60 ? Color.cyan
          : fps >= 30 ? Color.green
          : fps >= 10 ? Color.yellow
          : Color.red;
      });
    }
  }
}