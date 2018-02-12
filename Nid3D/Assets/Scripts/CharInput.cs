using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TeamUtility.IO;

[RequireComponent(typeof(CharController))]
public class CharInput : MonoBehaviour {

  private CharController character;
  private Transform cam;
  private Vector3 camInXZ;
  private Vector3 move;
  private GameController gameController;
  public PlayerID playerID;
  public PlayerControlState controlState;

  void Start () {
    gameController = FindObjectOfType<GameController> ();
    character = GetComponent<CharController>();
    cam = Camera.main.transform;
    controlState = new PlayerControlState ();
  }

  // Called once per frame; handle instantaneous inputs (buttons) here
  void Update () {
    if (!gameController.IsPaused ()) {
      if (!controlState.jump)
        controlState.jump = InputManager.GetButtonDown ("Jump", playerID);
      if (controlState.heightChange == 0)
        controlState.heightChange = (InputManager.GetButtonDown ("HeightUp", playerID) ? 1 : 0)
        - (InputManager.GetButtonDown ("HeightDown", playerID) ? 1 : 0);
      if (!controlState.attack)
        controlState.attack = InputManager.GetButtonDown ("Attack", playerID);
    }
  }

  // Called once per physics update; handle continuous inputs (axes) here
  // Pass new input state to CharController and reset button press values
  void FixedUpdate () {
    float horizIn = InputManager.GetAxis ("Horizontal", playerID);
    float vertIn = InputManager.GetAxis ("Vertical", playerID);

    camInXZ = Vector3.Scale(cam.forward, new Vector3(1, 0, 1)).normalized;
    move = vertIn*camInXZ + horizIn*cam.right;
    controlState.moveInXZ = (move.magnitude > 1) ? move.normalized : move;

    character.UpdateCharacter (controlState);
    controlState.heightChange = 0;
    controlState.jump = false;
    controlState.attack = false;
  }
}
