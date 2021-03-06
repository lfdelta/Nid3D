﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TeamUtility.IO;

public class Sword : MonoBehaviour {

  [HideInInspector] public System.Nullable<PlayerID> thisPlayer;
  [HideInInspector] public Rigidbody rbody;
  private bool beingThrown;
  private GameObject swordBox;
  private GameObject playerCollider;
  private GameObject throwCollider;
  private GameObject topDisarm;
  private GameObject bottomDisarm;
  private KillPlayer hitbox;
  private KillPlayer throwHitbox;
  private SwordDisarm topDisarmScript;
  private SwordDisarm bottomDisarmScript;

  void Awake() {
    rbody = GetComponent<Rigidbody> ();
    rbody.maxAngularVelocity = 100; // default is 7

    swordBox = transform.GetChild(1).gameObject;
    playerCollider = transform.GetChild (2).gameObject;
    throwCollider = transform.GetChild (4).gameObject;
    topDisarm = transform.GetChild (0).gameObject;
    bottomDisarm = transform.GetChild (3).gameObject;

    topDisarmScript = topDisarm.GetComponent<SwordDisarm> ();
    bottomDisarmScript = bottomDisarm.GetComponent<SwordDisarm> ();

    hitbox = playerCollider.GetComponent<KillPlayer> ();
    hitbox.killAllPlayers = false;
    throwHitbox = throwCollider.GetComponent<KillPlayer> ();
    throwHitbox.killAllPlayers = false;

    throwCollider.SetActive (false);

    ChangeOwnership (null);
  }

  public void ChangeOwnership(System.Nullable<PlayerID> id) {
    thisPlayer = id;
    rbody.drag = 0;
    rbody.angularDrag = 0;

    Activate (id != null);
   
    if (id != null) {
      rbody.constraints = RigidbodyConstraints.FreezeAll;
      PlayerID opponent = Tools.OtherPlayer((PlayerID)thisPlayer);
      hitbox.playerToKill = opponent;
      throwHitbox.playerToKill = opponent;
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

    Activate(false);
    throwCollider.SetActive (true);
  }

  void Activate(bool isActive) {
    swordBox.SetActive(isActive);
    playerCollider.SetActive (isActive);
    topDisarm.SetActive (isActive);
    bottomDisarm.SetActive (isActive);
  }

  public void StopThrowing() {
    if (beingThrown) {
      beingThrown = false;
      rbody.useGravity = true;
      rbody.velocity = Vector3.zero;
      rbody.angularVelocity = Vector3.zero;

      ChangeOwnership (null);
      throwCollider.SetActive (false);
    }
  }

  void OnCollisionEnter() {
    StopThrowing ();
  }

  // stop moving if it hangs out on the ground
  void OnCollisionStay(Collision c) {
    if (c.impulse.y > 0) {
      rbody.angularDrag += 0.1f;
      rbody.drag += 0.1f;
    }
  }
}
