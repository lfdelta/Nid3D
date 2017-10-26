using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class CharController : MonoBehaviour {

  // concerning the state of the character
  private bool isGrounded;
	private enum Height {Crouch, Low, Mid, High, Throw};
  public enum Gait {Static, Walk, Run};

	public float walkingMoveForce = 200;
  public float runningMoveForce = 100;
  public float runningSpeed = 2; // speed at which player isRunning
  public float maxSpeed = 5;     // maximum speed for player
	public float jumpForce = 500;
	public float groundCheckDist = 0.1f;
	public Vector3 origToFeet = 1f * Vector3.down;

	private Rigidbody rbody;
	private CapsuleCollider capsule;
 	private Vector3 capsuleCenter;
	private float capsuleHeight;
	private Height height;
  public Gait gait;
	private Vector3 groundNormal;

    
	// Use this for initialization
	void Start () {
		rbody = GetComponent<Rigidbody> ();
		rbody.constraints = RigidbodyConstraints.FreezeRotation;

		capsule = GetComponent<CapsuleCollider> ();
		capsuleCenter = capsule.center;
		capsuleHeight = capsule.height;

		height = Height.Mid;
    
	}

	public void Move (ControlState control_state) {
		CheckGround ();

    if (isGrounded) {
      if (control_state.heightChange != 0) {
        height += control_state.heightChange;
        height = (Height)Tools.Clamp ((int)height, (int)Height.Crouch, (int)Height.Throw);
        Debug.Log (height);
      }

      // *** handle height changes
			
      if (control_state.jump)
        rbody.AddForce (jumpForce * Vector3.up);
    }

    MoveXZ (control_state.moveInXZ.normalized);
	}

  void MoveXZ (Vector3 move) {
    // Adds the force to the player, but then imposes a max speed

    if (rbody.velocity.magnitude == 0) gait = Gait.Static;
    else if (rbody.velocity.magnitude < runningSpeed) gait = Gait.Walk;
    else gait = Gait.Run;
    float moveForce = (gait == Gait.Run) ? runningMoveForce : walkingMoveForce;
            
    // convert from world space to local/object space
		move = moveForce * transform.InverseTransformDirection(move);
		//move = Vector3.ProjectOnPlane(move, groundNormal);

    // apply the force and impose maximum speed (only in XZ plane)
    rbody.AddForce (move);

    Vector3 vXZ = Vector3.Scale(rbody.velocity, new Vector3(1,0,1));
    if (vXZ.magnitude > maxSpeed) {
      vXZ = vXZ.normalized * maxSpeed;
      rbody.velocity = new Vector3(vXZ.x, rbody.velocity.y, vXZ.z);
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
