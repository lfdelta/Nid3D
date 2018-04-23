using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TeamUtility.IO;

public class Sword : MonoBehaviour {

  //public Material disarmMaterial;
  //[HideInInspector] public MeshRenderer meshRender;
  //[HideInInspector] public Material defaultMaterial;

  [HideInInspector] public System.Nullable<PlayerID> thisPlayer;
  [HideInInspector] public Rigidbody rbody;
  private bool beingThrown;
  private GameObject swordBox;
  private GameObject playerCollider;
  private GameObject topDisarm;
  private GameObject bottomDisarm;
  private KillPlayer hitbox;
  private SwordDisarm topDisarmScript;
  private SwordDisarm bottomDisarmScript;

  void Awake() {
    rbody = GetComponent<Rigidbody> ();
    rbody.maxAngularVelocity = 100; // default is 7

    swordBox = transform.GetChild(1).gameObject;
    playerCollider = transform.GetChild (2).gameObject;
    topDisarm = transform.GetChild (0).gameObject;
    bottomDisarm = transform.GetChild (3).gameObject;

    topDisarmScript = topDisarm.GetComponent<SwordDisarm> ();
    bottomDisarmScript = bottomDisarm.GetComponent<SwordDisarm> ();

    hitbox = playerCollider.GetComponent<KillPlayer> ();
    hitbox.killAllPlayers = false;

    //meshRender = GetComponent<MeshRenderer> ();
    //defaultMaterial = meshRender.material;

    ChangeOwnership (null);
  }

  public void ChangeOwnership(System.Nullable<PlayerID> id) {
    thisPlayer = id;

    Activate (id != null);

    if (id != null) {
      rbody.constraints = RigidbodyConstraints.FreezeAll;
      hitbox.playerToKill = Tools.OtherPlayer((PlayerID)thisPlayer);
    } else {
      rbody.constraints = RigidbodyConstraints.None;
    }
  }

  public void SetDisarmStatus(bool canDisarm, int startingHeight) {
    topDisarmScript.canDisarm = canDisarm;
    bottomDisarmScript.canDisarm = canDisarm;
    topDisarmScript.startingHeight = startingHeight;
    bottomDisarmScript.startingHeight = startingHeight;
  }

  public void Throw() {
    beingThrown = true;
    rbody.constraints = RigidbodyConstraints.None;
    rbody.useGravity = false;

    swordBox.SetActive (false);
    playerCollider.SetActive (true);
    topDisarm.SetActive (false);
    bottomDisarm.SetActive (false);
  }

  void Activate(bool isActive) {
    swordBox.SetActive(isActive);
    playerCollider.SetActive (isActive);
    topDisarm.SetActive (isActive);
    bottomDisarm.SetActive (isActive);
  }
    
  void OnCollisionEnter() {
    if (beingThrown) {
      beingThrown = false;
      rbody.useGravity = true;
      ChangeOwnership (null);
    }
  }
}
