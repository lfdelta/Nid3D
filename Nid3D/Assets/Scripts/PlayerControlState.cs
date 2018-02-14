using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControlState {
  public Vector3 moveInXZ;
  public int heightChange; // -1,0,1
  public bool jump;
  public bool heightHold; // holding sword up or staying crouched
  public bool attack;

  void Awake () {
    moveInXZ = Vector3.zero;
    heightChange = 0;
    jump = false;
    heightHold = false;
    attack = false;
  }
}
