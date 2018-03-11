using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CastShadow : MonoBehaviour {
  [HideInInspector] public Transform casterPos;

  private Vector3 nhat;
  private MeshRenderer meshRender;
  private float surfaceDistance;
  private int layermask;

	void Start () {
    surfaceDistance = 0.01f;
    layermask = 1; // raycast to Default layer only
    meshRender = GetComponent<MeshRenderer> ();
	}
	
	void Update () {
    /*RaycastHit rayinfo;
    bool hit = Physics.Raycast(casterPos.position + 0.1f*Vector3.up, Vector3.down,
      out rayinfo, Mathf.Infinity, layermask);*/
    // raycast below the player
    RaycastHit[] raysinfo = Physics.RaycastAll (casterPos.position + 0.1f * Vector3.up, Vector3.down, Mathf.Infinity, layermask);
    float minDist = Mathf.Infinity;
    System.Nullable<RaycastHit> rayinfo = null;

    // find the nearest-hit object
    for (int i = 0; i < raysinfo.Length; i++) {
      RaycastHit thisHit = raysinfo [i];
      if (thisHit.distance < minDist) {
        minDist = thisHit.distance;
        rayinfo = thisHit;
      }
    }

    // place the shadow
    if (rayinfo != null) {
      meshRender.enabled = true;
      nhat = rayinfo.Value.normal;
      transform.position = rayinfo.Value.point + nhat * surfaceDistance;
      transform.rotation = Quaternion.LookRotation (Vector3.ProjectOnPlane (casterPos.forward, nhat), nhat);
    } else {
      meshRender.enabled = false;
    }
	}
}
