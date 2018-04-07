using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TeamUtility.IO;

public class PlayerControlState {
  public Vector3 moveInXZ;
  public bool running;
  public bool runHold;
  public float runHoldStartTime;
  public int heightChange; // -1,0,1
  public bool heightHeldLongEnough;
  public int heightHold;
  public float heightHoldStartTime;
  public bool jump;
  public bool attack;

  void Awake () {
    moveInXZ = Vector3.zero;
    running = false;
    heightChange = 0;
    heightHold = 0;
    jump = false;
    attack = false;
  }
}



[RequireComponent(typeof(CharController))]
public class CharInput : MonoBehaviour {

  private CharController character;
  private Transform cam;
  private Vector3 camInXZ;
  private Vector3 move;
  private GameController gameController;
  private PlayerID playerID;
  [HideInInspector] public PlayerControlState controlState;

  private const float runInputMagnitude = 0.9f*0.9f;
  private const float runInputDuration = 0.5f;
  private const float heightHoldDuration = 0.2f;

  void Awake () {
    gameController = FindObjectOfType<GameController> ();
    character = GetComponent<CharController>();
    playerID = character.playerid;
    cam = Camera.main.transform;
    controlState = new PlayerControlState ();
  }

  // Called once per frame; handle inputs here
  // if statements ensure that having multiple Updates between each FixedUpdate won't lead to dropped inputs
  void Update () {
    if (!gameController.IsPaused ()) {
      float horizIn = InputManager.GetAxis ("Horizontal", playerID);
      float vertIn = InputManager.GetAxis ("Vertical", playerID);
      camInXZ = Vector3.Scale (cam.forward, new Vector3 (1, 0, 1)).normalized;
      move = vertIn * camInXZ + horizIn * cam.right;
      controlState.moveInXZ = (move.magnitude > 1) ? move.normalized : move;

      // if you were not above the running input threshold, but you are now, store the start time
      // if you've been holding it for sufficiently long, set "running" to true
      bool holdingRunNow = move.sqrMagnitude > runInputMagnitude;
      if (!controlState.runHold && holdingRunNow)
        controlState.runHoldStartTime = Time.time;
      else if (holdingRunNow)
        controlState.running = (Time.time - controlState.runHoldStartTime) > runInputDuration;
      controlState.runHold = holdingRunNow;

      // handle inputs affecting sword height; if one direction has been held continuously, track that as well
      if (controlState.heightChange == 0) {
        controlState.heightChange = (InputManager.GetButtonDown ("HeightUp", playerID) ? 1 : 0)
        - (InputManager.GetButtonDown ("HeightDown", playerID) ? 1 : 0);
        if (controlState.heightChange != 0) { // you started pressing the button this frame
          controlState.heightHoldStartTime = Time.time;
          controlState.heightHold = controlState.heightChange;
        }
      }
      int currentHold = (InputManager.GetButton ("HeightUp", playerID) ? 1 : 0) - (InputManager.GetButton ("HeightDown", playerID) ? 1 : 0);
      if (currentHold != controlState.heightHold) // you changed directions or stopped between frames
        controlState.heightHeldLongEnough = false;
      else if (currentHold != 0) // you've been holding the same direction for multiple frames
        controlState.heightHeldLongEnough = (Time.time - controlState.heightHoldStartTime) > heightHoldDuration;

      if (!controlState.jump)
        controlState.jump = InputManager.GetButtonDown ("Jump", playerID);
      if (!controlState.attack)
        controlState.attack = InputManager.GetButtonDown ("Attack", playerID);
    }
  }

  // Pass new input state to CharController and reset button press values
  void FixedUpdate () {
    character.UpdateCharacter (controlState);
    controlState.heightChange = 0;
    controlState.jump = false;
    controlState.attack = false;
  }
}
