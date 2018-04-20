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

	void Awake () {
    string pstr = (playerID == PlayerID.One) ? "P1" : "P2";
    keyConfig = pstr + "Keyboard";
    padConfig = pstr + "Gamepad";

    Toggle t = GetComponent<Toggle> ();
    changeToKeyboard = t.isOn; // if it starts on, gamepad is active; switch to keyboard if toggled off

    RebindKeyboard (!changeToKeyboard);
	}

  public void ToggleKeyboardGamepad() {
    RebindKeyboard (changeToKeyboard);
    changeToKeyboard = !changeToKeyboard;
  }
	
  void RebindKeyboard(bool useKeyConfig) {
    for (int i = 0; i < keyButtons.Length; i++)
      keyButtons [i].SetActive(useKeyConfig);
    for (int i = 0; i < padButtons.Length; i++)
      padButtons [i].SetActive(!useKeyConfig);

    InputManager.SetInputConfiguration (useKeyConfig ? keyConfig : padConfig, playerID);
  }
}
