using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodeTraversal : MonoBehaviour {

  // converts a world position to a t value (typically in [0,1]) along the current node segment
  protected static float WorldtoLine(Vector3 loc, WorldNodeScript start) {
    WorldNodeScript end = start.nextNode;
    Vector3 r0 = loc - start.transform.position;
    Vector3 r1 = loc - end.transform.position;

    // calculate r vectors in terms of the oblique bisector-segment basis; t is the component along segmentHat
    // this formula is derived using [e1, e2]<a,b> = <x,z> for oblique basis vectors e1, e2; cartesian vector r = <x,z>
    // for e1 = segmentHat, we are interested in t'
    Vector3 l0 = start.segmentHat;
    Vector3 b0 = start.bisectorHat;
    float t0 = (r0.x * b0.z - r0.z * b0.x) / (l0.x * b0.z - l0.z * b0.x);

    Vector3 l1 = -start.segmentHat;
    Vector3 b1 = end.bisectorHat;
    float t1 = (r1.x * b1.z - r1.z * b1.x) / (l1.x * b1.z - l1.z * b1.x);

    float t = t0 / (t0 + t1);

    // if the player is beyond the last node, project their position onto segmentHat
    if (t0 < 0 && start.prevNode == null)
      t = Vector3.Dot(loc - start.transform.position, start.segmentHat);
    if (t1 < 0 && end.nextNode == null)
      t = Vector3.Dot(loc - end.transform.position, end.segmentHat) + 1; // so it doesn't drop back to 0

    return t;
  }

  // converts a t value to the linear interpolation between start and end node positions
  protected static Vector3 LinetoWorld(float t, WorldNodeScript start) {
    WorldNodeScript end = start.nextNode;
    return start.transform.position + t*(end.transform.position - start.transform.position);
  }

  // returns the leftmost node associated with the given position, based upon its projection onto the current segment
  protected static WorldNodeScript CheckNodeTransition (Vector3 loc, WorldNodeScript start) {
    float t = WorldtoLine (loc, start);

    if (t >= 1 && start.nextNode.nextNode != null)
      return start.nextNode;
    if (t < 0 && start.prevNode != null)
      return start.prevNode;

    return start;
  }

  //* this misbehaves very badly when startLoc is beyond the extent of the world nodes!!
  // returns a position (y=0) which lies on the line segments defined by the world nodes
  // this position is a given distance, traveling along the world node segments,
  // from startLoc's projection onto its current line segment (defined by "start")
  // moveForward == true will traverse forward/rightward along the world nodes; false traverses in reverse
  protected static Vector3 PositionAlongSegments(float distance, Vector3 startLoc, WorldNodeScript start, bool moveForward) {
    float dist = distance;
    WorldNodeScript node = moveForward ? start.nextNode : start;
    WorldNodeScript destNode = moveForward ? node.nextNode: node.prevNode;

    float t = WorldtoLine (startLoc, start);
    Vector3 playerT = LinetoWorld(t, start);
    Vector3 playerTtoNode = node.transform.position - playerT;

    // traverse down player nodes until sufficiently far from players
    while (playerTtoNode.sqrMagnitude < dist * dist) {
      dist -= playerTtoNode.magnitude;
      playerT = node.transform.position;
      destNode = moveForward ? node.nextNode : node.prevNode;
      if (destNode == null) // can't traverse any further
        break;
      node = destNode;
      playerTtoNode = node.transform.position - playerT;
    }

    // update position
    Vector3 wallDir = moveForward ? node.prevNode.segmentHat : -node.nextNode.prevSegmentHat;
    return playerT + dist * wallDir;
  }
}
