using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraNodeScript : MonoBehaviour {

	public GameObject prevNode=null;
	public GameObject nextNode=null;
	//public GameObject translationNode=null;
	//public GameObject firstRotationNode=null;
	//public GameObject firstZoomNode=null;
	public GameObject worldNode=null;

	private Quaternion dir;
	private Vector3 loc;
	private float zoom;
	private Vector3 worldloc;
	private Quaternion prevdir;
	private Vector3 prevloc;
	private float prevzoom;
	private Vector3 prevworldloc;
	private Quaternion nextdir;
	private Vector3 nextloc;
	private float nextzoom;
	private Vector3 nextworldloc;

	private GameObject currentRotationNode;
	private GameObject currentZoomNode;

	void Start () {
		loc = transform.position;
		dir = transform.localRotation;
		zoom = transform.localScale.z;
		worldloc = worldNode.transform.position;

		prevloc = prevNode.transform.position;
		prevdir = prevNode.transform.localRotation;
		prevzoom = prevNode.transform.localScale.z;
		prevworldloc = prevNode.GetComponent<CameraNodeScript>().worldNode.transform.position;

		nextloc = nextNode.transform.position;
		nextdir = nextNode.transform.localRotation;
		nextzoom = nextNode.transform.localScale.z;
		nextworldloc = nextNode.GetComponent<CameraNodeScript>().worldNode.transform.position;


		//currentRotationNode = firstRotationNode;
		//currentRotationNode = firstZoomNode;
	}

	void Update () {
		
	}

	public Vector3 getLoc(){
		return loc;
	}

	public Vector3 getWorldLoc(){
		return worldloc;
	}
}
