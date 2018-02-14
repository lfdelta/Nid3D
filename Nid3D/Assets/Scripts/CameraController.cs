using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TeamUtility.IO;

public class CameraController : MonoBehaviour {

  public float trackDistanceXZ = 10;
  public float height = 5;
  public float vertAngle = 10;
  public float rightOfWayTilt = 10;
  [HideInInspector] public Transform[] targets;
  [HideInInspector] public System.Nullable<PlayerID> rightOfWay;

  private Vector3 avgpos;
  private float tilt;
  	
	void Update () {
    avgpos = Vector3.zero;
    // find average position of targets, relative to camera
    for (int i = 0; i < targets.Length; i++)
      avgpos += targets [i].position;
    if (targets.Length > 0)
      avgpos /= targets.Length;

    // find the camera angle from right of way
    switch (rightOfWay) {
    case null:
      tilt = 0;
      break;
    case PlayerID.One:
      tilt = rightOfWayTilt;
      break;
    case PlayerID.Two:
      tilt = -rightOfWayTilt;
      break;
    }

    // point at avgpos
    Vector3 separation = new Vector3(Mathf.Sin(Mathf.Deg2Rad * tilt), 0, Mathf.Cos(Mathf.Deg2Rad * tilt)); // unit length
    transform.position = avgpos - trackDistanceXZ * separation + new Vector3(0, height, 0);
    transform.rotation = Quaternion.AngleAxis(tilt, transform.up) * Quaternion.AngleAxis(vertAngle, transform.right);
  }
}
