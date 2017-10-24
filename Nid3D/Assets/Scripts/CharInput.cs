using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharController))]
public class CharInput : MonoBehaviour {

	private CharController character;
	private Transform cam;
  private Vector3 camInXZ;
  private Vector3 move;
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
			control_state.jump = Input.GetButtonDown ("Jump");
		if (control_state.heightChange == 0)
			control_state.heightChange = (Input.GetButtonDown ("HeightUp") ? 1 : 0)
        - (Input.GetButtonDown("HeightDown") ? 1 : 0);
	}

	void FixedUpdate () {
		float horizIn = Input.GetAxis ("Horizontal");
		float vertIn = Input.GetAxis ("Vertical");
	
		camInXZ = Vector3.Scale(cam.forward, new Vector3(1, 0, 1)).normalized;
		move = vertIn*camInXZ + horizIn*cam.right;
    control_state.moveInXZ = move;
    
		character.Move (control_state);
		control_state.heightChange = 0;
		control_state.jump = false;
	}
}
