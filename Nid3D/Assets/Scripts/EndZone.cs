using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TeamUtility.IO;

public class EndZone : MonoBehaviour {
  public PlayerID player;
  public Material inactiveMaterial;
  public Material activeMaterial;

  private GameController gameController;
  private MeshRenderer meshRender;
  private bool active;

  void Awake() {
    gameController = FindObjectOfType<GameController> ();
    meshRender = GetComponent<MeshRenderer> ();
    UpdateROW (null);
  }

  // change "active" status and visual feedback based upon right of way
  // meant to be called by the GameController
  public void UpdateROW(System.Nullable<PlayerID> rightOfWay) {
    if (rightOfWay != null && rightOfWay == player) {
      active = true;
      meshRender.material = activeMaterial;
    } else {
      active = false;
      meshRender.material = inactiveMaterial;
    }
  }

  void OnTriggerEnter (Collider other) {
    if (active) {
      CharController otherChar = other.GetComponent<CharController> ();
      if (otherChar != null && otherChar.playerid == player)
        gameController.SendMessage ("PlayerWonGame", player);
    }
  }

  // in case the player is already standing inside the endzone when it is activated
  void OnTriggerStay(Collider other) {
    OnTriggerEnter (other);
  }
}
