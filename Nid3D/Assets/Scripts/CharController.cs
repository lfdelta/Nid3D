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

  // concerning the state of the character
	private enum Height {Low, Mid, High};
  private enum FSM {Fence, Stab, Run, Jump, BunnyHop,
                   Roll, LedgeGrab, Stunned, Dead};

  public PlayerID playerid;
  public float walkingSpeed = 0.01f;
  public float runningSpeed = 2; // speed at which player isRunning
  public float rotationSpeed = 12;
  float sqrWalkingSpeed, sqrRunningSpeed;
  public float moveForce = 50;
	public float jumpForce = 500;
  public float vMaxSlope = 1;
  public float frictionCoefficient = 1;
  public float dragSlope = 1;
  public float respawnTime = 1;
  public float respawnDistance = 1;
  public Object swordPrefab;
  public Object shadowPrefab;

  [HideInInspector] public bool isDead;

  private bool isGrounded;
  private Vector3 vXZ;
	private Rigidbody rbody;
  private Animator animator;
  private SkinnedMeshRenderer meshRender;
	private CapsuleCollider capsule;
	private Height height;
  private FSM playerState;
	//private Vector3 groundNormal;
  private PlayerControlState controlState;
  private GameObject[] otherplayers;
  private float deathTime, stabTime;
  private GameController gameController;

  private bool tryToThrowSword;
  private bool tryToCrouch;

  private Sword attachedSword;
  private float swordHeightIncrement = 4;
  private Vector3 swordInitPos = new Vector3 (0, 16.8f, -9.9f);
  private Quaternion swordLocalRot = Quaternion.Euler (new Vector3 (90, 0, 0));
  private Vector3 tryThrowSwordPos;
  private Quaternion tryThrowSwordRot = Quaternion.identity;
  private Vector3 swordLocalScale = new Vector3 (1, 5, 1);
  private CheckForSwords swordChecker;



	void Awake () {
    sqrWalkingSpeed = walkingSpeed * walkingSpeed;
    sqrRunningSpeed = runningSpeed * runningSpeed;

    animator = GetComponent<Animator> ();
		rbody = GetComponent<Rigidbody> ();
    rbody.constraints = RigidbodyConstraints.FreezeRotation;

    meshRender = GetComponent<SkinnedMeshRenderer> ();
		capsule = GetComponent<CapsuleCollider> ();

		height = Height.Mid;
    playerState = FSM.Fence;
    isDead = false;
    controlState = new PlayerControlState ();

    otherplayers = GetOtherPlayers ();

    gameController = FindObjectOfType<GameController>();

    swordChecker = GetComponentInChildren<CheckForSwords> ();
    AttachNewSword ();
    tryToThrowSword = false;
    tryToCrouch = false;
    tryThrowSwordPos = swordInitPos + 2 * swordHeightIncrement * Vector3.up;

    /*Object sh = Instantiate (shadowPrefab);
    CastShadow csh = ((GameObject)sh).GetComponent<CastShadow> ();
    csh.casterPos = transform;*/
	}



	public void UpdateCharacter (PlayerControlState newControlState) {
    controlState = newControlState;

    //vXZ = Vector3.ProjectOnPlane(rbody.velocity, groundNormal);
    vXZ = Vector3.ProjectOnPlane(rbody.velocity, Vector3.up);

    tryToThrowSword = (controlState.heightHeldLongEnough && controlState.heightHold == 1);
    tryToCrouch = (controlState.heightHeldLongEnough && controlState.heightHold == -1);

    // handle sword height, if a sword is attached
    if (attachedSword) {
      if (controlState.heightChange != 0) {
        height += controlState.heightChange;
        height = (Height)Tools.Clamp ((int)height, (int)Height.Low, (int)Height.High);
      }

      attachedSword.SendMessage ("Activate", (playerState == FSM.Fence && !tryToThrowSword) || playerState == FSM.Stab); // this might be very slow

      if (tryToThrowSword) {
        attachedSword.transform.localPosition = tryThrowSwordPos;
        attachedSword.transform.localRotation = tryThrowSwordRot;
      } else {
        Vector3 pos = attachedSword.transform.localPosition;
        Vector3 newpos = new Vector3 (pos.x, SwordHeightPos (), pos.z);
        //** there are probably more natural-looking interpolation curves for this than asymptotic-exponential
        attachedSword.transform.localPosition = Vector3.Lerp (pos, newpos, 30 * Time.deltaTime);
        attachedSword.transform.localRotation = swordLocalRot;

        bool isMoving = Mathf.Abs (pos.y - newpos.y) > 0.01 * swordHeightIncrement;
        attachedSword.SetDisarmStatus (isMoving);
      }
    } else if (controlState.heightChange == -1) { // try to pick up a sword if you aren't holding one
      Sword s = swordChecker.FirstElement();
      if (s != null)
        AttachSword (s);
    }

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
      capsule.enabled = false;
      meshRender.enabled = false;
      DropSword (false);
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
    AttachNewSword();
    height = Height.Mid;
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
    s.transform.localRotation = swordLocalRot;
    s.transform.localScale = swordLocalScale;
    s.ChangeOwnership(playerid);

    swordChecker.enabled = false;
  }

  void AttachNewSword() {
    Object s = Instantiate (swordPrefab);
    AttachSword (((GameObject)s).GetComponent<Sword>());
  }

  // parameter tells whether the player was disarmed or killed, determining the sword's resulting motion
  void DropSword(bool disarmed) {
    if (attachedSword != null) {
      // uncouple the sword from this player
      attachedSword.ChangeOwnership (null);
      attachedSword.transform.parent = null;

      // set the sword's angular and translational velocity
      if (disarmed) {
        attachedSword.rbody.AddForce (20 * Vector3.up + 7 * transform.forward, ForceMode.VelocityChange);
        attachedSword.rbody.AddTorque (200 * attachedSword.transform.right, ForceMode.VelocityChange);
      }
        
      attachedSword = null;
      swordChecker.enabled = true;
    }
  }

  float SwordHeightPos () {
    float init = swordInitPos.y;
    switch (height) {
    case Height.Low:
      return init - swordHeightIncrement;
    default:
    case Height.Mid:
      return init;
    case Height.High:
      return init + swordHeightIncrement;
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

    if (controlState.attack && attachedSword) {
      if (tryToThrowSword)
        ThrowSword ();
      else if (isGrounded)
        ChangeState (FSM.Stab);
    }
  }
    
  void ThrowSword() {

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
    
    if (controlState.attack && attachedSword) {
      if (tryToThrowSword)
        ThrowSword ();
      else if (isGrounded)
        ChangeState (FSM.Stab);
    }
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

    if (controlState.attack && tryToThrowSword && attachedSword)
      ThrowSword ();
  }
    
  void DoDead() {
    // coordinate with the game controller to choose a respawn location
    if (Time.time - deathTime >= respawnTime)
      Respawn (gameController.PlayerRespawnLoc(playerid, respawnDistance));
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
    


  // called by CheckGrounded child object
  void SetGrounded(bool grounded) {
    isGrounded = grounded;
    rbody.useGravity = !grounded;
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
