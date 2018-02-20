using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraNodeScript : MonoBehaviour {

	public GameObject prevNode=null;
	public GameObject nextNode=null;
	public GameObject worldNode=null;

	private Vector3 loc;
	private float zoom;
	private Vector3 worldloc;

	void Start () {
		loc = transform.position;
		worldloc = worldNode.transform.position;
  }

	public Vector3 getLoc() {
		return loc;
	}

	public Vector3 getWorldLoc() {
		return worldloc;
	}
}
