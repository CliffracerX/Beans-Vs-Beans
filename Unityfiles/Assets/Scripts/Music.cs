using UnityEngine;
using System.Collections;

public class Music : MonoBehaviour
{
	public AudioClip[] nearGameOver;
	public AudioClip[] normal;
	public string status = "normal";
	public Shoot shootS;

	void Update()
	{
		if(shootS.health<(shootS.maxhealth/2) && status=="normal")
		{
			status="neardeath";
			audio.clip=nearGameOver[Random.Range(0, nearGameOver.Length)];
			//audio.Play();
		}
		else if(shootS.health>(shootS.maxhealth/2) && status=="neardeath")
		{
			status="normal";
			audio.clip=normal[Random.Range(0, normal.Length)];
			//audio.Play();
		}
		if(audio.isPlaying==false && status=="normal")
		{
			audio.clip=normal[Random.Range(0, normal.Length)];
			//audio.Play();
		}
		if(audio.isPlaying==false && status=="neardeath")
		{
			audio.clip=nearGameOver[Random.Range(0, nearGameOver.Length)];
			//audio.Play();
		}
	}
}