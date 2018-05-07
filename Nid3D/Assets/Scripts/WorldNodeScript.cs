using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldNodeScript : MonoBehaviour {
  [HideInInspector] public WorldNodeScript prevNode = null;
  public WorldNodeScript nextNode = null;
  //public Vector3 camDistance;
  //public Vector3 camEuler;

  // below are all unit vectors, projected into the XZ plane
  [HideInInspector] public Vector3 prevSegmentHat; // pointing FROM previous node
  [HideInInspector] public Vector3 segmentHat; // pointing toward next node
  [HideInInspector] public Vector3 bisectorHat; // bisecting the line segments extending from this node\

  private static Vector3 projection = new Vector3 (1, 0, 1);
	
  //* initialization assumes that there are at least two WorldNodeScripts in the linked list
  // calculate vector to next node and doubly-link the list
	void Awake () {
    segmentHat = (nextNode != null) ? (nextNode.transform.position - transform.position).normalized : Vector3.zero;
    if (nextNode != null)
      nextNode.prevNode = this;
  }

  // now that the list is doubly-linked, calculate vector to previous node and angle bisector of segments
  // if this is the end of the list, copy in the connected segment vector
  void Start () {
    #if !UNITY_EDITOR
    MeshRenderer m = GetComponent<MeshRenderer>();
    m.enabled = false;
    #endif

    prevSegmentHat = (prevNode != null) ? prevNode.segmentHat : Vector3.zero;
    bisectorHat = VectorBisectorinXZ (prevSegmentHat, segmentHat);

    if (segmentHat == Vector3.zero)
      segmentHat = prevSegmentHat;
    if (prevSegmentHat == Vector3.zero)
      prevSegmentHat = segmentHat;
  }

  // returns a unit vector which is the angle-bisector of the (zero or equal-length) inputs
  public static Vector3 VectorBisectorinXZ(Vector3 prev, Vector3 seg) {
    prev = Vector3.Scale (prev, projection).normalized;
    seg = Vector3.Scale (seg, projection).normalized;

    if (prev == Vector3.zero || prev == seg)
      return PerpendicularVector (seg);
    if (seg == Vector3.zero)
      return PerpendicularVector (prev);
   
    return (seg - prev).normalized;
  }

  // returns the perpendicular unit vector, assuming v.y == 0
  public static Vector3 PerpendicularVector(Vector3 v) {
    if (v.z == 0)
      return new Vector3 (0, 0, 1);

    Vector3 perp = new Vector3 (1, 0, -v.x / v.z);
    return perp.normalized;
  }
}
