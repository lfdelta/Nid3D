using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TeamUtility.IO;

public class KillPlayer : MonoBehaviour {
  public bool killOnEnter; // false -> killOnExit
  public bool killAllPlayers;
  public PlayerID playerToKill; // if killAllPlayers is false, only this player will die

  void OnTriggerEnter (Collider other) {
    if (killOnEnter)
      DoKill (other);
  }

  void OnTriggerExit (Collider other) {
    if (!killOnEnter)
      DoKill (other);
	}

  // if the appropriate player triggered the event, kill them
  void DoKill (Collider other) {
    CharController otherChar = other.GetComponent<CharController> ();
    if (otherChar != null) {
      if (killAllPlayers || playerToKill == otherChar.playerid)
        otherChar.SendMessage("Die");
    }
  }
}
