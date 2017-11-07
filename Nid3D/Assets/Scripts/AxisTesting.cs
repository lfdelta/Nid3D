using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TeamUtility.IO;

public class AxisTesting : MonoBehaviour {

  public Transform pos;
  public bool normz;
  public Text buttonstr;

	// Use this for initialization
	void Start () {
    
	}
	
	// Update is called once per frame
	void Update () {
    float horizIn = InputManager.GetAxis ("Horizontal");
    float vertIn = InputManager.GetAxis ("Vertical");

    Vector3 move = new Vector3 (horizIn, vertIn, 0);

    pos.position = ((move.magnitude > 1 && normz) ? move.normalized : move) - 1*Vector3.forward;

    UpdateText ();
	}

  void UpdateText () {
    string t = "";
    List<string> keys = new List<string>() {"HeightUp", "HeightDown", "Jump", "Attack", "Pause"};

    for (int i = 0; i < keys.Count; i++) {
      if (InputManager.GetButton (keys [i])) {
        t = string.Concat (t, keys [i]);
        t = string.Concat (t, "\n");
      }
    }

    buttonstr.text = t;
  }
}
