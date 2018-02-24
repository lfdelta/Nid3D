using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TeamUtility.IO;

public class RightOfWayWall : NodeTraversal {
  public float distance;
  [HideInInspector] public PlayerID player;
  [HideInInspector] public Vector3 avgPlayerPos;

  [HideInInspector] public WorldNodeScript playerStartNode;
  private WorldNodeScript playerEndNode;
  private WorldNodeScript startNode;
  private WorldNodeScript endNode;

  // to be called by the GameController
  public void Initialize (PlayerID pid, WorldNodeScript fnode) {
    player = pid;
    playerStartNode = fnode;
    playerEndNode = playerStartNode.nextNode;

    startNode = playerStartNode;
    endNode = playerEndNode;
  }
	
  // places the wall the assigned distance from the players
  // aligns its rotation with the equi-t line defined by node bisectors
  void PlaceSelfAndRotate () {
    float dist = distance;
    Vector3 pos = transform.position;
    WorldNodeScript node = (player == PlayerID.One) ? playerStartNode : playerEndNode;
    WorldNodeScript newnode = (player == PlayerID.One) ? node.prevNode : node.nextNode;

    float t = WorldtoLine (avgPlayerPos, playerStartNode, playerEndNode);
    Vector3 playerT = LinetoWorld(t, playerStartNode, playerEndNode);
    Vector3 playerTtoNode = node.transform.position - playerT;

    // traverse down player nodes until sufficiently far from players
    while (playerTtoNode.sqrMagnitude < dist * dist) {
      dist -= playerTtoNode.magnitude;
      playerT = node.transform.position;
      newnode = (player == PlayerID.One) ? node.prevNode : node.nextNode;
      if (newnode == null) // can't traverse any further
        break;
      node = newnode;
      playerTtoNode = node.transform.position - playerT;
    }

    // update position
    Vector3 wallDir = (player == PlayerID.One) ? -node.nextNode.prevSegmentHat : node.prevNode.segmentHat;
    transform.position = playerT + dist*wallDir;

    // update rotation
    float twall = WorldtoLine (transform.position, startNode, endNode);
    Debug.Log (twall);
    Vector3 equiT = Vector3.Slerp(startNode.bisectorHat, endNode.bisectorHat, twall);
    transform.rotation = Quaternion.LookRotation (equiT);
  }

  // if you have exceeded the bounds of this segment, move to the adjacent segment
  void CheckNodeTransition () {
    float t = WorldtoLine (avgPlayerPos, playerStartNode, playerEndNode);

    if (t >= 1 && playerEndNode.nextNode != null) {
      playerStartNode = playerEndNode;
      playerEndNode = playerStartNode.nextNode;
    } else if (t < 0 && playerStartNode.prevNode != null) {
      playerEndNode = playerStartNode;
      playerStartNode = playerStartNode.prevNode;
    }

    float twall = WorldtoLine (transform.position, startNode, endNode);

    if (twall >= 1 && endNode.nextNode != null) {
      startNode = endNode;
      endNode = startNode.nextNode;
    } else if (twall < 0 && startNode.prevNode != null) {
      endNode = startNode;
      startNode = startNode.prevNode;
    }
  }

	void Update () {
    CheckNodeTransition ();
    PlaceSelfAndRotate ();
	}
}
