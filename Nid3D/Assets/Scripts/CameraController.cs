using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TeamUtility.IO;

public class CameraController : MonoBehaviour {

  public float trackDistanceXZ = 10;
  public float height = 5;
  public float vertAngle = 10;
  public float rightOfWayTilt = 10;
  //public CameraNodeScript firstNode;
  public WorldNodeScript firstNode;
  [HideInInspector] public Vector3 avgpos;

  private float tilt;
  //private CameraNodeScript startNode;
  //private CameraNodeScript nextNode;
  private WorldNodeScript startNode;
  private WorldNodeScript endNode;

  void Start() {
    startNode = firstNode;
    endNode = startNode.nextNode;
  }



  /*float WorldtoLine(Vector3 location) {    
    Vector3 relativeEnd = endNode.transform.position - startNode.transform.position;
    Vector3 relativeLoc = location - startNode.transform.position;

    float result = Vector3.Dot(relativeLoc, relativeEnd)/relativeEnd.sqrMagnitude;
    return result;
  }*/

  // converts a world position to a t value (typically in [0,1]) along the current node segment
  float WorldtoLine(Vector3 loc) {
    Vector3 projection = new Vector3 (1, 0, 1);
    Vector3 r0 = Vector3.Scale (loc - startNode.transform.position, projection);
    Vector3 r1 = Vector3.Scale (loc - endNode.transform.position, projection);

    /*// t0, t1 are the relative distances of loc, parallel to the current segment, to the bisectors of either node
    Vector3 t0dotSeg = r0 - startNode.bisectorHat * Vector3.Dot (startNode.bisectorHat, r0);
    float t0 = Vector3.Dot (t0dotSeg, startNode.segmentHat);
    Vector3 t1dotSeg = r1 - nextNode.bisectorHat * Vector3.Dot (startNode.bisectorHat, r1);
    float t1 = Vector3.Dot (t1dotSeg, nextNode.segmentHat);*/

    // calculate r vectors in terms of the non-orthonormal bisector-segment basis; t is the segment-vector component
    // this formula is derived using [e1, e2]<a,b> = <x,z> for oblique basis vectors e1, e2; r = <x,z>
    // for e1 = segmentHat, we are interested in t' = a
    Vector3 l0 = startNode.segmentHat;
    Vector3 b0 = startNode.bisectorHat;
    float t0 = (r0.x * b0.z - r0.z * b0.x) / (l0.x * b0.z - l0.z * b0.x);

    Vector3 l1 = -startNode.segmentHat;
    Vector3 b1 = endNode.bisectorHat;
    float t1 = (r1.x * b1.z - r1.z * b1.x) / (l1.x * b1.z - l1.z * b1.x);

    Debug.Log ("t: " + t0 / (t0 + t1));

    return t0 / (t0 + t1);
  }

  // conve  rts a t value to the linear interpolation between current and next node position
  Vector3 LinetoWorld(float t) {
    return startNode.transform.position + t*(endNode.transform.position - startNode.transform.position);
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
    /*Vector3 cameraLoc = startNode.getLoc ();
    if (nextNode != null) {
      float t = WorldtoLine (avgpos);
      if (t >= 1 && nextNode.nextNode != null) {
        startNode = nextNode;
        nextNode = nextNode.nextNode.GetComponent<CameraNodeScript> ();
        t = WorldtoLine (avgpos);
        cameraLoc = LinetoWorld (t);
      } else if (t < 0 && startNode.prevNode != null) {
        nextNode = startNode;
        startNode = startNode.prevNode.GetComponent<CameraNodeScript> ();
        t = WorldtoLine (avgpos);
        cameraLoc = LinetoWorld (t);
      } else {
        cameraLoc = LinetoWorld (t);
      }
    }*/

    Vector3 cameraLoc = startNode.transform.position;
    //Vector3 cameraLoc = avgpos;
    if (endNode != null) {
      float t = WorldtoLine (avgpos);
      //Debug.Log (t);
      if (t >= 1 && endNode.nextNode != null) {
        startNode = endNode;
        endNode = startNode.nextNode;
        t = WorldtoLine (avgpos);
      } else if (t < 0 && startNode.prevNode != null) {
        endNode = startNode;
        startNode = startNode.prevNode;
        t = WorldtoLine (avgpos);
      }

      cameraLoc = LinetoWorld (t);
    }

    // point at avgpos
    Vector3 separation = new Vector3(Mathf.Sin(Mathf.Deg2Rad * tilt), 0, Mathf.Cos(Mathf.Deg2Rad * tilt)); // unit length
    transform.position = avgpos - trackDistanceXZ * separation + new Vector3(0, height, 0);
    transform.rotation = Quaternion.Euler (vertAngle, tilt, 0);
    //transform.position = cameraLoc;
    //transform.rotation = Quaternion.AngleAxis(tilt, transform.up) * Quaternion.AngleAxis(vertAngle, transform.right);
  }
}
