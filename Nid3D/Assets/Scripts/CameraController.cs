using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TeamUtility.IO;

//public class CameraController : MonoBehaviour {
public class CameraController : NodeTraversal {
  public bool debug = false;
  public float trackDistanceXZ = 10;
  public float height = 5;
  public float vertAngle = 10;
  public float rightOfWayTilt = 10;
  public float translationSpeed = 1;
  public float rotationSpeed = 1;
  //[HideInInspector] public Vector3 avgPlayerPos;

  private float tilt;
  private Vector3 avgPlayerPos;
  private WorldNodeScript leftNode;
  //[HideInInspector] public WorldNodeScript leftNode;

  // to be called by the GameController
  public void UpdatePlayerInfo (Vector3 loc, WorldNodeScript node) {
    avgPlayerPos = loc;
    leftNode = node;
  }

  // find the camera angle from right of way
  public void UpdateROW(System.Nullable<PlayerID> rightOfWay) {
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
  }



  void Update () {
    Vector3 cameraLoc = avgPlayerPos; //LinetoWorld (t);
    if (cameraLoc.y < 0)
      cameraLoc = new Vector3 (cameraLoc.x, 0, cameraLoc.z);

    // smoothly follow and point at avgPlayerPos
    Vector3 separation = new Vector3(Mathf.Sin(Mathf.Deg2Rad * tilt), 0, Mathf.Cos(Mathf.Deg2Rad * tilt)); // unit length
    Vector3 newPos = cameraLoc - trackDistanceXZ * separation + new Vector3(0, height, 0);
    Quaternion newRot = Quaternion.Euler(vertAngle, tilt, 0);

    transform.position = Vector3.Lerp(transform.position, newPos, translationSpeed * Time.deltaTime);
    transform.rotation = Quaternion.Slerp (transform.rotation, newRot, rotationSpeed*Time.deltaTime);
  }
}
