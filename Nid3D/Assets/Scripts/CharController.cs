﻿using System.Collections;
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
	public float walkingMoveForce = 200;
  public float runningMoveForce = 100;
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
  private ControlState controlState;
    
	// Use this for initialization
	void Start () {
		rbody = GetComponent<Rigidbody> ();
		rbody.constraints = RigidbodyConstraints.FreezeRotation;

		capsule = GetComponent<CapsuleCollider> ();
		capsuleCenter = capsule.center;
		capsuleHeight = capsule.height;

		height = Height.Mid;
    stateText.text = "";
    playerState = FSM.Fence;
    controlState = new ControlState ();
	}

	public void UpdateCharacter (ControlState newControlState) {
    controlState = newControlState;
    
    CheckGround ();

    if (isGrounded &&controlState.jump)
      //playerState = FSM.Jump;
      rbody.AddForce (jumpForce * Vector3.up);

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
    stateText.text = playerState.ToString ();
	}

  void MoveXZ (Vector3 move) {
    // Adds the force to the player, but then imposes a max speed
    float moveForce = (playerState == FSM.Run) ? runningMoveForce : walkingMoveForce;
		move = moveForce * transform.InverseTransformDirection(move);
		move = Vector3.ProjectOnPlane(move, groundNormal);

    // apply the force and impose maximum speed (only in XZ plane)
    rbody.AddForce (move);

    Vector3 vXZ = Vector3.Scale(rbody.velocity, new Vector3(1,0,1));
    if (vXZ.magnitude > maxSpeed) {
      vXZ = vXZ.normalized * maxSpeed;
      rbody.velocity = new Vector3(vXZ.x, rbody.velocity.y, vXZ.z);
    }
  }

  void DoFence () {
    // if v > runspeed, FSM->run
    // handle movement (do first)
    MoveXZ(controlState.moveInXZ.normalized);

    //if (rbody.velocity.magnitude < walkingSpeed) playerState = FSM.Fence;
    if (rbody.velocity.magnitude > runningSpeed) StartRun ();
    if (isGrounded && controlState.jump) {
      Debug.Log("Jumping");
      StartJump();
    }

    // handle sword height
    if (controlState.heightChange != 0) {
      height += controlState.heightChange;
      height = (Height)Tools.Clamp ((int)height, (int)Height.Low, (int)Height.Throw);
      Debug.Log (height);
    }

  }

  void StartRun () {
    // called for transition TO Run from some other state
    rbody.velocity = Vector3.zero;
    playerState = FSM.Run;
  }
  
  void DoRun () {
    // apply force (do first)
    MoveXZ (controlState.moveInXZ.normalized);

    if (DirectionChange ()) {
      Debug.Log("Direction Change");
      rbody.velocity = Vector3.zero;
      playerState = FSM.Fence;
    }
    if (isGrounded && controlState.jump) StartJump();
    if (rbody.velocity.magnitude < walkingSpeed)
      // maybe change this so < runningSpeed and decelerating?
      playerState = FSM.Fence;
  }

  void StartJump () {
    // called when some state Transitions TO jump, not every update
    playerState = FSM.Jump;
    rbody.AddForce (jumpForce * Vector3.up);
  }

  void DoJump () {
    MoveXZ (controlState.moveInXZ.normalized);
    // called every update during jump state

    if (isGrounded && rbody.velocity.magnitude > runningSpeed)
      playerState = FSM.Run;
    else if (isGrounded)
      playerState = FSM.Fence;
  }
  
  bool DirectionChange() {
    // Returns true if controlState is changing the direction of the character
    // in XZ
    if (playerState != FSM.Run) return false;
    Vector3 vXZ = Vector3.ProjectOnPlane(rbody.velocity, Vector3.up);
    float angle = Vector3.Angle(controlState.moveInXZ, vXZ);
    return angle > directionChangeThreshold;
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
