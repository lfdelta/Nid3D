using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TeamUtility.IO;

public class RightOfWayWall : MonoBehaviour {
  public float distance;
  [HideInInspector] public PlayerID player;
  [HideInInspector] public Vector3 avgPlayerPos;

  public WorldNodeScript startNode;
  private WorldNodeScript endNode;

//  void Start() {
//    endNode = startNode.nextNode;
//  }

  // to be called by the GameController
  public void Initialize (PlayerID pid, WorldNodeScript fnode) {
    player = pid;
    startNode = fnode;
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

    return t0 / (t0 + t1);
  }

  // converts a t value to the linear interpolation between start and end node positions
  Vector3 LinetoWorld(float t) {
    return startNode.transform.position + t*(endNode.transform.position - startNode.transform.position);
  }

  // if players have exceeded the bounds of this segment, move to the adjacent segment
  void CheckNodeTransition () {
    float t = WorldtoLine (avgPlayerPos);

    if (t >= 1 && endNode.nextNode != null) {
      startNode = endNode;
      endNode = startNode.nextNode;
      t = WorldtoLine (avgPlayerPos);
    } else if (t < 0 && startNode.prevNode != null) {
      endNode = startNode;
      startNode = startNode.prevNode;
      t = WorldtoLine (avgPlayerPos);
    }
  }
	
  void WalkAlongSegments () {
    float dist = distance;
    Vector3 pos = transform.position;
    WorldNodeScript node = (player == PlayerID.One) ? startNode : endNode;

    Vector3 playerT = LinetoWorld(WorldtoLine(avgPlayerPos));
    Vector3 playerTtoNode = node.transform.position - playerT;

    while (playerTtoNode.sqrMagnitude < dist * dist) {
      dist -= playerTtoNode.magnitude;
      playerT = node.transform.position;
      WorldNodeScript newnode = (player == PlayerID.One) ? node.prevNode : node.nextNode;
      if (newnode == null) // can't progress further
        break;
      node = newnode;
      playerTtoNode = node.transform.position - playerT;
    }

    Vector3 wallDir = (player == PlayerID.One) ? node.prevSegmentHat : node.segmentHat;
    transform.position = playerT + dist*wallDir;
  }

	// Update is called once per frame
	void Update () {
    CheckNodeTransition ();

    WalkAlongSegments ();
	}
}
