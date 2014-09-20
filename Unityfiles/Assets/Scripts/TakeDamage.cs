using UnityEngine;
using System.Collections;

public class TakeDamage : MonoBehaviour
{
	public Shoot shootScript;
	public int ammoCrateBullets = 12;
	public int SammoCrateBullets = 8;
	public int HammoCrateBullets = 4;
	public int medkitHealth = 25;
	public int ammoCrateRespawntime = 5000;
	public int medkitRespawntime = 5000;
	public int SammoCrateRespawntime = 5000;
	public int HammoCrateRespawntime = 5000;
	public AudioClip pickupSound;
	
	void OnTriggerEnter(Collider other)
	{
		if(other.tag=="Instadeath")
		{
			shootScript.Instakill();
		}
		if(other.tag=="Bullet")
		{
			ProjectileScript proj = other.gameObject.GetComponent<ProjectileScript>();
			if(proj.networkView.isMine)
			{
				shootScript.networkView.RPC("RecalcHealthMeter", RPCMode.All, shootScript.health-proj.damageDone);
				proj.networkView.RPC("DestroyO", RPCMode.All);
				Destroy(proj.gameObject);
			}
		}
		if(other.tag=="PrimaryAmmoCrate")
		{
			RotatingLoot loot = other.gameObject.GetComponent<RotatingLoot>();
			if(loot.enabled)
			{
				shootScript.primaryAmmoLeft+=ammoCrateBullets;
				loot.enabled=false;
				loot.timeUntilRespawn=ammoCrateRespawntime;
				loot.renderer.enabled=false;
				shootScript.RecalcAmmoMeter();
				audio.clip=pickupSound;
				audio.Play();
			}
		}
		if(other.tag=="SpecialAmmoCrate")
		{
			RotatingLoot loot = other.gameObject.GetComponent<RotatingLoot>();
			if(loot.enabled)
			{
				shootScript.specialAmmoLeft+=SammoCrateBullets;
				loot.enabled=false;
				loot.timeUntilRespawn=SammoCrateRespawntime;
				loot.renderer.enabled=false;
				shootScript.RecalcAmmoMeter();
				audio.clip=pickupSound;
				audio.Play();
			}
		}
		if(other.tag=="HeavyAmmoCrate")
		{
			RotatingLoot loot = other.gameObject.GetComponent<RotatingLoot>();
			if(loot.enabled)
			{
				shootScript.heavyAmmoLeft+=HammoCrateBullets;
				loot.enabled=false;
				loot.timeUntilRespawn=HammoCrateRespawntime;
				loot.renderer.enabled=false;
				shootScript.RecalcAmmoMeter();
				audio.clip=pickupSound;
				audio.Play();
			}
		}
		if(other.tag=="Medkit")
		{
			RotatingLoot loot = other.gameObject.GetComponent<RotatingLoot>();
			if(loot.enabled && shootScript.health<shootScript.maxhealth)
			{
				shootScript.networkView.RPC("RecalcHealthMeter", RPCMode.All, shootScript.health+medkitHealth);
				loot.enabled=false;
				loot.timeUntilRespawn=medkitRespawntime;
				loot.renderer.enabled=false;
				audio.clip=pickupSound;
				audio.Play();
			}
		}
	}
}
