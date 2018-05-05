using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
//using UnityEngine.SceneManagement;
using TeamUtility.IO;

public class GameController : NodeTraversal {
  public WorldNodeScript startingNode;
  public float victoryMessageTime = 2;
  public Canvas HUD;
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
  private GameObject ROWp1, ROWp2;

  private CharController[] players;
  private Transform[] livePlayers;
  private CameraController cam;
  private Vector3 avgPlayerPos;

  private WorldNodeScript playerNode;
  private RightOfWayWall leftWall;
  private RightOfWayWall rightWall;
  private EndZone[] endZones;
  private bool initializedGame = false;

  private SceneController sceneControl;

  void Awake () {
    UnpauseGame(); // this allows new scenes to start fresh even when loaded from the pause menu
    rightOfWay = null;
    victor = null;
    playerNode = startingNode;

    cam = FindObjectOfType<CameraController> ();
    endZones = FindObjectsOfType<EndZone> ();

    leftWall = ((GameObject)Instantiate (wallPrefab)).GetComponent<RightOfWayWall> ();
    rightWall = ((GameObject)Instantiate (wallPrefab)).GetComponent<RightOfWayWall> ();
    leftWall.Initialize (PlayerID.One, startingNode);
    rightWall.Initialize (PlayerID.Two, startingNode);

    sceneControl = FindObjectOfType<SceneController> ();

    ROWp1 = HUD.transform.GetChild (0).gameObject;
    ROWp2 = HUD.transform.GetChild (1).gameObject;
	}

  void Start () {
    players = GetPlayers ();
    livePlayers = new Transform[players.Length];
    for (int i = 0; i < players.Length; i++)
      livePlayers [i] = players [i].transform;

    GetAvgPlayerPositionAndNode ();
    SendAvgPlayerPositionAndNode ();
    SendRightOfWay ();

    sceneControl.LoadInputs ();
  }

  void Update() {
    // if a player wins, wait while a message displays, then load the menu scene
    if (victor != null && Time.time - winTime > victoryMessageTime)
      sceneControl.AsyncLoadSceneByName("Main Menu");

    bool pauseButton = Tools.CheckButtonBothPlayers(InputManager.GetButtonDown, "Pause");

    if (pauseButton) {
      if (gameIsPaused) {
        UnpauseGame ();
        sceneControl.SaveInputs ();
      } else {
        PauseGame ();
      }
    }

    GetAvgPlayerPositionAndNode ();
    SendAvgPlayerPositionAndNode ();

    // heavy-handed solution to walls freezing in default position on scene load
    // we can't do this in Start() because the node bisectors are still uninitialized
    if (!initializedGame) {
      SetUpNodeBisectors ();
      leftWall.SendMessage ("PlaceSelfAndRotate", false);
      rightWall.SendMessage ("PlaceSelfAndRotate", false);
      initializedGame = true;
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
    
  // evaluate rightOfWay based upon given player's death
  // CURRENTLY ONLY WORKS FOR TWO PLAYERS (not designed to handle more)
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

    ROWp1.SetActive (rightOfWay == PlayerID.One);
    ROWp2.SetActive (rightOfWay == PlayerID.Two);  

    SendRightOfWay ();
  }

  void SendRightOfWay() {
    cam.UpdateROW (rightOfWay);
    leftWall.UpdateROW (rightOfWay);
    rightWall.UpdateROW (rightOfWay);
    for (int i = 0; i < endZones.Length; i++)
      endZones [i].UpdateROW (rightOfWay);
  }

  // meant to be called from inside of CharController
  // respawns the given player in front of the other; if both are dead, they respawn centered between the ROW walls
  public Vector3 PlayerRespawnLoc(PlayerID player, float distance) {
    Vector3 startLoc = (rightOfWay != null) ? avgPlayerPos : (rightWall.transform.position + leftWall.transform.position) / 2;
    float dist = (rightOfWay != null) ? distance : distance / 2;
    return PositionAlongSegments (dist, startLoc, playerNode, (player == PlayerID.Two));
  }



  // meant to be called by EndZone script
  void PlayerWonGame(PlayerID pid) {
    if (victor != null) // ignore the message if somebody already won
      return;
    
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

  public void UnpauseGame() {
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

  // make sure the WorldNode bisectors are all pointing in the same "half-space" defined by the line segments
  // ie on a spiral section all of them will be pointing "inward" or all of them will pointing "outward"
  // this ensures that the ROW walls don't over-rotate when interpolating between adjacent nodes
  void SetUpNodeBisectors() {
    // walk to the beginning of the list; its bisector is the following segment's normal vector
    WorldNodeScript tmp = startingNode;
      while (tmp.prevNode != null)
      tmp = tmp.prevNode;
    Vector3 nhat = tmp.bisectorHat;
    tmp = tmp.nextNode;

    // make sure each bisector is facing the right direction, based upon the previous segment's normal vector
    while (tmp != null) {
      if (Vector3.Dot (tmp.bisectorHat, nhat) < 0)
        tmp.bisectorHat = -tmp.bisectorHat;
      
      nhat = WorldNodeScript.PerpendicularVector (tmp.segmentHat);
      if (Vector3.Dot (tmp.bisectorHat, nhat) < 0)
        nhat = -nhat;
      
      tmp = tmp.nextNode;
    }
  }
}
