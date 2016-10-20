using UnityEngine;

/**
 * This is semi-generated with Visual Studio 2015 and VS Tools for Unity.
 *
 * Ctrl+Shift+M lets you generate method bodies and then you use a regex to turn them
 * into interfaces.
 *
 * Needle: public void (\w+)\((.*?)\)\r?\n\s+{\s+}
 * Replacement: public interface IMB_$1 { void $1($2); }
 **/
namespace com.tinylabproductions.TLPLib.Components.Interfaces
{
  // Awake is called when the script instance is being loaded
  public interface IMB_Awake { void Awake(); }

  // Update is called every frame, if the MonoBehaviour is enabled
  public interface IMB_Update { void Update(); }

  // Start is called just before any of the Update methods is called the first time
  public interface IMB_Start { void Start(); }

  // Reset to default values
  public interface IMB_Reset { void Reset(); }

  // OnWillRenderObject is called once for each camera if the object is visible
  public interface IMB_OnWillRenderObject { void OnWillRenderObject(); }

  // This function is called when the script is loaded or a value is changed in the inspector (Called in the editor only)
  public interface IMB_OnValidate { void OnValidate(); }

  // OnTriggerStay2D is called once per frame for every Collider2D other that is touching the trigger (2D physics only)
  public interface IMB_OnTriggerStay2D { void OnTriggerStay2D(Collider2D collision); }

  // OnTriggerStay is called once per frame for every Collider other that is touching the trigger
  public interface IMB_OnTriggerStay { void OnTriggerStay(Collider other); }

  // OnTriggerExit2D is called when the Collider2D other has stopped touching the trigger (2D physics only)
  public interface IMB_OnTriggerExit2D { void OnTriggerExit2D(Collider2D collision); }

  // OnTriggerExit is called when the Collider other has stopped touching the trigger
  public interface IMB_OnTriggerExit { void OnTriggerExit(Collider other); }

  // OnTriggerEnter2D is called when the Collider2D other enters the trigger (2D physics only)
  public interface IMB_OnTriggerEnter2D { void OnTriggerEnter2D(Collider2D collision); }

  // OnTriggerEnter is called when the Collider other enters the trigger
  public interface IMB_OnTriggerEnter { void OnTriggerEnter(Collider other); }

  // Callback sent to the graphic afer a Transform parent change occurs
  public interface IMB_OnTransformParentChanged { void OnTransformParentChanged(); }

  // Callback sent to the graphic afer a Transform children change occurs
  public interface IMB_OnTransformChildrenChanged { void OnTransformChildrenChanged(); }

  // Called on the server whenever a Network.InitializeServer was invoked and has completed
  public interface IMB_OnServerInitialized { void OnServerInitialized(); }

