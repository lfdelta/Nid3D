using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tools : MonoBehaviour {

	public static int Clamp (int val, int min, int max) {
		return (val < min) ? min : ((val > max) ? max : val);
	}
}
