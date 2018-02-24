using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// it might be possible/useful to rewrite these functions without an "end" parameter and compute it automatically

public class NodeTraversal : MonoBehaviour {
  
  // converts a world position to a t value (typically in [0,1]) along the current node segment
  protected float WorldtoLine(Vector3 loc, WorldNodeScript start, WorldNodeScript end) {
    Vector3 projection = new Vector3 (1, 0, 1);
    Vector3 r0 = Vector3.Scale (loc - start.transform.position, projection);
    Vector3 r1 = Vector3.Scale (loc - end.transform.position, projection);

    // calculate r vectors in terms of the non-orthonormal bisector-segment basis; t is the segment-vector component
    // this formula is derived using [e1, e2]<a,b> = <x,z> for oblique basis vectors e1, e2; r = <x,z>
    // for e1 = segmentHat, we are interested in t' = a
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
  protected Vector3 LinetoWorld(float t, WorldNodeScript start, WorldNodeScript end) {
    return start.transform.position + t*(end.transform.position - start.transform.position);
  }
}
