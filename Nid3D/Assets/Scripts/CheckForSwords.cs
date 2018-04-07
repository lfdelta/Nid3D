using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckForSwords : MonoBehaviour {
  private List<Collider> activeColliders;

  void Awake () {
    activeColliders = new List<Collider>();
  }

  void OnTriggerEnter(Collider other) {
    Sword s = other.gameObject.GetComponent<Sword> ();

    if (s != null && s.thisPlayer == null)
      activeColliders.Add (other);
  }

  void OnTriggerExit(Collider other) {
    activeColliders.Remove (other); // this should be okay even if the elemnt was never added
  }
    
  public Sword FirstElement() {
    if (activeColliders.Count > 0)
      return activeColliders [0].gameObject.GetComponent<Sword>();
    else
      return null;
  }
}