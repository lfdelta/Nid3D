using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class CharController : MonoBehaviour {
	private enum Height {Crouch, Low, Mid, High, Throw};

	public float moveSpeed = 20;
	public float jumpForce = 10;
	public float gravMultiplier = 1;
	public float groundCheckDist = 0.1f;
	public Vector3 origToFeet = 1f * Vector3.down;

	private Rigidbody rbody;
	private CapsuleCollider capsule;
	private Vector3 capsuleCenter;
	private float capsuleHeight;
	private Height height;
	private bool isGrounded;
	private Vector3 groundNormal;

	private int i = 0; // just for debugging


	// Use this for initialization
	void Start () {
		rbody = GetComponent<Rigidbody> ();
		rbody.constraints = RigidbodyConstraints.FreezeRotation;

		capsule = GetComponent<CapsuleCollider> ();
		capsuleCenter = capsule.center;
		capsuleHeight = capsule.height;

		height = Height.Mid;
	}

	public void Move (Vector3 moveInXZ, int heightkey, bool jump) {
		CheckGround ();

		Vector3 move = moveSpeed * transform.InverseTransformDirection (moveInXZ.normalized); // convert from world space to local/object space
		move = Vector3.ProjectOnPlane(move, groundNormal);

		if (isGrounded) {
			if (heightkey != 0) {
				height += heightkey;
				height = (Height)Tools.Clamp ((int)height, (int)Height.Crouch, (int)Height.Throw);
				Debug.Log (height);
			}

			// *** handle height changes

			rbody.AddForce (move);

			if (jump)
				rbody.AddForce (jumpForce * Vector3.up);
		} else {
			// *** gravity
		}
	}

	void CheckGround() {
		RaycastHit info;
		// send raycast from just above the feet
		if (Physics.Raycast (transform.position + origToFeet + 0.1f*Vector3.up, Vector3.down, out info, groundCheckDist)) {
			isGrounded = true;
			groundNormal = info.normal;
		} else {
			isGrounded = false;
			groundNormal = Vector3.up;
		}
	}
}
