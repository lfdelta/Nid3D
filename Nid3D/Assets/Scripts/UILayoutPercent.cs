using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// if root, create an invisible rectangle from upper-left and lower-right screen coordinates, listed in percentages
// place all of the child objects at the given percentage coordinates WITHIN the "virtual screen" this object produces

public class UILayoutPercent : MonoBehaviour {

  public class layoutRect {
    public RectTransform rect;
    public Vector2 upperLeft;
    public Vector2 lowerRight;
  }

  public bool root;
  public layoutRect[] children;

	void Start () {
    if (root)
      SetChildren ();
	}
	
  public void SetChildren () {
    for (int i = 0; i < children.Length; i++) {
      layoutRect child = children [i];
      child.rect.position = new Vector3 (child.upperLeft.x, child.upperLeft.y, 0);

      UILayoutPercent uilp = child.rect.GetComponent<UILayoutPercent> ();
      if (uilp)
        uilp.SetChildren ();
    }
  }
}
