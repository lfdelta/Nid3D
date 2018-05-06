using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VolumeInit : MonoBehaviour {

  void Start () {
    Slider s = GetComponent<Slider> ();
    SceneController sc = FindObjectOfType<SceneController> ();
    s.value = sc.volume;
	}
}