  // Used to customize synchronization of variables in a script watched by a network view
  public interface IMB_OnSerializeNetworkView { void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info); }

  // OnRenderObject is called after camera has rendered the scene
  public interface IMB_OnRenderObject { void OnRenderObject(); }

  // OnRenderImage is called after all rendering is complete to render image
  public interface IMB_OnRenderImage { void OnRenderImage(RenderTexture source, RenderTexture destination); }

  // Callback that is sent if an associated RectTransform is removed
  public interface IMB_OnRectTransformRemoved { void OnRectTransformRemoved(); }

  // Callback that is sent if an associated RectTransform has it's dimensions changed
  public interface IMB_OnRectTransformDimensionsChange { void OnRectTransformDimensionsChange(); }

  // OnPreRender is called before a camera starts rendering the scene
  public interface IMB_OnPreRender { void OnPreRender(); }

  // OnPreCull is called before a camera culls the scene
  public interface IMB_OnPreCull { void OnPreCull(); }

  // OnPostRender is called after a camera finished rendering the scene
  public interface IMB_OnPostRender { void OnPostRender(); }

  // Called on the server whenever a player disconnected from the server
  public interface IMB_OnPlayerDisconnected { void OnPlayerDisconnected(NetworkPlayer player); }

  // Called on the server whenever a new player has successfully connected
  public interface IMB_OnPlayerConnected { void OnPlayerConnected(NetworkPlayer player); }

  // OnParticleCollision is called when a particle hits a collider
  public interface IMB_OnParticleCollision { void OnParticleCollision(GameObject other); }

  // Called on objects which have been network instantiated with Network.Instantiate
  public interface IMB_OnNetworkInstantiate { void OnNetworkInstantiate(NetworkMessageInfo info); }

  // OnMouseUpAsButton is only called when the mouse is released over the same GUIElement or Collider as it was pressed
  public interface IMB_OnMouseUpAsButton { void OnMouseUpAsButton(); }

  // OnMouseUp is called when the user has released the mouse button
  public interface IMB_OnMouseUp { void OnMouseUp(); }

  // OnMouseOver is called every frame while the mouse is over the GUIElement or Collider
  public interface IMB_OnMouseOver { void OnMouseOver(); }

  // OnMouseExit is called when the mouse is not any longer over the GUIElement or Collider
  public interface IMB_OnMouseExit { void OnMouseExit(); }

  // OnMouseEnter is called when the mouse entered the GUIElement or Collider
  public interface IMB_OnMouseEnter { void OnMouseEnter(); }

  // OnMouseDrag is called when the user has clicked on a GUIElement or Collider and is still holding down the mouse
  public interface IMB_OnMouseDrag { void OnMouseDrag(); }

  // OnMouseDown is called when the user has pressed the mouse button while over the GUIElement or Collider
  public interface IMB_OnMouseDown { void OnMouseDown(); }

  // Called on clients or servers when reporting events from the MasterServer
  public interface IMB_OnMasterServerEvent { void OnMasterServerEvent(MasterServerEvent msEvent); }

  // This function is called after a new level was loaded
  public interface IMB_OnLevelWasLoaded { void OnLevelWasLoaded(int level); }

  // Called when a joint attached to the same game object broke
  public interface IMB_OnJointBreak { void OnJointBreak(float breakForce); }

  // OnGUI is called for rendering and handling GUI events
  public interface IMB_OnGUI { void OnGUI(); }

  // Called on clients or servers when there is a problem connecting to the MasterServer
  public interface IMB_OnFailedToConnectToMasterServer { void OnFailedToConnectToMasterServer(NetworkConnectionError error); }

  // Called on the client when a connection attempt fails for some reason
  public interface IMB_OnFailedToConnect { void OnFailedToConnect(NetworkConnectionError error); }

  // This function is called when the object becomes enabled and active
  public interface IMB_OnEnable { void OnEnable(); }

  // Implement this OnDrawGizmos if you want to draw gizmos that are also pickable and always drawn
  public interface IMB_OnDrawGizmosSelected { void OnDrawGizmosSelected(); }

  // Implement this OnDrawGizmosSelected if you want to draw gizmos only if the object is selected
  public interface IMB_OnDrawGizmos { void OnDrawGizmos(); }

  // Called on the client when the connection was lost or you disconnected from the server
  public interface IMB_OnDisconnectedFromServer { void OnDisconnectedFromServer(NetworkDisconnection info); }

  // Called on the client when the connection was lost or you disconnected from the master server
  public interface IMB_OnDisconnectedFromMasterServer { void OnDisconnectedFromMasterServer(NetworkDisconnection info); }

  // This function is called when the behaviour becomes disabled or inactive
  public interface IMB_OnDisable { void OnDisable(); }

  // This function is called when the MonoBehaviour will be destroyed
  public interface IMB_OnDestroy { void OnDestroy(); }

  // OnControllerColliderHit is called when the controller hits a collider while performing a Move
  public interface IMB_OnControllerColliderHit { void OnControllerColliderHit(ControllerColliderHit hit); }

  // Called on the client when you have successfully connected to a server
  public interface IMB_OnConnectedToServer { void OnConnectedToServer(); }

  // OnCollisionStay2D is called once per frame for every collider2D/rigidbody2D that is touching rigidbody2D/collider2D (2D physics only)
  public interface IMB_OnCollisionStay2D { void OnCollisionStay2D(Collision2D collision); }

  // OnCollisionStay is called once per frame for every collider/rigidbody that is touching rigidbody/collider
  public interface IMB_OnCollisionStay { void OnCollisionStay(Collision collision); }

  // OnCollisionExit2D is called when this collider2D/rigidbody2D has stopped touching another rigidbody2D/collider2D (2D physics only)
  public interface IMB_OnCollisionExit2D { void OnCollisionExit2D(Collision2D collision); }

  // OnCollisionExit is called when this collider/rigidbody has stopped touching another rigidbody/collider
  public interface IMB_OnCollisionExit { void OnCollisionExit(Collision collision); }

  // OnCollisionEnter2D is called when this collider2D/rigidbody2D has begun touching another rigidbody2D/collider2D (2D physics only)
  public interface IMB_OnCollisionEnter2D { void OnCollisionEnter2D(Collision2D collision); }

  // OnCollisionEnter is called when this collider/rigidbody has begun touching another rigidbody/collider
  public interface IMB_OnCollisionEnter { void OnCollisionEnter(Collision collision); }

  // Callback that is sent if the canvas group is changed
  public interface IMB_OnCanvasGroupChanged { void OnCanvasGroupChanged(); }

  // Callback sent to the graphic before a Transform parent change occurs
  public interface IMB_OnBeforeTransformParentChanged { void OnBeforeTransformParentChanged(); }

  // OnBecameVisible is called when the renderer became visible by any camera
  public interface IMB_OnBecameVisible { void OnBecameVisible(); }

  // OnBecameInvisible is called when the renderer is no longer visible by any camera
  public interface IMB_OnBecameInvisible { void OnBecameInvisible(); }

  // If OnAudioFilterRead is implemented, Unity will insert a custom filter into the audio DSP chain
  public interface IMB_OnAudioFilterRead { void OnAudioFilterRead(float[] data, int channels); }

  // Sent to all game objects before the application is quit
  public interface IMB_OnApplicationQuit { void OnApplicationQuit(); }

  // Sent to all game objects when the player pauses
  public interface IMB_OnApplicationPause { void OnApplicationPause(bool pause); }

  // Sent to all game objects when the player gets or looses focus
  public interface IMB_OnApplicationFocus { void OnApplicationFocus(bool focus); }

  // This callback will be invoked at each frame after the state machines and the animations have been evaluated, but before OnAnimatorIK
  public interface IMB_OnAnimatorMove { void OnAnimatorMove(); }

  // Callback for setting up animation IK (inverse kinematics)
  public interface IMB_OnAnimatorIK { void OnAnimatorIK(int layerIndex); }

  // LateUpdate is called every frame, if the Behaviour is enabled
  public interface IMB_LateUpdate { void LateUpdate(); }

  // This function is called every fixed framerate frame, if the MonoBehaviour is enabled
  public interface IMB_FixedUpdate { void FixedUpdate(); }
}
