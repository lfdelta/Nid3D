using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TeamUtility.IO;

public class GameController : MonoBehaviour {

  public Canvas pauseUI;

  private bool gameIsPaused;

	void Start () {
    pauseUI.enabled = false;
    gameIsPaused = false;
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

  public void PauseGame() {
    Time.timeScale = 0;
    pauseUI.enabled = true;
    gameIsPaused = true;
  }

  public void UnpauseGame() {
    Time.timeScale = 1;
    pauseUI.enabled = false;
    gameIsPaused = false;
  }

  public bool IsPaused() {
    return gameIsPaused;
  }
}
