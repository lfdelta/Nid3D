using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TeamUtility.IO;

public class CameraController : MonoBehaviour {
  public bool debug = false;
  public float trackDistanceXZ = 10;
  public float height = 5;
  public float vertAngle = 10;
  public float rightOfWayTilt = 10;
  public float translationSpeed = 1;
  public float rotationSpeed = 1;
  public WorldNodeScript firstNode;
  [HideInInspector] public Vector3 avgpos;

  private float tilt;
  private WorldNodeScript startNode;
  private WorldNodeScript endNode;

  void Start() {
    startNode = firstNode;
    endNode = startNode.nextNode;
  }

  // converts a world position to a t value (typically in [0,1]) along the current node segment
  float WorldtoLine(Vector3 loc) {
    Vector3 projection = new Vector3 (1, 0, 1);
    Vector3 r0 = Vector3.Scale (loc - startNode.transform.position, projection);
    Vector3 r1 = Vector3.Scale (loc - endNode.transform.position, projection);

    // calculate r vectors in terms of the non-orthonormal bisector-segment basis; t is the segment-vector component
    // this formula is derived using [e1, e2]<a,b> = <x,z> for oblique basis vectors e1, e2; r = <x,z>
    // for e1 = segmentHat, we are interested in t' = a
    Vector3 l0 = startNode.segmentHat;
    Vector3 b0 = startNode.bisectorHat;
    float t0 = (r0.x * b0.z - r0.z * b0.x) / (l0.x * b0.z - l0.z * b0.x);

    Vector3 l1 = -startNode.segmentHat;
    Vector3 b1 = endNode.bisectorHat;
    float t1 = (r1.x * b1.z - r1.z * b1.x) / (l1.x * b1.z - l1.z * b1.x);

    if (debug)
      Debug.Log (startNode.name + " t: " + t0 / (t0 + t1));

    return t0 / (t0 + t1);
  }

  // converts a t value to the linear interpolation between start and end node positions
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
    float t = WorldtoLine (avgpos);
    
    if (t >= 1 && endNode.nextNode != null) {
      startNode = endNode;
      endNode = startNode.nextNode;
      t = WorldtoLine (avgpos);
    } else if (t < 0 && startNode.prevNode != null) {
      endNode = startNode;
      startNode = startNode.prevNode;
      t = WorldtoLine (avgpos);
    }

    Vector3 cameraLoc = avgpos; //LinetoWorld (t);

    // smoothly follow and point at avgpos
    Vector3 separation = new Vector3(Mathf.Sin(Mathf.Deg2Rad * tilt), 0, Mathf.Cos(Mathf.Deg2Rad * tilt)); // unit length
    Vector3 newPos = cameraLoc - trackDistanceXZ * separation + new Vector3(0, height, 0);
    Quaternion newRot = Quaternion.Euler(vertAngle, tilt, 0);

    transform.position = Vector3.Lerp(transform.position, newPos, translationSpeed * Time.deltaTime);
    transform.rotation = Quaternion.Slerp (transform.rotation, newRot, rotationSpeed*Time.deltaTime);
  }
}
