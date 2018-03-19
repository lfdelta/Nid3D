using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckGrounded : MonoBehaviour {
  private CharController controller;
  private List<Collider> activeColliders;
  private Vector3 groundNormal; // normal vector of object most recently entered

  void Awake () {
    controller = GetComponentInParent<CharController> ();
    activeColliders = new List<Collider>();
  }

  void OnTriggerEnter(Collider other) {
    activeColliders.Add (other);
    SendGroundedMessage ();
  }

  void OnTriggerExit(Collider other) {
    activeColliders.Remove (other);
    SendGroundedMessage ();
  }

  void SendGroundedMessage() {
    bool isGrounded = (activeColliders.Count > 0);
    controller.SendMessage ("SetGrounded", isGrounded);

    Debug.Log (controller.playerid + " " + isGrounded);
  }
}
