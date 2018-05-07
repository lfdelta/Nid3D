using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TeamUtility.IO;

public class KillPlayer : MonoBehaviour {
  public bool killAllPlayers;
  public PlayerID playerToKill; // if killAllPlayers is false, only this player will die

  // if the appropriate player enters the trigger, kill them
  void OnTriggerEnter (Collider other) {
    CharController otherChar = other.GetComponent<CharController> ();
    if (otherChar != null) {
      if (killAllPlayers || playerToKill == otherChar.playerid)
        otherChar.SendMessage("Die");
    }
  }
}
