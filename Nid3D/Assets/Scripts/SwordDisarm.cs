using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TeamUtility.IO;

public class SwordDisarm : MonoBehaviour {

  [HideInInspector] public bool canDisarm = false; // updated via Sword, in turn via CharController

  // if this collides with another sword, then tell the player holding it to drop the sword
  void OnTriggerEnter (Collider other) {
    if (canDisarm) {
      System.Nullable<PlayerID> otherPlayer = (other.transform.parent.GetComponent<Sword> ()).thisPlayer;
      if (otherPlayer != null) {
        CharController otherChar = other.transform.parent.parent.GetComponent<CharController> ();
        otherChar.SendMessage ("DropSword", true);
      }
    }
  }
}
