using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TeamUtility.IO;

public class EndZone : MonoBehaviour {
  public PlayerID player;

  private GameController gameController;

  void Awake() {
    gameController = FindObjectOfType<GameController> ();
  }

  void OnTriggerEnter (Collider other) {
    CharController otherChar = other.GetComponent<CharController> ();
    if (otherChar != null && otherChar.playerid == player)
      gameController.SendMessage("PlayerWonGame", player);
  }
}
