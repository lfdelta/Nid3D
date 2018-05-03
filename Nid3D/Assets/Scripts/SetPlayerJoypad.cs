using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SetPlayerJoypad : MonoBehaviour {

  public GameObject padControlsPanel;
  public Text text;
	
  public void SetJoypadNumber(float joynum) {
    //Transform joypanel = (transform.parent.parent).GetChild(3);
    RebindInput[] controls = padControlsPanel.GetComponentsInChildren<RebindInput> ();

    int num = Tools.Clamp ((int)joynum, 0, 11);
    for (int i = 0; i < controls.Length; i++) {
      controls [i]._joystick = num;
    }

    text.text = num.ToString();
  }
}
