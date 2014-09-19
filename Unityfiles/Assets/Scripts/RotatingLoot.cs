using UnityEngine;
using System.Collections;

public class RotatingLoot : MonoBehaviour
{
	public int timeUntilRespawn = 5000;
	public bool enabled = true;
	void Update()
	{
		transform.Rotate(new Vector3(0, 30, 0)*Time.deltaTime);
		if(!enabled)
			timeUntilRespawn--;
		if(timeUntilRespawn<1)
		{
			enabled=true;
			renderer.enabled=true;
		}
	}
}
