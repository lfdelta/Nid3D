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
  public PlayerID playerID;
	public ControlState control_state;

  // Use this for initialization
	void Start () {
    character = GetComponent<CharController>();
		cam = Camera.main.transform;
    control_state = new ControlState ();
  }
	
	// Update is called once per frame
	void Update () {
		if (!control_state.jump)
			control_state.jump = InputManager.GetButtonDown ("Jump", playerID);
		if (control_state.heightChange == 0)
			control_state.heightChange = (InputManager.GetButtonDown ("HeightUp", playerID) ? 1 : 0)
        - (InputManager.GetButtonDown("HeightDown", playerID) ? 1 : 0);
	}

	void FixedUpdate () {
		float horizIn = InputManager.GetAxis ("Horizontal", playerID);
		float vertIn = InputManager.GetAxis ("Vertical", playerID);
	
		camInXZ = Vector3.Scale(cam.forward, new Vector3(1, 0, 1)).normalized;
		move = vertIn*camInXZ + horizIn*cam.right;
    control_state.moveInXZ = move;
    
		character.UpdateCharacter (control_state);
		control_state.heightChange = 0;
		control_state.jump = false;
	}
}
