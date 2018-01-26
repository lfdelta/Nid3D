//using System.Nullable;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TeamUtility.IO;

public class GameController : MonoBehaviour {
  public Canvas pauseUI;
  public GameObject startPanel;
  public GameObject[] otherPanels;

  private System.Nullable<PlayerID> rightOfWay;
  private bool gameIsPaused;
  private CharController[] players;
  //private CameraController cam;

	void Awake () {
    pauseUI.enabled = false;
    gameIsPaused = false;
    rightOfWay = null;
	}

  void Start () {
    players = GetPlayers (4);
  }

  void Update() {
    bool pauseButton = Tools.CheckButtonBothPlayers(InputManager.GetButtonDown, "Pause");

    if (pauseButton) {
      if (gameIsPaused)
        UnpauseGame();
      else
        PauseGame ();
    }
  }

  // *** CURRENTLY ONLY WORKS FOR TWO PLAYERS (not designed to handle more)
  // determine right-of-way based upon player deaths
  // this function is meant to be called by the CharController upon player death
  void PlayerDied(PlayerID deadPlayer) {
    for (int i = 0; i < players.Length; i++) {
      CharController p = players [i];
      if (p.playerid != deadPlayer) {
        if (p.isDead) // verbosely written to appease the type checker
          rightOfWay = null;
        else
          rightOfWay = p.playerid;
      }
    }

    //Debug.Log (rightOfWay);
  }

  void PauseGame() {
    Time.timeScale = 0;
    gameIsPaused = true;

    pauseUI.enabled = true;
    startPanel.SetActive (true);
    for (int i = 0; i < otherPanels.Length; i++)
      otherPanels [i].SetActive(false);
  }

  void UnpauseGame() {
    Time.timeScale = 1;
    gameIsPaused = false;

    pauseUI.enabled = false;
  }

  public bool IsPaused() {
    return gameIsPaused;
  }

  public void SetVolume(float vol) {
    AudioListener.volume = Mathf.Clamp01 (vol);
  }

  CharController[] GetPlayers(int maxNum) {
    // collect the first maxNum CharControllers in the scene
    Object[] allChars = Object.FindObjectsOfType(typeof(CharController));
    int size = (maxNum < allChars.Length) ? maxNum : allChars.Length;

    CharController[] players = new CharController[size];
    for (int i = 0; i < size; i++) {
      players [i] = (CharController)allChars [i];
    }
    return players;
  }
}
