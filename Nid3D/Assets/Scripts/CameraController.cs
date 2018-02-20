using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TeamUtility.IO;

public class CameraController : MonoBehaviour {

  public float trackDistanceXZ = 10;
  public float height = 5;
  public float vertAngle = 10;
  public float rightOfWayTilt = 10;
  public GameObject firstNode;
  //[HideInInspector] public System.Nullable<PlayerID> rightOfWay;
  [HideInInspector] public Vector3 avgpos;

  private float tilt;
  private CameraNodeScript currentNode;
  private CameraNodeScript nextNode;

  void Start() {
    currentNode = firstNode.GetComponent<CameraNodeScript>();
    nextNode = currentNode.nextNode.GetComponent<CameraNodeScript>();
  }

  float WorldtoLine(Vector3 location) {    
    Vector3 relativeEnd = nextNode.getWorldLoc() - currentNode.getWorldLoc();
    Vector3 relativeLoc = location - currentNode.getWorldLoc();

    float result = Vector3.Dot(relativeLoc, relativeEnd)/(Mathf.Pow(relativeEnd.magnitude,2));

    return result;
  }

  Vector3 LinetoWorld(float t) {
    return currentNode.getLoc () + t*(nextNode.getLoc () - currentNode.getLoc ());
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
    Vector3 cameraLoc = currentNode.getLoc ();
    if (nextNode != null) {
      float t = WorldtoLine (avgpos);
      if (t >= 1 && nextNode.nextNode != null) {
        currentNode = nextNode;
        nextNode = nextNode.nextNode.GetComponent<CameraNodeScript> ();
        t = WorldtoLine (avgpos);
        cameraLoc = LinetoWorld (t);
      } else if (t < 0 && currentNode.prevNode != null) {
        nextNode = currentNode;
        currentNode = currentNode.prevNode.GetComponent<CameraNodeScript> ();
        t = WorldtoLine (avgpos);
        cameraLoc = LinetoWorld (t);
      } else {
        cameraLoc = LinetoWorld (t);
      }
    }

    // point at avgpos
    transform.position = cameraLoc;
    transform.rotation = Quaternion.AngleAxis(tilt, transform.up) * Quaternion.AngleAxis(vertAngle, transform.right);
  }
}
