using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TeamUtility.IO;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class CharController : MonoBehaviour {

  public Text stateText;

  // concerning the state of the character
  private bool isGrounded;
	private enum Height {Low, Mid, High, Throw};
  private enum FSM {Fence, Run, Jump, BunnyHop, Roll,
                   LedgeGrab, Stunned, Dead};

  public PlayerID playerid;
  public float walkingSpeed = 0.01f;
  public float runningSpeed = 2; // speed at which player isRunning
  public float rotationSpeed = 12;
  float sqrWalkingSpeed, sqrRunningSpeed;
  public float moveForce = 50;
	public float jumpForce = 500;
	public float groundCheckDist = 0.1f;
  public float vMaxSlope = 1;
  public float frictionCoefficient = 1;
  public float dragSlope = 1;
  public Vector3 originToFeet = 1f * Vector3.down; // vector for player mesh
  public float respawnTime = 1;

  private Vector3 vXZ;
	private Rigidbody rbody;
  private Animator animator;
	private CapsuleCollider capsule;
 	private Vector3 capsuleCenter;
	private float capsuleHeight;
	private Height height;
  private FSM playerState;
	private Vector3 groundNormal;
  private ControlState controlState;
  private GameObject[] otherplayers;
  private float deathTime;



	void Start () {
    sqrWalkingSpeed = walkingSpeed * walkingSpeed;
    sqrRunningSpeed = runningSpeed * runningSpeed;

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

    otherplayers = GetOtherPlayers (4);
	}



	public void UpdateCharacter (ControlState newControlState) {
    controlState = newControlState;
    
    CheckGround ();
    vXZ = Vector3.ProjectOnPlane(rbody.velocity, groundNormal);

    switch (playerState) {
    case FSM.Fence:
      LookAtNearestPlayer();
      DoFence ();
      break;
    case FSM.Run:
      LookAtLastVelocity ();
      DoRun ();
      break;
    case FSM.Jump:
      LookAtLastVelocity();
      DoJump ();
      break;
    case FSM.Dead:
      DoDead ();
      break;
    }

    if (stateText) stateText.text = playerState.ToString ();
  }

  void ChangeState(FSM state) {
    switch (state) {
    case FSM.Jump:
      rbody.AddForce (jumpForce * Vector3.up);
      break;
    case FSM.Dead:
      deathTime = Time.time;
      break;
    }

    playerState = state;
  }



  // these exist to be called externally via SendMessage
  void Die () {
    ChangeState (FSM.Dead);
  }

  void Respawn (Vector3 spawnLoc) {
    transform.position = spawnLoc;
    rbody.velocity = Vector3.zero;
    ChangeState (FSM.Fence);
  }



  float VMax () {
    return vMaxSlope * controlState.moveInXZ.magnitude;
  }

  float SqrVMax () {
    return vMaxSlope * vMaxSlope * controlState.moveInXZ.sqrMagnitude;
  }

  float Friction (float v) {
    return frictionCoefficient * v;
  }

  float Drag (float v) {
    return dragSlope * (v - VMax ());
  }



  void MoveXZ (Vector3 move) {
    float v = vXZ.magnitude;
    float sqrV = vXZ.sqrMagnitude;
    float inputDotVel = Vector3.Dot (controlState.moveInXZ, vXZ);
    Vector3 ihat = controlState.moveInXZ.normalized;
    Vector3 vhat = vXZ.normalized;

    Vector3 moveForceVec;
    if (controlState.moveInXZ.sqrMagnitude == 0) {
      moveForceVec = -Friction(v) * vhat;
    } else if (sqrV < SqrVMax() || inputDotVel < 0) {
      moveForceVec = moveForce * ihat;
    } else {
      // centripetal force (player input perpendicular to velocity) plus drag
      Vector3 perpInput = controlState.moveInXZ - inputDotVel/sqrV * vXZ;
      moveForceVec = moveForce * perpInput - Drag(v) * vhat;
    }

    rbody.AddForce (moveForceVec);
  }



  void DoFence () {
    // handle movement (do first)
    MoveXZ(controlState.moveInXZ);

    // if v > runspeed, FSM->run
    if (vXZ.sqrMagnitude > sqrRunningSpeed)
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

    if (isGrounded && controlState.jump)
      ChangeState (FSM.Jump);
    if (vXZ.sqrMagnitude < sqrWalkingSpeed)
      // maybe change this so < runningSpeed and decelerating?
      playerState = FSM.Fence;

    animator.SetInteger ("State", 1);
  }

  void DoJump () {
    MoveXZ (controlState.moveInXZ);
    // called every update during jump state
    if (isGrounded && vXZ.sqrMagnitude > sqrRunningSpeed)
      playerState = FSM.Run;
    else if (isGrounded)
      playerState = FSM.Fence;
  }

  void DoDead() {
    if (Time.time - deathTime >= respawnTime)
      Respawn (transform.position); //** in the future, coordinate with the camera to choose a location
  }



  Quaternion PointCharacter(Vector3 newdir, bool instant = false) {
    newdir = new Vector3 (-newdir.x, 0, -newdir.z);

    if (instant) {
      rbody.transform.rotation = Quaternion.LookRotation(newdir);
    } else {
      rbody.transform.rotation = Quaternion.Slerp (rbody.transform.rotation, Quaternion.LookRotation(newdir), rotationSpeed*Time.deltaTime);
    }

    return rbody.transform.rotation;
  }

  void LookAtNearestPlayer() {
    float mindistance = 9999999999;
    float newdistance = 0;
    int minindex = -1;
    for (int i = 0; i < otherplayers.Length; i++) {
      if (otherplayers [i] == null)
        continue;
      newdistance = Vector3.Distance (otherplayers [i].transform.position, this.gameObject.transform.position);
      if (newdistance <= mindistance) {
        mindistance = newdistance;
        minindex = i;
      }
    }
    PointCharacter (otherplayers[minindex].transform.position - transform.position);
  }

  void LookAtLastVelocity() {
    Vector3 v = rbody.velocity;
    if (v.sqrMagnitude > 0)
      PointCharacter (v);
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



  GameObject[] GetOtherPlayers(int totalplayers) {
    GameObject[] others = new GameObject[totalplayers-1];
    int j = 0;
    Object[] allcharcontrollers = Object.FindObjectsOfType(typeof(CharController));
    for (int i = 0; i < allcharcontrollers.Length; i++) {
      if (allcharcontrollers [i] == this) {
        print (((CharController)allcharcontrollers [i]).gameObject.GetComponent<CharInput> ().playerID);
      } else if (j < others.Length) {
        others [j] = ((CharController)allcharcontrollers [i]).gameObject;
        j++;
      } else {
        break;
      }
    }
    if (allcharcontrollers.Length <= others.Length) {
      for (int i = allcharcontrollers.Length; i < others.Length; i++) {
        others [i] = null;
      }
    }
    return others;
  }
}
