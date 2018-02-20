using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldNodeScript : MonoBehaviour {
  public WorldNodeScript prevNode;
  public WorldNodeScript nextNode;
  //public Vector3 camDistance;
  //public Vector3 camEuler;

  private Vector3 bisectorHat; // unit vector bisecting the line segments extending from this node
	
	void Start () {
  }

}
