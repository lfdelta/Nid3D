using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TeamUtility.IO;

public class GameController : NodeTraversal {
  public WorldNodeScript startingNode;
  public float victoryMessageTime = 2;
  public Canvas pauseUI;
  public GameObject startPanel;
  public GameObject[] otherPanels;
  public Object wallPrefab;

  private System.Nullable<PlayerID> rightOfWay;
  private System.Nullable<PlayerID> victor;
  private float winTime;
  private string winText;
  private GUIStyle winFormat;
  private Rect winRect;
  private bool gameIsPaused;
  private CharController[] players;
  private Transform[] livePlayers;
  private CameraController cam;
  private Vector3 avgPlayerPos;

  private WorldNodeScript playerNode;
  private RightOfWayWall leftWall;
  private RightOfWayWall rightWall;
  private bool initializedWalls = false;

  void Awake () {
    pauseUI.enabled = false;
    gameIsPaused = false;
    rightOfWay = null;
    victor = null;
    playerNode = startingNode;

    cam = FindObjectOfType<CameraController> ();

    leftWall = ((GameObject)Instantiate (wallPrefab)).GetComponent<RightOfWayWall> ();
    rightWall = ((GameObject)Instantiate (wallPrefab)).GetComponent<RightOfWayWall> ();
    leftWall.Initialize (PlayerID.One, startingNode);
    rightWall.Initialize (PlayerID.Two, startingNode);
	}

  void Start () {
    players = GetPlayers ();
    livePlayers = new Transform[players.Length];
    for (int i = 0; i < players.Length; i++)
      livePlayers [i] = players [i].transform;

    GetAvgPlayerPositionAndNode ();
    SendAvgPlayerPositionAndNode ();
    SendRightOfWay ();
  }

  void Update() {
    // if a player wins, wait while a message displays, then reload the scene (later, load a menu scene)
    if (victor != null && Time.time - winTime > victoryMessageTime)
      SceneManager.LoadScene (SceneManager.GetActiveScene ().buildIndex);

    bool pauseButton = Tools.CheckButtonBothPlayers(InputManager.GetButtonDown, "Pause");

    if (pauseButton) {
      if (gameIsPaused)
        UnpauseGame();
      else
        PauseGame ();
    }

    GetAvgPlayerPositionAndNode ();
    SendAvgPlayerPositionAndNode ();

    // heavy-handed solution to walls freezing in default position on scene load
    // we can't do this in Start() because the node bisectors are still uninitialized
    if (!initializedWalls) {
      leftWall.SendMessage ("PlaceSelfAndRotate", false);
      rightWall.SendMessage ("PlaceSelfAndRotate", false);
      initializedWalls = true;
    }
  }

  void OnGUI() {
    if (victor != null)
      GUI.Label (winRect, winText, winFormat);
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

  void GetAvgPlayerPositionAndNode() {
    if (livePlayers.Length > 0) {
      avgPlayerPos = Vector3.zero;
      for (int i = 0; i < livePlayers.Length; i++)
        avgPlayerPos += livePlayers [i].position;
      avgPlayerPos /= livePlayers.Length;
    }

    playerNode = CheckNodeTransition (avgPlayerPos, playerNode);
  }

  void SendAvgPlayerPositionAndNode() {
    cam.UpdatePlayerInfo (avgPlayerPos, playerNode);
    leftWall.UpdatePlayerInfo (avgPlayerPos, playerNode);
    rightWall.UpdatePlayerInfo (avgPlayerPos, playerNode);
  }

  // *** CURRENTLY ONLY WORKS FOR TWO PLAYERS (not designed to handle more)
  // evaluate rightOfWay based upon given player's death
  void UpdateRightOfWay(PlayerID deadPlayer) {
    for (int i = 0; i < players.Length; i++) {
      CharController p = players [i];
      if (p.playerid != deadPlayer) {
        if (p.isDead) // type checker won't accept ternary operator
          rightOfWay = null;
        else
          rightOfWay = p.playerid;
      }
    }

    SendRightOfWay ();

    Debug.Log ("ROW: " + rightOfWay.ToString());
  }

  void SendRightOfWay() {
    cam.UpdateROW (rightOfWay);
    leftWall.UpdateROW (rightOfWay);
    rightWall.UpdateROW (rightOfWay);
  }

  // meant to be called from inside of CharController
  // respawns the given player in front of the other; if both are dead, they respawn centered between the ROW walls
  public Vector3 PlayerRespawnLoc(PlayerID player, float dist) {
    Vector3 startLoc = (rightOfWay != null) ? avgPlayerPos : (rightWall.transform.position + leftWall.transform.position) / 2;
    return PositionAlongSegments (dist, startLoc, playerNode, (player == PlayerID.Two));
  }



  // meant to be called by EndZone script
  void PlayerWonGame(PlayerID pid) {
    victor = pid;
    winTime = Time.time;
    winText = "<color=red>Player " + victor.ToString() + " Wins!</color>";
    float w = Screen.width, h = Screen.height;
    winRect = new Rect (w/4, h/4, w/2, h/2);
    winFormat = new GUIStyle ();
    winFormat.fontSize = 100;
    winFormat.richText = true;
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
