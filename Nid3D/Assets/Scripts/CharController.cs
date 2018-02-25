using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TeamUtility.IO;



public class PlayerAlive {
  public PlayerID playerid;
  public bool alive;

  public PlayerAlive(PlayerID id, bool l) {
    playerid = id;
    alive = l;
  }
}



[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class CharController : MonoBehaviour {

  public Text stateText;

  // concerning the state of the character
  private bool isGrounded;
	private enum Height {Low, Mid, High, Throw};
  private enum FSM {Fence, Stab, Run, Jump, BunnyHop,
                   Roll, LedgeGrab, Stunned, Dead};

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
  public float respawnTime = 1;
  public Object swordPrefab;

  [HideInInspector] public bool isDead;

  private Vector3 vXZ;
	private Rigidbody rbody;
  private Animator animator;
  private SkinnedMeshRenderer meshRender;
	private CapsuleCollider capsule;
  private Vector3 originToFeet = 0.05f * Vector3.down; // vector for player mesh
	private Height height;
  private FSM playerState;
	private Vector3 groundNormal;
  private PlayerControlState controlState;
  private GameObject[] otherplayers;
  private float deathTime, stabTime;
  private GameController gameController;
  private Sword attachedSword;
  private Vector3 swordInitPos = new Vector3(0, 16.8f, -9.9f);



	void Start () {
    sqrWalkingSpeed = walkingSpeed * walkingSpeed;
    sqrRunningSpeed = runningSpeed * runningSpeed;

    animator = GetComponent<Animator> ();
		rbody = GetComponent<Rigidbody> ();
    rbody.constraints = RigidbodyConstraints.FreezeRotation;

    meshRender = GetComponent<SkinnedMeshRenderer> ();
		capsule = GetComponent<CapsuleCollider> ();

		height = Height.Mid;
    if (stateText) stateText.text = "";
    playerState = FSM.Fence;
    isDead = false;
    controlState = new PlayerControlState ();

    otherplayers = GetOtherPlayers ();

    gameController = FindObjectOfType<GameController>();

    Object s = Instantiate (swordPrefab);
    AttachSword (((GameObject)s).GetComponent<Sword>());
	}



	public void UpdateCharacter (PlayerControlState newControlState) {
    controlState = newControlState;
    
    CheckGround ();
    vXZ = Vector3.ProjectOnPlane(rbody.velocity, groundNormal);

    switch (playerState) {
    case FSM.Fence:
      LookAtNearestPlayer();
      DoFence ();
      break;
    case FSM.Stab:
      //LookAtLastVelocity ();
      LookAtNearestPlayer();
      DoStab ();
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
    case FSM.Fence:
      animator.SetInteger ("State", 0);
      //animator.Play ("FenceIdle");
      break;
    case FSM.Stab:
      stabTime = Time.time;
      break;
    case FSM.Run:
      animator.SetInteger ("State", 1);
      //animator.Play ("Run");
      break;
    case FSM.Jump:
      rbody.AddForce (jumpForce * Vector3.up);
      break;
    case FSM.Dead:
      isDead = true;
      rbody.velocity = Vector3.zero;
      deathTime = Time.time;
      //capsule.enabled = false;
      meshRender.enabled = false;
      gameController.SendMessage ("PlayerIsAlive", new PlayerAlive(playerid, false));
      break;
    default:
      break;
    }

    playerState = state;
  }



  // Die() and Respawn() exist to be called externally via SendMessage
  void Die () {
    // handle all state-transition factors in ChangeState
    ChangeState (FSM.Dead);
  }

  void Respawn (Vector3 spawnLoc) {
    // handle state-transition factors here, because there is no FSM.Respawn
    isDead = false;
    transform.position = spawnLoc;
    rbody.velocity = Vector3.zero;
    capsule.enabled = true;
    meshRender.enabled = true;
    attachedSword.transform.localPosition = swordInitPos;
    gameController.SendMessage ("PlayerIsAlive", new PlayerAlive(playerid, true));
    ChangeState (FSM.Fence);
  }



  // maximum velocity is based on the magnitude of player input
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
    float inputDotVel = Vector3.Dot (move, vXZ);
    Vector3 ihat = move.normalized;
    Vector3 vhat = vXZ.normalized;

    Vector3 moveForceVec;
    if (move.sqrMagnitude == 0) {
      moveForceVec = -Friction(v) * vhat;
    } else if (sqrV < SqrVMax() || inputDotVel < 0) {
      moveForceVec = moveForce * ihat;
    } else {
      // ~centripetal force (player input perpendicular to velocity) plus drag
      Vector3 perpInput = move - inputDotVel/sqrV * vXZ;
      moveForceVec = moveForce * perpInput - Drag(v) * vhat;
    }

    rbody.AddForce (moveForceVec);
  }
 


  void AttachSword(Sword s) {
    attachedSword = s;
    s.transform.parent = transform;
    s.transform.localPosition = swordInitPos;
    s.transform.localRotation = Quaternion.Euler (new Vector3 (90, 0, 0));
    s.transform.localScale = new Vector3 (1, 5, 1);
    s.thisPlayer = playerid;
  }

  void DropSword() {
    //** set sword's angular and translational velocity
    attachedSword.thisPlayer = null;
    attachedSword.transform.parent = null;

    attachedSword = null;
  }

  float SwordHeightPos () {
    float init = swordInitPos.y;
    switch (height) {
    case Height.Low:
      return init - 4;
    default:
    case Height.Mid:
      return init;
    case Height.High:
      return init + 4;
    case Height.Throw:
      return init + 8;
    }
  }



  void DoFence () {
    // handle movement (do first)
    MoveXZ(controlState.moveInXZ);

    // if v > runspeed, FSM->run
    if (vXZ.sqrMagnitude > sqrRunningSpeed && Vector3.Dot(vXZ, controlState.moveInXZ) > 0)
      ChangeState (FSM.Run);
    if (isGrounded && controlState.jump)
      ChangeState (FSM.Jump);

    // handle sword height, if a sword is attached
    if (controlState.heightChange != 0 && attachedSword) {
      height += controlState.heightChange;
      height = (Height)Tools.Clamp ((int)height, (int)Height.Low, (int)Height.Throw);
      Vector3 pos = attachedSword.transform.localPosition;
      attachedSword.transform.localPosition = new Vector3(pos.x, SwordHeightPos (), pos.z);
    }

    if (controlState.attack && isGrounded && attachedSword)
      ChangeState (FSM.Stab);
  }
    
  // initiate a sequence of nested helper functions
  void DoStab() {
    MoveXZ (Vector3.zero); // ignore player inputs while stabbing

    float dSword = StabAnimation(Time.time);
    Vector3 pos = attachedSword.transform.localPosition;

    if (dSword > 0) {
      attachedSword.transform.localPosition = new Vector3 (pos.x, pos.y, swordInitPos.z - dSword);
    } else {
      attachedSword.transform.localPosition = new Vector3(swordInitPos.x, pos.y, swordInitPos.z);
      ChangeState (FSM.Fence);
    }
  }

  // returns the sword's horizontal deviation as a function of time
  // must return >0 until stab is finished, by construction of DoStab()
  float StabAnimation(float t) {
    float dt = t - stabTime;
    return AsymmetricLinearStab (12, 0.1f, 0.15f, dt);
  }

  float AsymmetricLinearStab(float maxDist, float upTime, float downTime, float dt) {
    if (dt < upTime) {
      float slope = maxDist / upTime;
      return slope * dt;
    } else {
      float slope = maxDist / downTime;
      return maxDist - slope * (dt - upTime);
    }
  }

  void DoRun () {
    // apply force (do first)
    MoveXZ (controlState.moveInXZ);

    if (isGrounded && controlState.jump)
      ChangeState (FSM.Jump);
    if (vXZ.sqrMagnitude < sqrWalkingSpeed)
      ChangeState (FSM.Fence);
    if (controlState.attack && attachedSword)
      ChangeState (FSM.Stab);
  }

  void DoJump () {
    MoveXZ (controlState.moveInXZ);
    // called every update during jump state
    if (isGrounded) {
      if (vXZ.sqrMagnitude > sqrRunningSpeed)
        ChangeState (FSM.Run);
      else
        ChangeState (FSM.Fence);
    }
  }

  void DoDead() {
    if (Time.time - deathTime >= respawnTime)
      Respawn (transform.position); //** in the future, coordinate with the camera/game controller to choose a location
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
    float mindistance = Mathf.Infinity;
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
    if (vXZ.sqrMagnitude > 0.01)
      PointCharacter (vXZ);
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



  // return an array of all other CharControllers in the scene
  GameObject[] GetOtherPlayers() {
    Object[] allChars = Object.FindObjectsOfType(typeof(CharController));
    GameObject[] others = new GameObject[allChars.Length - 1];

    int j = 0;
    for (int i = 0; i < allChars.Length; i++) {
      if (allChars [i] != this)
        others [j++] = ((CharController)allChars [i]).gameObject;
    }
    return others;
  }
}
