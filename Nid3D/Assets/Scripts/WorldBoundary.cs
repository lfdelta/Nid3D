using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldBoundary : MonoBehaviour {

  // if a player leaves the attached trigger collider, kill them
  // if any other object leaves, remove it from the scene
  void OnTriggerExit(Collider other) {
    CharController otherChar = other.GetComponent<CharController> ();
    if (otherChar != null)
      otherChar.SendMessage("Die");
    else
      Destroy (other.gameObject);
  }	
}
