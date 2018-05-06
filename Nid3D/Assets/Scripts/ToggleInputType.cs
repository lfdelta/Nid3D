using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TeamUtility.IO;
using UnityEngine.UI;

public class ToggleInputType : MonoBehaviour {

  public PlayerID playerID;
  public GameObject[] keyButtons;
  public GameObject[] padButtons;

  private string keyConfig;
  private string padConfig;
  private bool changeToKeyboard;

  private SceneController sceneControl;

	void Awake () {
    string pstr = (playerID == PlayerID.One) ? "P1" : "P2";
    keyConfig = pstr + "Keyboard";
    padConfig = pstr + "Gamepad";

    sceneControl = FindObjectOfType<SceneController> ();
	}

  void Start () {
    Init ();
  }

  public void Init () {
    bool usePad = (playerID == PlayerID.One) ? sceneControl.P1usePad : sceneControl.P2usePad;

    // if the gamepad is set to be active, then the toggle and the gamepad panel should be on
    Toggle t = GetComponent<Toggle> ();
    t.isOn = usePad;
    RebindKeyboard (!usePad);

    changeToKeyboard = usePad; // if the gamepad is active, then switch to keyboard on next toggle
  }

  public void ToggleKeyboardGamepad() {
    RebindKeyboard (changeToKeyboard);
    changeToKeyboard = !changeToKeyboard;
    sceneControl.SetUsePad (playerID, changeToKeyboard);
  }
	
  void RebindKeyboard(bool useKeyConfig) {
    for (int i = 0; i < keyButtons.Length; i++)
      keyButtons [i].SetActive(useKeyConfig);
    for (int i = 0; i < padButtons.Length; i++)
      padButtons [i].SetActive(!useKeyConfig);

    InputManager.SetInputConfiguration (useKeyConfig ? keyConfig : padConfig, playerID);
  }
}
