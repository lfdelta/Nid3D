using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TeamUtility.IO;

public class SceneController : MonoBehaviour {

  private static string defaultInputsPath;
  private static string userPrefsPath;

  [HideInInspector] public float volume;
  [HideInInspector] public bool P1usePad, P2usePad;
  [HideInInspector] public int P1padNum, P2padNum;

  void Awake () {
    defaultInputsPath = System.IO.Path.Combine (Application.persistentDataPath, "default_input_config.xml");
    // copy default input configuration
    /*TextAsset defaultfile = Resources.Load ("default_input_config.xml") as TextAsset;
    System.IO.File.WriteAllText (defaultInputsPath, defaultfile.text);*/

    userPrefsPath = System.IO.Path.Combine (Application.persistentDataPath, "user_prefs.txt");

    // if no preferences are found, set default values
    if (!System.IO.File.Exists (userPrefsPath)) {
      volume = 0.5f;
      P1usePad = false;
      P2usePad = false;
      P1padNum = 0;
      P2padNum = 0;
      SavePreferences ();
    } else {
      LoadPreferences ();
    }

    // if there is no default configuration, save the current configuration as the default
    if (!System.IO.File.Exists(defaultInputsPath))
      InputManager.Save (defaultInputsPath);

    LoadInputs ();
	}

  void Start() {
    LoadInputs ();
  }
	


  public void SaveInputs() {
    InputManager.Save ();
  }

  public void LoadInputs() {
    InputManager.Load ();
  }

  public void LoadDefaultInputs() {
    InputManager.Load (defaultInputsPath);
  }



  void SavePreferences() {
    string[] prefStr = new string[5];
    prefStr[0] = volume.ToString();
    prefStr[1] = P1usePad.ToString ();
    prefStr[2] = P2usePad.ToString();
    prefStr[3] = P1padNum.ToString ();
    prefStr[4] = P2padNum.ToString ();
    System.IO.File.WriteAllLines (userPrefsPath, prefStr);
  }

  void LoadPreferences() {
    string[] prefContents = System.IO.File.ReadAllLines (userPrefsPath);
    volume = float.Parse (prefContents [0]);
    P1usePad = bool.Parse (prefContents [1]);
    P2usePad = bool.Parse (prefContents [2]);
    P1padNum = int.Parse (prefContents [3]);
    P2padNum = int.Parse (prefContents [4]);
  }

  public void SetVol(float vol) {
    volume = vol;
    SavePreferences ();
  }

  public void SetUsePad(PlayerID pid, bool p) {
    switch (pid) {
    case PlayerID.One:
      P1usePad = p;
      break;
    default:
      P2usePad = p;
      break;
    }
    SavePreferences ();
  }

  public void SetPadNum(PlayerID pid, int n) {
    switch (pid) {
    case PlayerID.One:
      P1padNum = n;
      break;
    default:
      P2padNum = n;
      break;
    }
    SavePreferences ();
  }



  public void AsyncLoadSceneByName(string scene) {
    StartCoroutine (AsyncLoadScene (scene));
  }

  public void AsyncReloadScene() {
    StartCoroutine (AsyncLoadScene (SceneManager.GetActiveScene().name));
  }

  IEnumerator AsyncLoadScene(string scene) {
    AsyncOperation loadScene = SceneManager.LoadSceneAsync (scene);

    while (!loadScene.isDone)
      yield return null;
  }

  public void QuitApp() {
    Application.Quit ();
  }
}
