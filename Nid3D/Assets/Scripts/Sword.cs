using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TeamUtility.IO;

public class Sword : MonoBehaviour {

  public PlayerID thisPlayer;

	// Use this for initialization
	void Start () {
		
	}
	
  void ChangeOwnership(PlayerID id) {
    thisPlayer = id;
  }

  void OnTriggerEnter(Collider other) {
    CharController charcontrol = other.GetComponent<CharController> ();
    if (charcontrol != null && charcontrol.playerid != thisPlayer) {
      charcontrol.SendMessage("Die");
    }
  }
}
