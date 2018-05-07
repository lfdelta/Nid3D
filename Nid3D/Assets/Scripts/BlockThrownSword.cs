using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockThrownSword : MonoBehaviour {

  // if another sword enters this hitbox, and it is being thrown, tell it to stop
  void OnTriggerEnter (Collider other) {
    Sword otherSword = other.GetComponentInParent<Sword> ();
    if (otherSword != null)
      otherSword.StopThrowing();
  }
}
