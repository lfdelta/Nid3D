using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillPlayer : MonoBehaviour {
  public bool killOnEnter;

  void OnTriggerEnter (Collider other) {
    if (killOnEnter)
      DoKill (other);
  }

  // if a player exits this collider, kill them
  void OnTriggerExit (Collider other) {
    if (!killOnEnter)
      DoKill (other);
	}

  void DoKill (Collider other) {
    CharController otherChar = other.GetComponent<CharController> ();
    if (otherChar != null)
      otherChar.SendMessage("Die");
  }
}
