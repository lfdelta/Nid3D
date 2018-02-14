using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerColliderRepel : MonoBehaviour {

  public float forceScalingFactor = 1;
  private Rigidbody rbody;

  void Start() {
    rbody = transform.parent.parent.GetComponent<Rigidbody> ();
  }
    
  void OnTriggerEnter(Collider other) {
    Debug.Log ("collision");
    Vector3 r1 = transform.position;
    Vector3 r2 = other.transform.position;
    Vector3 r = r2 - r1; // vector from this to other

    Rigidbody otherbody = other.transform.parent.parent.GetComponent<Rigidbody> ();
    otherbody.AddForce (forceScalingFactor * r);
    rbody.AddForce (-forceScalingFactor * r);
  }

  void OnTriggerStay(Collider other) {
    OnTriggerEnter (other);
  }
}
