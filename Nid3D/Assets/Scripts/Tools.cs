using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TeamUtility.IO;

public class Tools : MonoBehaviour {

  public delegate bool ButtonGet(string s, PlayerID p);
  public delegate float AxisGet(string s, PlayerID p);

  public static int Clamp (int val, int min, int max) {
    return (val < min) ? min : ((val > max) ? max : val);
  }

  public static bool CheckButtonBothPlayers(ButtonGet f, string button) {
    return f (button, PlayerID.One) || f (button, PlayerID.Two);
  }

  public static float CheckAxisBothPlayers(AxisGet f, string ax) {
    return Mathf.Max (f (ax, PlayerID.One), f (ax, PlayerID.Two));
  }

  public static PlayerID OtherPlayer(PlayerID thisPlayer) {
    switch (thisPlayer) {
    case PlayerID.One:
      return PlayerID.Two;
    default:
      return PlayerID.One;
    }
  }
}
