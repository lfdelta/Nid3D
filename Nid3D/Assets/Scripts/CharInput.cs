using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharController))]
public class CharInput : MonoBehaviour {

	private CharController character;
	private Transform cam;
	private Vector3 camInXZ;
	private Vector3 move;
	private bool jump;
	private int heightkey;

	// Use this for initialization
	void Start () {
		character = GetComponent<CharController>();
		cam = Camera.main.transform;
	}
	
	// Update is called once per frame
	void Update () {
		if (!jump)
			jump = Input.GetButtonDown ("Jump");
		if (heightkey == 0)
			heightkey = (Input.GetButtonDown ("HeightUp") ? 1 : 0) - (Input.GetButtonDown("HeightDown") ? 1 : 0);
	}

	void FixedUpdate () {
		float horizIn = Input.GetAxis ("Horizontal");
		float vertIn = Input.GetAxis ("Vertical");
	
		camInXZ = Vector3.Scale(cam.forward, new Vector3(1, 0, 1)).normalized;
		move = vertIn*camInXZ + horizIn*cam.right;

		character.Move (move, heightkey, jump);
		heightkey = 0;
		jump = false;
	}
}
