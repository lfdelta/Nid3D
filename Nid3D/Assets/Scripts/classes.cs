using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlState {
  public Vector3 moveInXZ = new Vector3 (0,0,0);
  public int heightChange = 0;         // -1, 0, 1
  public bool jump        = false;
  public bool heightHold  = false;     // -1,0,1 holding sword up or staying crouched
  public bool attack      = false;
}

