using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// attach this script, alongside a non-kinematic rigidbody, to colliders which you want to
// repel each other in the horizontal plane

// this pushes the players away from each others' centers based upon the approximate sword overlap
public class TriggerColliderRepel : MonoBehaviour {

  public float linearCoefficient = 1;
  public float quadraticCoefficient = 1;
  private Rigidbody rbody;

  void Start() {
    rbody = transform.parent.parent.GetComponent<Rigidbody> (); // fencer
  }
    
  //** be careful not to do this if there is no parent (de/activate in charcontroller drop/attach)
  //** project into XZ plane
  void OnTriggerEnter(Collider other) {
    if (other.GetComponent<TriggerColliderRepel> () == null)
      return;

    Rigidbody otherbody = other.transform.parent.parent.GetComponent<Rigidbody> ();
    Vector3 sword1 = transform.position;
    Vector3 sword2 = other.transform.position;
    Vector3 fencer1 = transform.parent.parent.position;
    Vector3 fencer2 = other.transform.parent.parent.position;

    Vector3 rsword = sword2 - sword1; // vector from this to other
    float overlap = ((BoxCollider)other).size.x - rsword.magnitude;

    Vector3 fencerhat = Vector3.Scale(fencer2 - fencer1, new Vector3(1,0,1)).normalized;
    Vector3 f = RepulsionForce (Mathf.Abs(overlap) * fencerhat);
    otherbody.AddForce (f);
    rbody.AddForce (-f);
  }

  void OnTriggerStay(Collider other) {
    OnTriggerEnter (other);
  }

  Vector3 RepulsionForce(Vector3 r) {
    //return linearCoefficient * r;

    return (linearCoefficient + quadraticCoefficient * r.magnitude) * r;
  }
}
