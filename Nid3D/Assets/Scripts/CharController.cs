using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class CharController : MonoBehaviour {

  // concerning the state of the character
  private bool isGrounded;
	private enum Height {Low, Mid, High, Throw};
  private enum FSM {Idle, Jerk, Walk, Run,
                    Crouch, Jump, BunnyHop, Stab,
                    DiveKick, Punch, Roll,
                    Cartwheel, SweepKick,
                    LedgeGrab, Stunned};

	public float walkingMoveForce = 200;
  public float runningMoveForce = 100;
  public float runningSpeed = 2; // speed at which player isRunning
  public float maxSpeed = 5;     // maximum speed for player
	public float jumpForce = 500;
	public float groundCheckDist = 0.1f;
  public float directionChangeThreshold = 45;
	public Vector3 origToFeet = 1f * Vector3.down;

	private Rigidbody rbody;
	private CapsuleCollider capsule;
 	private Vector3 capsuleCenter;
	private float capsuleHeight;
	private Height height;
  private FSM playerState;
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

    if (rbody.velocity.magnitude == 0) playerState = FSM.Idle;
    else if (rbody.velocity.magnitude < runningSpeed) playerState = FSM.Walk;
    else playerState = FSM.Run;
    float moveForce = (playerState == FSM.Run) ? runningMoveForce : walkingMoveForce;
       
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
  
  bool switchState(FSM newPlayerState) {
    // switches the playerState to newPlayerState if the switch is allowed
    // returns true on success

    switch (playerState) {
      case FSM.Idle:      return switchFromIdle      (newPlayerState);
      case FSM.Jerk:      return switchFromJerk      (newPlayerState);
      case FSM.Walk:      return switchFromWalk      (newPlayerState);
      case FSM.Run:       return switchFromRun       (newPlayerState);
      case FSM.Crouch:    return switchFromCrouch    (newPlayerState);
      case FSM.Jump:      return switchFromJump      (newPlayerState);
      case FSM.BunnyHop:  return switchFromBunnyHop  (newPlayerState);
      case FSM.Stab:      return switchFromStab      (newPlayerState);
      case FSM.DiveKick:  return switchFromDiveKick  (newPlayerState);
      case FSM.Punch:     return switchFromPunch     (newPlayerState);
      case FSM.Roll:      return switchFromRoll      (newPlayerState);
      case FSM.Cartwheel: return switchFromCartwheel (newPlayerState);
      case FSM.SweepKick: return switchFromSweepKick (newPlayerState);
      case FSM.LedgeGrab: return switchFromLedgeGrab (newPlayerState);
      case FSM.Stunned:   return switchFromStunned   (newPlayerState);
    }
  }

  void switchFromIdle(FSM newPlayerState) {
    switch (newPlayerState) {
      case FSM.Idle:
        
  }
  void switchFromJerk(FSM newPlayerState) {
  }
  void switchFromWalk(FSM newPlayerState) {
  }
  void switchFromRun(FSM newPlayerState) {
  }
  void switchFromCrouch(FSM newPlayerState) {
  }
  void switchFromJump(FSM newPlayerState) {
  }
  void switchFromBunnyHop(FSM newPlayerState) {
  }
  void switchFromStab(FSM newPlayerState) {
  }
  void switchFromDiveKick(FSM newPlayerState) {
  }
  void switchFromPunch(FSM newPlayerState) {
  }
  void switchFromRoll(FSM newPlayerState) {
  }
  void switchFromCartwheel(FSM newPlayerState) {
  }
  void switchFromSweepKick(FSM newPlayerState) {
  }
  void switchFromLedgeGrab(FSM newPlayerState) {
  }
  void switchFromStunned(FSM newPlayerState) {
  }
    
  bool directionChange() {
    // Returns true if controlState is changing the direction of the character
    // in XZ
    Vector3 vXZ = Vector3.ProjectOnPlane(rbody.velocity, Vector3.up);
    float angle = Vector3.angle(control_state.moveInXZ, vXZ);
    return angle > directionChangeThreshold
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
