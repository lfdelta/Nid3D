using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharController : MonoBehaviour {
	private enum Height {Crouch, Low, Mid, High, Throw};

	public GameObject player;
	public float speed;
	private Height height;

	private float hspeed, vspeed;
	private int heightkey;

	// Use this for initialization
	void Start () {
		CapsuleCollider capsule = GetComponent<CapsuleCollider>();
		height = Height.Mid;
	}
	
	// Update is called once per frame
	void Update () {
		GetInputs ();
	}

	void GetInputs () {
		hspeed = speed * Input.GetAxis ("Horizontal");
		vspeed = speed * Input.GetAxis ("Vertical");
		heightkey = (Input.GetButtonDown ("HeightUp") ? 1 : 0) - (Input.GetButtonDown("HeightDown") ? 1 : 0);

		if (heightkey != 0) {
			height += heightkey;
			Mathf.Clamp (height, Height.Crouch, Height.Throw);
			Debug.Log (height);
		}
	}
}
