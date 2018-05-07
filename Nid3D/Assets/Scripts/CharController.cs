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
  private enum FSM {Fence, Stab, Run, Jump,
                    Crouch, Sweepkick, Roll, BunnyHop,
                    Divekick, Punch,
                    Stunned, Dead};

  public PlayerID playerid;
  public float rotationSpeed = 30;
  public float moveForce = 100;
	public float jumpForce = 900;
  public float vMaxSlope = 10;
  public float runSpeedScale = 1.2f;
  public float swordlessSpeedScale = 1.2f;
  public float frictionCoefficient = 10;
  public float dragSlope = 20;
  public float stunMinDuration = 0.5f;
  public float respawnTime = 1;
  public float respawnDistance = 10;
  public Object divekickPrefab;
  public Object swordPrefab;
  public Transform swordHand;
  //public Object shadowPrefab;

  [HideInInspector] public bool isDead;
  [HideInInspector] public bool canBeAffected; // by ChangePlayerStatus

  private bool isGrounded;
  private Vector3 vXZ;
	private Rigidbody rbody;
  private Animator animator;
  private SkinnedMeshRenderer meshRender;
	private CapsuleCollider capsule;
	private Height height;
  private Height startingHeight; // used for disarming
  private FSM playerState;
	//private Vector3 groundNormal;
  private PlayerControlState controlState;
  //private GameObject[] otherplayers;
  private CharController[] otherplayers;
  private float deathTime, stabTime, stunTime;
  private GameController gameController;

  private GameObject divekickHitbox = null;
  private Vector3 divekickLook, divekickMove;

  private bool tryToThrowSword;
  private bool lastFrameThrowSword;
  private bool tryToCrouch;
  private bool swordFollowingHand;

  private Sword attachedSword;
  private float swordHeightIncrement = 5;//4;
  private Vector3 swordInitPos = new Vector3 (0, 16.8f, -9.9f);
  private Quaternion swordLocalRot = Quaternion.Euler (new Vector3 (90, 0, 0));
  private Vector3 tryThrowSwordPos;
  private Quaternion tryThrowSwordRot = Quaternion.Euler (new Vector3 (0, 0, 45));
  private Vector3 swordLocalScale = new Vector3 (1, 5, 1);
  private CheckForSwords swordChecker;



	void Awake () {
    animator = GetComponent<Animator> ();
		rbody = GetComponent<Rigidbody> ();
    rbody.constraints = RigidbodyConstraints.FreezeRotation;

    meshRender = GetComponent<SkinnedMeshRenderer> ();
		capsule = GetComponent<CapsuleCollider> ();

		height = Height.Mid;
    startingHeight = Height.Mid;
    playerState = FSM.Fence;
    isDead = false;
    canBeAffected = true;
    controlState = new PlayerControlState ();

    otherplayers = GetOtherPlayers ();

    gameController = FindObjectOfType<GameController>();

    GenerateDivekick ();

    swordChecker = GetComponentInChildren<CheckForSwords> ();
    AttachNewSword ();
    tryToThrowSword = false;
    tryToCrouch = false;
    tryThrowSwordPos = swordInitPos + (swordHeightIncrement * Vector3.up) + (-5 * Vector3.right);

    /*Object sh = Instantiate (shadowPrefab);
    CastShadow csh = ((GameObject)sh).GetComponent<CastShadow> ();
    csh.casterPos = transform;*/
	}



	public void UpdateCharacter (PlayerControlState newControlState) {
    controlState = newControlState;

    //vXZ = Vector3.ProjectOnPlane(rbody.velocity, groundNormal);
    vXZ = Vector3.ProjectOnPlane(rbody.velocity, Vector3.up);

    lastFrameThrowSword = tryToThrowSword;
    tryToThrowSword = (controlState.heightHeldLongEnough && controlState.heightHold == 1);
    tryToCrouch = (controlState.heightHeldLongEnough && controlState.heightHold == -1);

    // handle sword height, if a sword is attached
    if (attachedSword && !swordFollowingHand) {
      if (controlState.heightChange != 0) {
        height += controlState.heightChange;
        height = (Height)Tools.Clamp ((int)height, (int)Height.Low, (int)Height.High);
      }

      attachedSword.SendMessage ("Activate", playerState == FSM.Stab || (playerState == FSM.Fence && !tryToThrowSword)); // this might be very slow

      if (tryToThrowSword) {
        attachedSword.transform.localPosition = tryThrowSwordPos;
        attachedSword.transform.localRotation = tryThrowSwordRot;
      } else {
        if (lastFrameThrowSword) {
          attachedSword.transform.localPosition = new Vector3(swordInitPos.x, SwordHeightPos(), swordInitPos.z);
          attachedSword.transform.localRotation = swordLocalRot;
        }
        Vector3 pos = attachedSword.transform.localPosition;
        Vector3 newpos = new Vector3 (pos.x, SwordHeightPos (), pos.z);
        //** there are probably more natural-looking interpolation curves for this than asymptotic-exponential
        attachedSword.transform.localPosition = Vector3.Lerp (pos, newpos, 30 * Time.deltaTime);
        attachedSword.transform.localRotation = swordLocalRot;

        bool isMoving = Mathf.Abs (pos.y - newpos.y) > 0.01 * swordHeightIncrement;
        attachedSword.SetDisarmStatus (isMoving, (int)startingHeight);
        if (!isMoving) // if you've finished moving, your starting height is the current height
          startingHeight = height;
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
    case FSM.Run:
      LookAtLastVelocity ();
      DoRun ();
      break;
    case FSM.Jump:
      if (tryToThrowSword)
        LookAtNearestPlayer ();
      else
        LookAtLastVelocity ();
      DoJump ();
      break;
    case FSM.Stab:
      LookAtNearestPlayer();
      DoStab ();
      break;
    case FSM.Divekick:
      //LookAtNearestPlayer ();
      DoDivekick ();
      break;
    case FSM.Stunned:
      DoStunned ();
      break;
    case FSM.Dead:
      DoDead ();
      break;
    }
  }
    
  void ChangeState(FSM state) {
    if (playerState == FSM.Run)
      ChildSwordToHand (false);

    switch (state) {
    case FSM.Fence:
      animator.SetInteger ("State", 0);
      //animator.Play ("FenceIdle");
      break;
    case FSM.Run:
      animator.SetInteger ("State", 1);
      //animator.Play ("Run");
      ChildSwordToHand(true);
      break;
    case FSM.Jump:
      rbody.AddForce (jumpForce * Vector3.up);
      break;
    case FSM.Stab:
      stabTime = Time.time;
      break;
    case FSM.Divekick:
      divekickHitbox.SetActive (true);
      rbody.useGravity = false;

      Vector3 rvec;
      if (!otherplayers [0].isDead)
        rvec = Vector3.ProjectOnPlane(otherplayers [0].transform.position - transform.position, Vector3.up).normalized;
      else
        rvec = controlState.moveInXZ.normalized;
      float hforce = 10, yforce = 20;
      divekickMove = hforce * rvec - yforce * Vector3.up;
      divekickLook = rvec - Vector3.Project (rvec, divekickMove);
      rbody.AddForce (-rbody.velocity, ForceMode.VelocityChange);
      rbody.AddForce (divekickMove, ForceMode.VelocityChange);
      break;
    case FSM.Stunned:
      DropSword ();
      canBeAffected = false;
      swordChecker.active = false;
      divekickHitbox.SetActive (false);
      stunTime = Time.time;
      transform.position = new Vector3 (transform.position.x, transform.position.y + 1, transform.position.z);
      transform.localRotation = Quaternion.AngleAxis (90, transform.right);
      break;
    case FSM.Dead:
      isDead = true;
      canBeAffected = false;
      rbody.velocity = Vector3.zero;
      deathTime = Time.time;
      capsule.enabled = false;
      meshRender.enabled = false;
      swordChecker.active = false;
      if (divekickHitbox)
        divekickHitbox.SetActive (false);
      StartCoroutine(DelayVoidFunction(1, DropSword));
      gameController.SendMessage ("PlayerIsAlive", new PlayerAlive(playerid, false));
      break;
    default:
      break;
    }

    playerState = state;
  }



  // Stun(), Die(), and Respawn() exist to be called externally via SendMessage
  void Stun() {
    ChangeState (FSM.Stunned);
  }

  void Die () {
    // handle all state-transition factors in ChangeState
    ChangeState (FSM.Dead);
  }

  void Respawn (Vector3 spawnLoc) {
    // handle state-transition factors here, because there is no FSM.Respawn
    isDead = false;
    canBeAffected = true;
    transform.position = spawnLoc;
    rbody.velocity = Vector3.zero;
    capsule.enabled = true;
    meshRender.enabled = true;
    AttachNewSword ();
    GenerateDivekick ();
    height = Height.Mid;
    gameController.SendMessage ("PlayerIsAlive", new PlayerAlive(playerid, true));
    ChangeState (FSM.Fence);
  }



  // if running, maximum velocity is a constant, and is faster if the player does not have a sword
  // otherwise, max velocity is based on the magnitude of player input
  float VMax () {
    switch (playerState) {
    case FSM.Run:
      return vMaxSlope * runSpeedScale * (attachedSword ? 1 : swordlessSpeedScale);
    default:
      return vMaxSlope * controlState.moveInXZ.magnitude;
    }
  }

  //** once these values no longer need to be tweaked in-editor, bake squared values in Awake to save computation
  float SqrVMax () {
    switch (playerState) {
    case FSM.Run:
      return vMaxSlope * vMaxSlope * runSpeedScale * runSpeedScale * (attachedSword ? 1 : swordlessSpeedScale * swordlessSpeedScale);
    default:
      return vMaxSlope * vMaxSlope * controlState.moveInXZ.sqrMagnitude;
    }
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

    swordChecker.active = false;
  }

  void AttachNewSword() {
    Object s = Instantiate (swordPrefab);
    AttachSword (((GameObject)s).GetComponent<Sword>());
  }

  void GenerateDivekick() {
    if (divekickHitbox != null)
      return;
    
    Object d = Instantiate(divekickPrefab);
    divekickHitbox = (GameObject)d;

    divekickHitbox.transform.parent = transform;
    divekickHitbox.transform.localPosition = Vector3.zero;
    divekickHitbox.transform.localRotation = Quaternion.Euler (Vector3.zero);
    divekickHitbox.transform.localScale = new Vector3 (10, 10, 10);

    ChangePlayerStatus dkick = divekickHitbox.GetComponent<ChangePlayerStatus> ();
    dkick.affectAllPlayers = false;
    dkick.playerToAffect = Tools.OtherPlayer (playerid);
    dkick.killVStun = false;

    divekickHitbox.SetActive (false);
  }

  void ChildSwordToHand(bool useHand) {
    if (attachedSword == null)
      return;
    
    if (useHand) {
      attachedSword.transform.parent = swordHand;
      attachedSword.transform.localPosition = new Vector3 (-0.109963f, 0.6539122f, 0.3592776f);
      attachedSword.transform.localRotation = Quaternion.Euler (-2.26f, 105.743f, 29.567f);
      attachedSword.transform.localScale = new Vector3 (0.1585785f, 0.7928925f, 0.1585785f);
      attachedSword.SendMessage ("Activate", false);
    } else {
      attachedSword.transform.parent = transform;
      attachedSword.transform.localPosition = swordInitPos;
      attachedSword.transform.localRotation = swordLocalRot;
      attachedSword.transform.localScale = swordLocalScale;
    }

    swordFollowingHand = useHand;
  }
    
  // waits the given number of frames before calling func(arg)
  delegate void VoidFunc();
  IEnumerator DelayVoidFunction(int frames, VoidFunc func) {
    int numSkipped = 0;
    while (numSkipped++ < frames)
      yield return null;

    func ();
  }

  delegate void BoolFunc(bool b);
  IEnumerator DelayBoolFunction(int frames, BoolFunc func, bool arg) {
    int numSkipped = 0;
    while (numSkipped++ < frames)
      yield return null;

    func (arg);
  }

  // uncouple the attached sword, if one exists
  void DropSword() {
    if (attachedSword != null) {
      // uncouple the sword from this player
      attachedSword.ChangeOwnership (null);
      attachedSword.transform.parent = null;
      attachedSword = null;

      if (playerState != FSM.Dead)
        swordChecker.active = true;
      if (playerState == FSM.Stab) // stop the player from getting stuck in Stab
        ChangeState (FSM.Fence);
    }
  }

  // called from opponent's SwordDisarm script
  void Disarm() {
    attachedSword.rbody.AddForce (20 * Vector3.up + 7 * transform.forward, ForceMode.VelocityChange);
    attachedSword.rbody.AddTorque (7 * attachedSword.transform.right, ForceMode.VelocityChange);

    DropSword ();
  }

  // this should only ever be called if we know that a sword is attached
  void ThrowSword() {
    attachedSword.transform.parent = null;
    attachedSword.Throw ();

    Vector3 throwDir = otherplayers [0].transform.position - attachedSword.transform.position;
    throwDir = Vector3.ProjectOnPlane (throwDir, Vector3.up).normalized;
    if (throwDir == Vector3.zero)
      throwDir = transform.forward;
    
    attachedSword.rbody.AddForce (20 * throwDir, ForceMode.VelocityChange);
    attachedSword.rbody.AddTorque (-20 * attachedSword.transform.right, ForceMode.VelocityChange);

    attachedSword = null;
    swordChecker.active = true;
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

    if (controlState.running)
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
    MoveXZ (controlState.moveInXZ);

    if (!controlState.running)
      ChangeState (FSM.Fence);

    if (isGrounded && controlState.jump)
      ChangeState (FSM.Jump);
    
    if (controlState.attack && attachedSword) {
      if (tryToThrowSword)
        ThrowSword ();
      else if (isGrounded)
        ChangeState (FSM.Stab);
    }
  }

  void DoJump () {
    MoveXZ (controlState.moveInXZ);

    if (isGrounded)
      ChangeState (controlState.running ? FSM.Run : FSM.Fence);

    if (controlState.attack) {
      if (tryToThrowSword && attachedSword)
        ThrowSword ();
      else
        ChangeState (FSM.Divekick);
    }
  }

  void DoDivekick() {
    transform.rotation = Quaternion.LookRotation (-divekickLook, -divekickMove);

    if (isGrounded || rbody.velocity.y >= 0) {
      divekickHitbox.SetActive (false);
      rbody.useGravity = true;
      ChangeState (FSM.Fence);
    }
  }

  void DoStunned() {
    MoveXZ (Vector3.zero);

    if (controlState.heightChange > 0 && Time.time - stunTime >= stunMinDuration) {
      transform.localRotation = Quaternion.Euler(Vector3.zero);
      swordChecker.active = true;
      canBeAffected = true;
      ChangeState (FSM.Fence);
    }
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
    if (!otherplayers [minindex].isDead)
      PointCharacter (otherplayers [minindex].transform.position - transform.position);
    else
      LookAtLastVelocity ();
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
  CharController[] GetOtherPlayers() {
    Object[] allChars = Object.FindObjectsOfType(typeof(CharController));
    CharController[] others = new CharController[allChars.Length - 1];

    int j = 0;
    for (int i = 0; i < allChars.Length; i++) {
      if (allChars [i] != this)
        others [j++] = (CharController)allChars [i];
    }
    return others;
  }
}
