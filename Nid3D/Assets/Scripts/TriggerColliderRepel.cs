using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerColliderRepel : MonoBehaviour {

  public float forceScalingFactor = 1;
  private Rigidbody rbody;

  void Start() {
    rbody = transform.parent.parent.GetComponent<Rigidbody> ();
  }
    
  // be careful not to do this if there is no parent
  void OnTriggerEnter(Collider other) {
    Vector3 r1 = transform.parent.parent.position;
    Vector3 r2 = other.transform.parent.parent.position;
    Vector3 r = r2 - r1; // vector from this to other

    Rigidbody otherbody = other.transform.parent.parent.GetComponent<Rigidbody> ();
    otherbody.AddForce (forceScalingFactor * r / r.sqrMagnitude);
    rbody.AddForce (-forceScalingFactor * r / r.sqrMagnitude);
  }

  void OnTriggerStay(Collider other) {
    OnTriggerEnter (other);
  }
}
