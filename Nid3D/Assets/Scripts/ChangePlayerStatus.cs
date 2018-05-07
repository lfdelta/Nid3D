using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TeamUtility.IO;

public class ChangePlayerStatus : MonoBehaviour {
  public bool affectAllPlayers;
  public PlayerID playerToAffect; // if affectAllPlayers is false, only this player will die
  public bool killVStun;

  // if the appropriate player enters the trigger, kill or stun them
  void OnTriggerEnter (Collider other) {
    CharController otherChar = other.GetComponent<CharController> ();
    if (otherChar != null) {
      if (affectAllPlayers || (playerToAffect == otherChar.playerid && otherChar.canBeAffected))
        otherChar.SendMessage (killVStun ? "Die" : "Stun");
    }
  }
}
