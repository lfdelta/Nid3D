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
  private Transform[] livePlayers;
  private CameraController cam;
  private Vector3 avgPlayerPos;

	void Awake () {
    pauseUI.enabled = false;
    gameIsPaused = false;
    rightOfWay = null;
	}

  void Start () {
    players = GetPlayers ();
    livePlayers = new Transform[players.Length];
    for (int i = 0; i < players.Length; i++)
      livePlayers [i] = players [i].transform;

    cam = FindObjectOfType<CameraController> ();
    cam.UpdateROW(rightOfWay);
  }

  void Update() {
    bool pauseButton = Tools.CheckButtonBothPlayers(InputManager.GetButtonDown, "Pause");

    if (pauseButton) {
      if (gameIsPaused)
        UnpauseGame();
      else
        PauseGame ();
    }

    GetAvgPlayerPosition ();
    cam.avgpos = avgPlayerPos;
  }



  // * this function is meant to be called by the CharController upon player death and revival
  // * parameter should be a 2-element list of the form {PlayerID, bool}
  // update rightOfWay and camera targets based upon player death or revival
  void PlayerIsAlive(PlayerAlive pinfo) {
    if (!pinfo.alive)
      UpdateRightOfWay (pinfo.playerid);

    GetLivePlayers ();
  }

  void GetLivePlayers() {
    int alive = 0;
    for (int i = 0; i < players.Length; i++)
      if (!players [i].isDead)
        alive++;
    
    livePlayers = new Transform [alive];
    int j = 0;
    for (int i = 0; i < players.Length; i++) {
      if (!players [i].isDead)
        livePlayers [j++] = players [i].transform;
    }
  }

  Vector3 GetAvgPlayerPosition() {
    if (livePlayers.Length > 0) {
      avgPlayerPos = Vector3.zero;
      for (int i = 0; i < livePlayers.Length; i++)
        avgPlayerPos += livePlayers [i].position;
      avgPlayerPos /= livePlayers.Length;
    }

    return avgPlayerPos;
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

    cam.UpdateROW (rightOfWay);

    Debug.Log ("ROW: " + rightOfWay.ToString());
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

  // return an array of all CharControllers in the scene
  CharController[] GetPlayers() {
    Object[] allChars = Object.FindObjectsOfType(typeof(CharController));

    CharController[] players = new CharController[allChars.Length];
    for (int i = 0; i < allChars.Length; i++)
      players [i] = (CharController)allChars [i];

    return players;
  }
}
