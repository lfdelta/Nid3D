using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TeamUtility.IO;

public class RightOfWayWall : NodeTraversal {
  public float distance;
  public float translationSpeed;
  public float rotationSpeed;
  [HideInInspector] public PlayerID player;
  [HideInInspector] public Vector3 avgPlayerPos;

  [HideInInspector] public WorldNodeScript playerLeftNode;
  private WorldNodeScript leftNode;

  private bool standStill;
  private MeshRenderer meshRender;
  private BoxCollider boxCol;
  private float height;

  void Awake () {
    meshRender = GetComponent<MeshRenderer> ();
    boxCol = GetComponent<BoxCollider> ();
    height = boxCol.bounds.extents.y;
  }

  // to be called by the GameController
  public void Initialize (PlayerID pid, WorldNodeScript fnode) {
    player = pid;
    playerLeftNode = fnode;

    leftNode = playerLeftNode;
  }

  // to be called by the GameController
  public void UpdatePlayerInfo (Vector3 loc, WorldNodeScript node) {
    avgPlayerPos = loc;
    playerLeftNode = node;
  }

  // activate or deactivate movement and collisions based on right of way
  public void UpdateROW (System.Nullable<PlayerID> rightOfWay) {
    if (rightOfWay == null) {
      standStill = true;
      ActivateSelf (true);
    } else {
      standStill = false;
      ActivateSelf (rightOfWay == player);
    }
  }

  void ActivateSelf(bool active) {
    meshRender.enabled = active;
    boxCol.enabled = active;
  }
	
  // places the wall the assigned distance from the players
  // aligns its rotation with the equi-t line defined by node bisectors
  void PlaceSelfAndRotate (bool smooth=true) {
    // update position
    Vector3 newPos = PositionAlongSegments(distance, avgPlayerPos, playerLeftNode, (player == PlayerID.Two));
    newPos += new Vector3 (0, height, 0);

    // update rotation
    float twall = WorldtoLine (transform.position, leftNode);
    Vector3 equiT = Vector3.Slerp(leftNode.bisectorHat, leftNode.nextNode.bisectorHat, twall);
    Quaternion newRot = Quaternion.LookRotation (equiT);

    if (smooth) {
      transform.position = Vector3.Lerp (transform.position, newPos, translationSpeed * Time.deltaTime);
      transform.rotation = Quaternion.Slerp (transform.rotation, newRot, rotationSpeed * Time.deltaTime);
    } else {
      transform.position = newPos;
      transform.rotation = newRot;
    }
  }

	void Update () {
    leftNode = CheckNodeTransition (transform.position, leftNode);

    if (!standStill)
      PlaceSelfAndRotate ();
	}
}
