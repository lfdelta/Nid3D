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
  private CameraController cam;

	void Awake () {
    pauseUI.enabled = false;
    gameIsPaused = false;
    rightOfWay = null;
	}

  void Start () {
    players = GetPlayers ();

    cam = FindObjectOfType<CameraController> ();
    UpdateCamera ();
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



  // * this function is meant to be called by the CharController upon player death and revival
  // * parameter should be a 2-element list of the form {PlayerID, bool}
  // update rightOfWay and camera targets based upon player death or revival
  void PlayerIsAlive(PlayerAlive pinfo) {
    if (!pinfo.alive)
      UpdateRightOfWay (pinfo.playerid);

    UpdateCamera ();
  }

  // *** CURRENTLY ONLY WORKS FOR TWO PLAYERS (not designed to handle more)
  // evaluate rightOfWay based upon given player's death
  void UpdateRightOfWay(PlayerID deadPlayer) {
    for (int i = 0; i < players.Length; i++) {
      CharController p = players [i];
      if (p.playerid != deadPlayer) {
        if (p.isDead) // verbosely written to appease the type checker
          rightOfWay = null;
        else
          rightOfWay = p.playerid;
      }
    }

    Debug.Log ("ROW: " + rightOfWay.ToString());
  }

  // tell camera to target only living players, and update rightOfWay
  void UpdateCamera () {
    int alive = 0;
    for (int i = 0; i < players.Length; i++)
      if (!players [i].isDead)
        alive++;

    Transform[] ts = new Transform [alive];
    int j = 0;
    for (int i = 0; i < players.Length; i++) {
      if (!players [i].isDead)
        ts [j++] = players [i].transform;
    }

    cam.targets = ts;
    cam.rightOfWay = rightOfWay;
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

  // return a list of all CharControllers in the scene
  CharController[] GetPlayers() {
    Object[] allChars = Object.FindObjectsOfType(typeof(CharController));

    CharController[] players = new CharController[allChars.Length];
    for (int i = 0; i < allChars.Length; i++)
      players [i] = (CharController)allChars [i];

    return players;
  }

  /*CharController[] GetPlayers(int maxNum) {
    // collect the first maxNum CharControllers in the scene
    Object[] allChars = Object.FindObjectsOfType(typeof(CharController));
    int size = (maxNum < allChars.Length) ? maxNum : allChars.Length;

    CharController[] players = new CharController[size];
    for (int i = 0; i < size; i++) {
      players [i] = (CharController)allChars [i];
    }
    return players;
  }*/
}
