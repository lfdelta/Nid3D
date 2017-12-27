using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class CharController : MonoBehaviour {

  public Text stateText;

  // concerning the state of the character
  private bool isGrounded;
	private enum Height {Low, Mid, High, Throw};
  private enum FSM {Fence, Run, Jump, BunnyHop, Roll,
                    LedgeGrab, Stunned};

  public float walkingSpeed = 0.01f;
  public float runningSpeed = 2; // speed at which player isRunning
  //public float maxSpeed = 5; // maximum speed for player
  float sqrWalkingSpeed, sqrRunningSpeed;//, sqrMaxSpeed;
  public float moveForce = 50;
	//public float walkingMoveForce = 200;
  //public float runningMoveForce = 100;
	public float jumpForce = 500;
	public float groundCheckDist = 0.1f;
  public float directionChangeThreshold = 45;
  public float vMaxSlope = 1;
  public float frictionCoefficient = 1;
  public float dragSlope = 1;
	public Vector3 originToFeet = 1f * Vector3.down;

	private Rigidbody rbody;
  private Animator animator;
	private CapsuleCollider capsule;
 	private Vector3 capsuleCenter;
	private float capsuleHeight;
	private Height height;
  private FSM playerState;
	private Vector3 groundNormal;
  private ControlState controlState;



	void Start () {
    sqrWalkingSpeed = walkingSpeed * walkingSpeed;
    sqrRunningSpeed = runningSpeed * runningSpeed;
    //sqrMaxSpeed = maxSpeed * maxSpeed;

    animator = GetComponent<Animator> ();
		rbody = GetComponent<Rigidbody> ();
		rbody.constraints = RigidbodyConstraints.FreezeRotation;

		capsule = GetComponent<CapsuleCollider> ();
		capsuleCenter = capsule.center;
		capsuleHeight = capsule.height;

		height = Height.Mid;
    if (stateText) stateText.text = "";
    playerState = FSM.Fence;
    controlState = new ControlState ();
	}



	public void UpdateCharacter (ControlState newControlState) {
    controlState = newControlState;
    
    CheckGround ();

    switch (playerState) {
      case FSM.Fence:
        DoFence ();
        break;
      case FSM.Run:
        DoRun ();
        break;
      case FSM.Jump:
        DoJump ();
        break;
    }
    if (stateText) stateText.text = playerState.ToString ();
	}



  float VMax () {
    return vMaxSlope * controlState.moveInXZ.magnitude;
  }

  float SqrVMax () {
    return vMaxSlope * vMaxSlope * controlState.moveInXZ.sqrMagnitude;
  }

  float Friction (float sqrV) {
    return frictionCoefficient * sqrV;
  }

  float Drag (float sqrV) {
    return dragSlope * (sqrV - SqrVMax ());
  }



  // we'll need to overhaul this thing to handle turning implicitly
  void MoveXZ (Vector3 move) {
    //Vector3 moveXZ = runningMoveForce * transform.InverseTransformDirection (move);
    //moveXZ = Vector3.ProjectOnPlane (move, groundNormal);

    float sqrV = rbody.velocity.sqrMagnitude;
    float inputDotVel = Vector3.Dot (controlState.moveInXZ, rbody.velocity);
    Vector3 ihat = controlState.moveInXZ.normalized;
    Vector3 vhat = rbody.velocity.normalized;

    Vector3 moveForceVec;
    if (controlState.moveInXZ.sqrMagnitude == 0) {
      moveForceVec = -Friction(rbody.velocity.sqrMagnitude) * vhat;
    } else if (sqrV < SqrVMax() || inputDotVel < 0) {
      moveForceVec = moveForce * ihat;
    } else {
      // centripetal force (player input perpendicular to velocity) plus drag
      Vector3 perpInput = controlState.moveInXZ - inputDotVel * rbody.velocity;
      moveForceVec = moveForce * perpInput - Drag(sqrV) * vhat;
    }

    rbody.AddForce (moveForceVec);

    /*
    Vector3 vXZ = Vector3.ProjectOnPlane (rbody.velocity, groundNormal);
    if (vXZ.sqrMagnitude > sqrMaxSpeed) {
      rbody.AddForce(-dragSlope * rbody.velocity.normalized);
    }
    */

    /*
    // Adds the force to the player, but then imposes a max speed
    float moveForce = (playerState == FSM.Run) ? runningMoveForce : walkingMoveForce;
		move = moveForce * transform.InverseTransformDirection(move);
		move = Vector3.ProjectOnPlane(move, groundNormal);

    // apply the force and impose maximum speed (only in XZ plane)
    rbody.AddForce (move);

    Vector3 vXZ = Vector3.Scale(rbody.velocity, new Vector3(1,0,1));
    if (vXZ.sqrMagnitude > sqrMaxSpeed) {
      vXZ = vXZ.normalized * maxSpeed; //** replace this with a force ~ -vXZ
      rbody.velocity = new Vector3(vXZ.x, rbody.velocity.y, vXZ.z);
    }
    */
  }



  void ChangeState(FSM state) {
    switch (state) {
    case FSM.Run:
      rbody.velocity = Vector3.zero;
      break;
    case FSM.Jump:
      rbody.AddForce (jumpForce * Vector3.up);
      break;
    }

    playerState = state;

    /*int fsmint;
    switch(state) {
    case FSM.Fence:
      fsmint = 0;
      break;
    case FSM.Run:
      fsmint = 1;
      break;
    default:
      fsmint = 0;
      break;
    }
    animator.SetInteger ("State", fsmint);*/
  }



  void DoFence () {
    // handle movement (do first)
    MoveXZ(controlState.moveInXZ);

    // if v > runspeed, FSM->run
    if (rbody.velocity.sqrMagnitude > sqrRunningSpeed)
      ChangeState (FSM.Run);
    if (isGrounded && controlState.jump)
      ChangeState (FSM.Jump);

    // handle sword height
    if (controlState.heightChange != 0) {
      height += controlState.heightChange;
      height = (Height)Tools.Clamp ((int)height, (int)Height.Low, (int)Height.Throw);
      Debug.Log (height);
    }

    animator.SetInteger ("State", 0);
  }



  void DoRun () {
    // apply force (do first)
    MoveXZ (controlState.moveInXZ);

    /*if (DirectionChange ()) {
      Debug.Log("Direction Change");
      rbody.velocity = Vector3.zero;
      playerState = FSM.Fence;
    }*/
    if (isGrounded && controlState.jump)
      ChangeState (FSM.Jump);
    if (rbody.velocity.sqrMagnitude < sqrWalkingSpeed)
      // maybe change this so < runningSpeed and decelerating?
      playerState = FSM.Fence;

    animator.SetInteger ("State", 1);
  }



  void DoJump () {
    MoveXZ (controlState.moveInXZ);
    // called every update during jump state

    /*if (DirectionChange ()) { // do we want this behavior in midair?
      Debug.Log("Direction Change");
      rbody.velocity = new Vector3(0, rbody.velocity.y, 0);
    }*/

    if (isGrounded && rbody.velocity.sqrMagnitude > sqrRunningSpeed)
      playerState = FSM.Run;
    else if (isGrounded)
      playerState = FSM.Fence;
  }



  bool DirectionChange() {
    // Returns true if controlState is changing the direction of the character in XZ
    //if (playerState != FSM.Run) return false; // redundancy check; commented for use in DoJump
    Vector3 vXZ = Vector3.ProjectOnPlane(rbody.velocity, Vector3.up);
    float angle = Vector3.Angle(controlState.moveInXZ, vXZ);
    return angle > directionChangeThreshold;
  }



	void CheckGround() {
		RaycastHit info;
		// send raycast from just above the feet
		if (Physics.Raycast (transform.position + originToFeet + 0.1f*Vector3.up,
                         Vector3.down, out info, groundCheckDist)) {
			isGrounded = true;
			groundNormal = info.normal;
		} else {
			isGrounded = false;
			groundNormal = Vector3.up;
		}
	}
}
