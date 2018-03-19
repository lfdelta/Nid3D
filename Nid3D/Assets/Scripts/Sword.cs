using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TeamUtility.IO;

public class Sword : MonoBehaviour {

  private System.Nullable<PlayerID> thisPlayer;
  private CapsuleCollider capsule;
  private GameObject swordBox;
  private GameObject groundCollider;

  void Awake() {
    capsule = GetComponent<CapsuleCollider> ();
    swordBox = transform.GetChild(0).gameObject;
    groundCollider = transform.GetChild (1).gameObject;
    Activate (true);
  }

  public void ChangeOwnership(System.Nullable<PlayerID> id) {
    thisPlayer = id;
    Activate (id != null);
  }

  void OnTriggerEnter(Collider other) {
    CharController otherChar = other.GetComponent<CharController> ();
    if (otherChar != null && otherChar.playerid != thisPlayer)
      otherChar.SendMessage("Die");
  }

  void Activate(bool isActive) {
    capsule.enabled = isActive;
    swordBox.SetActive(isActive);
    groundCollider.SetActive (!isActive);
  }
}
