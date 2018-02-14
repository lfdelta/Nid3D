using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TeamUtility.IO;

public class Sword : MonoBehaviour {

  public System.Nullable<PlayerID> thisPlayer;

  void ChangeOwnership(System.Nullable<PlayerID> id) {
    thisPlayer = id;
  }

  void OnTriggerEnter(Collider other) {
    CharController otherChar = other.GetComponent<CharController> ();
    if (otherChar != null && otherChar.playerid != thisPlayer) {
      otherChar.SendMessage("Die");
    }
  }
}
