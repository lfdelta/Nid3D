using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TeamUtility.IO;

public class SetPlayerJoypad : MonoBehaviour {

  public GameObject padControlsPanel;
  public Text text;
  public PlayerID playerID;

  private SceneController sceneControl;

  void Start() {
    Init ();
  }

  public void Init () {
    sceneControl = FindObjectOfType<SceneController> ();
    int n = (playerID == PlayerID.One) ? sceneControl.P1padNum : sceneControl.P2padNum;
    SetJoypadNumber (n);

    Slider s = GetComponent<Slider> ();
    s.value = n;
  }
	
  public void SetJoypadNumber(float joynum) {
    //Transform joypanel = (transform.parent.parent).GetChild(3);
    RebindInput[] controls = padControlsPanel.GetComponentsInChildren<RebindInput> ();

    int num = Tools.Clamp ((int)joynum, 0, 11);
    for (int i = 0; i < controls.Length; i++) {
      controls [i]._joystick = num;
    }

    text.text = num.ToString();

    sceneControl.SetPadNum (playerID, num);
  }
}
