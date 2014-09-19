using UnityEngine;
using System.Collections;

public class ProjectileScript : MonoBehaviour
{
	public int fadeTime = 500;
	public int damageDone = 5;
	void FixedUpdate()
	{
		if(fadeTime>0)
			fadeTime--;
		if(fadeTime<=0)
			Destroy(this.gameObject);
	}
}
