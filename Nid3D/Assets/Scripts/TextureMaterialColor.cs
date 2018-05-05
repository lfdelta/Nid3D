using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextureMaterialColor : MonoBehaviour {

  public Material referenceMaterial;
  private Image sprite;

  void Awake () {
    sprite = GetComponent<Image> ();
    sprite.color = referenceMaterial.color;
  }
}
