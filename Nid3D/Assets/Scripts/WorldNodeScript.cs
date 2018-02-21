using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldNodeScript : MonoBehaviour {
  public WorldNodeScript prevNode = null;
  public WorldNodeScript nextNode = null;
  //public Vector3 camDistance;
  //public Vector3 camEuler;

  // below are all unit vectors, projected into the XZ plane
  [HideInInspector] public Vector3 prevSegmentHat; // pointing FROM previous node
  [HideInInspector] public Vector3 segmentHat; // pointing toward next node
  [HideInInspector] public Vector3 bisectorHat; // bisecting the line segments extending from this node
	
  //* initialization assumes that there are at least two WorldNodeScripts in the linked list
	void Awake () {
    Vector3 projection = new Vector3 (1, 0, 1);
    segmentHat = (nextNode != null) ? (nextNode.transform.position - transform.position) : Vector3.zero;
    segmentHat = Vector3.Scale(segmentHat, projection).normalized;
  }

  void Start () {
    prevSegmentHat = (prevNode != null) ? prevNode.segmentHat : Vector3.zero;
    bisectorHat = VectorBisector (prevSegmentHat, segmentHat);
  }

  // returns a unit vector which is the angle-bisector of the (zero or equal-length) inputs
  Vector3 VectorBisector(Vector3 prev, Vector3 seg) {
    if (prev == Vector3.zero)
      return PerpendicularVector (seg);
    if (seg == Vector3.zero)
      return PerpendicularVector (prev);

    return (seg - prev).normalized;
  }

  // returns the perpendicular unit vector, assuming v.y == 0
  Vector3 PerpendicularVector(Vector3 v) {
    if (v.z == 0)
      return new Vector3 (0, 0, 1);

    Vector3 perp = new Vector3 (1, 0, -v.x / v.z);
    return perp.normalized;
  }
}
