using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TeamUtility.IO;

public class SceneController : MonoBehaviour {

  private string defaultInputsPath;

  void Awake () {
    defaultInputsPath = System.IO.Path.Combine (Application.persistentDataPath, "default_input_config.xml");

    // if there is no default configuration, save the current configuration as the default
    if (!System.IO.File.Exists(defaultInputsPath))
      InputManager.Save (defaultInputsPath);
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
