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
  [HideInInspector] public Transform[] targets;
  [HideInInspector] public System.Nullable<PlayerID> rightOfWay;

  private Vector3 avgpos;
  private float tilt;
  private CameraNodeScript currentNode;
  private CameraNodeScript nextNode;

  void Start() {
    currentNode = firstNode.GetComponent<CameraNodeScript>();
    nextNode = currentNode.nextNode.GetComponent<CameraNodeScript>();
  }

  Vector3 avgTargetPosition(Transform[] pos){
    if (targets.Length > 0) {
      avgpos = Vector3.zero;
      for (int i = 0; i < pos.Length; i++)
        avgpos += pos [i].position;
      avgpos /= pos.Length;
    }

    return avgpos;
  }

  float WorldtoLine(Vector3 location){    
    Vector3 relativeEnd = nextNode.getWorldLoc() - currentNode.getWorldLoc();
    Vector3 relativeLoc = location - currentNode.getWorldLoc();

    float result = Vector3.Dot(relativeLoc, relativeEnd)/(Mathf.Pow(relativeEnd.magnitude,2));

    return result;
  }

  Vector3 LinetoWorld(float t){
    return currentNode.getLoc () + ((nextNode.getLoc () - currentNode.getLoc ()) * t);
  }

  void Update () {
    avgpos = avgTargetPosition (targets);
    Vector3 cameraLoc = currentNode.getLoc ();
    if (nextNode != null) {
      float t = WorldtoLine (avgpos);
      //print (t);
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
    //Vector3 separation = new Vector3(Mathf.Sin(Mathf.Deg2Rad * tilt), 0, Mathf.Cos(Mathf.Deg2Rad * tilt)); // unit length
    transform.position = cameraLoc;
    //transform.position = avgpos - trackDistanceXZ * separation + new Vector3(0, height, 0);
    transform.rotation = Quaternion.AngleAxis(tilt, transform.up) * Quaternion.AngleAxis(vertAngle, transform.right);
  }
}
