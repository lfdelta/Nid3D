using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TeamUtility.IO;

public class Tools : MonoBehaviour {

  public delegate bool ButtonGet(string s, PlayerID p);

	public static int Clamp (int val, int min, int max) {
		return (val < min) ? min : ((val > max) ? max : val);
	}

  public static bool CheckButtonBothPlayers(ButtonGet f, string button) {
    return f (button, PlayerID.One) || f (button, PlayerID.Two);
  }
}
