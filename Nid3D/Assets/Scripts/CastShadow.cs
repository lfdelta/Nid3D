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
    surfaceDistance = 0.1f;
    layermask = 1; // raycast to Default layer only
    meshRender = GetComponent<MeshRenderer> ();
	}
	
	void Update () {
    RaycastHit rayinfo;
    bool hit = Physics.Raycast(casterPos.position + 0.1f*Vector3.up, Vector3.down,
      out rayinfo, Mathf.Infinity, layermask);

    if (hit) {
      meshRender.enabled = true;
      nhat = rayinfo.normal;
      transform.position = rayinfo.point + nhat * surfaceDistance;
      transform.rotation = Quaternion.LookRotation (Vector3.ProjectOnPlane (casterPos.forward, nhat), nhat);
    } else {
      meshRender.enabled = false;
    }
	}
}
