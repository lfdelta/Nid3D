using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TeamUtility.IO;

public class Sword : MonoBehaviour {

  private System.Nullable<PlayerID> thisPlayer;
  private Rigidbody rbody;
  private GameObject swordBox;
  private GameObject playerCollider;
  private KillPlayer hitbox;

  void Awake() {
    rbody = GetComponent<Rigidbody> ();
    swordBox = transform.GetChild(0).gameObject;
    playerCollider = transform.GetChild (1).gameObject;

    hitbox = playerCollider.GetComponent<KillPlayer> ();
    hitbox.killAllPlayers = false;

    ChangeOwnership (null);
  }

  public void ChangeOwnership(System.Nullable<PlayerID> id) {
    thisPlayer = id;
    Activate (id != null);
  }

  void Activate(bool isActive) {
    if (isActive) {
      rbody.constraints = RigidbodyConstraints.FreezeAll;
      hitbox.playerToKill = OtherPlayer();
    } else {
      rbody.constraints = RigidbodyConstraints.None;
    }

    swordBox.SetActive(isActive);
    playerCollider.SetActive (isActive);
  }

  PlayerID OtherPlayer() {
    switch (thisPlayer) {
    case PlayerID.One:
      return PlayerID.Two;
    default:
      return PlayerID.One;
    }
  }
}
