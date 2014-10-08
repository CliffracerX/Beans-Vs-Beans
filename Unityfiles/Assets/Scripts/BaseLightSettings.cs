using UnityEngine;
using System.Collections;

public class BaseLightSettings : MonoBehaviour
{
	public Color[] colors;
	public int currentMonth = 1;
	void Start()
	{
		currentMonth = System.DateTime.Now.Month;
		light.color=colors[currentMonth-1];
	}
}

