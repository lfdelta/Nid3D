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
  //public WorldNodeScript firstNode;
  [HideInInspector] public Vector3 avgPlayerPos;

  private float tilt;
  private WorldNodeScript startNode;
  private WorldNodeScript endNode;

  // to be called by the GameController
  public void Initialize (WorldNodeScript fnode) {
    startNode = fnode;
    endNode = startNode.nextNode;
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

  // if players have exceeded the bounds of this segment, move to the adjacent segment
  void CheckNodeTransition () {
    float t = WorldtoLine (avgPlayerPos, startNode, endNode);
    if (debug)
      Debug.Log (t);

    if (t >= 1 && endNode.nextNode != null) {
      startNode = endNode;
      endNode = startNode.nextNode;    
    } else if (t < 0 && startNode.prevNode != null) {
      endNode = startNode;
      startNode = startNode.prevNode;
    }
  }

  void Update () {
    CheckNodeTransition ();

    Vector3 cameraLoc = avgPlayerPos; //LinetoWorld (t);

    // smoothly follow and point at avgPlayerPos
    Vector3 separation = new Vector3(Mathf.Sin(Mathf.Deg2Rad * tilt), 0, Mathf.Cos(Mathf.Deg2Rad * tilt)); // unit length
    Vector3 newPos = cameraLoc - trackDistanceXZ * separation + new Vector3(0, height, 0);
    Quaternion newRot = Quaternion.Euler(vertAngle, tilt, 0);

    transform.position = Vector3.Lerp(transform.position, newPos, translationSpeed * Time.deltaTime);
    transform.rotation = Quaternion.Slerp (transform.rotation, newRot, rotationSpeed*Time.deltaTime);
  }
}
