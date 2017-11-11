using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharController))]
public class CharInput : MonoBehaviour {

	private CharController character;
	private Transform cam;
  private Vector3 camInXZ;
  private Vector3 move;
	public ControlState controlState;

  // Use this for initialization
	void Start () {
    character = GetComponent<CharController>();
		cam = Camera.main.transform;
    controlState = new ControlState ();
  }
	
	// Update is called once per frame
	void Update () {
		if (!controlState.jump)
			controlState.jump = Input.GetButtonDown ("Jump");
		if (controlState.heightChange == 0)
			controlState.heightChange = (Input.GetButtonDown ("HeightUp") ? 1 : 0)
        - (Input.GetButtonDown("HeightDown") ? 1 : 0);
	}

	void FixedUpdate () {
		float horizIn = Input.GetAxis ("Horizontal");
		float vertIn = Input.GetAxis ("Vertical");
	
		camInXZ = Vector3.Scale(cam.forward, new Vector3(1, 0, 1)).normalized;
		move = vertIn*camInXZ + horizIn*cam.right;
    controlState.moveInXZ = (move.magnitude > 1) ? move.normalized : move;

    character.UpdateCharacter (controlState);
		controlState.heightChange = 0;
		controlState.jump = false;
	}
}
