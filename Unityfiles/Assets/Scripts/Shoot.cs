using UnityEngine;
using System.Collections;

public class Shoot : MonoBehaviour
{
	public GameObject mainGun;
	public GameObject mainShot;
	public bool mainGunOut;
	public int mainGunBulletspeed = 100;
	public int mainGunClipsize = 6;
	public int ammoInMainGun = 6;
	public int mainGunFireSpeed=60;  //Higher values fire slower.  It's weird, I know.  -CliffracerX
	public int timeSinceLMF=60;  //Short for "Time since last main fire", increases every tick
	public int primaryAmmoLeft = 60;
	public GUIText ammoCount;
	public Vector3 respawnPoint;
	public float health = 100;
	public float maxhealth = 100;
	public int timeUntilRespawn = 500;
	public GameObject player;
	public Vector3 playerpos;
	public int timeSinceLastDamageTaken = 100;
	public int timeUntilHPRegen = 10;
	public float healthBarLength = (Screen.width / 4) * (100/50);
	public GUISkin skin;
	public GUISkin skin2;
	public GameObject bulletPoint;
	public AudioClip[] fireSounds;
	public AudioClip[] reloadSounds;
	public int respawnSpeed = 500;
	public GameObject specialGun;
	public GameObject specialShot;
	public bool specialGunOut;
	public int specialGunBulletspeed = 100;
	public int specialGunClipsize = 4;
	public int ammoInSpecialGun = 4;
	public int specialGunFireSpeed=120;  //Higher values fire slower.  It's weird, I know.  -CliffracerX
	public int timeSinceLSF=120;  //Short for "Time since last main fire", increases every tick
	public int specialAmmoLeft = 40;
	public GameObject heavyGun;
	public GameObject heavyShot;
	public bool heavyGunOut;
	public int heavyGunBulletspeed = 100;
	public int heavyGunClipsize = 2;
	public int ammoInHeavyGun = 2;
	public int heavyGunFireSpeed=240;  //Higher values fire slower.  It's weird, I know.  -CliffracerX
	public int timeSinceLHF=240;  //Short for "Time since last main fire", increases every tick
	public int heavyAmmoLeft = 20;
	public AudioLowPassFilter filt1;
	public AudioLowPassFilter filt2;
	public AudioLowPassFilter filt3;
	public AudioClip deathNoise;
	public string team = "Red";
	public NetworkManager netMan;
	public TakeDamage dmg;
	public GameObject mainGunR;
	public GameObject specialGunR;
	public GameObject heavyGunR;
	public bool takenDamageThisTick = false;
	public bool teamSubtract = true;

	[RPC] public void RecalcHealthMeter(float healthNetworked)
	{
		//if(!takenDamageThisTick)
		{
		health = healthNetworked;
		if(health>maxhealth)
			health=maxhealth;
		filt1.cutoffFrequency=health*10+500;
		filt2.cutoffFrequency=health*10+500;
		filt3.cutoffFrequency=health*10+500;
		if(player.networkView.isMine)
		{
			if(health>0)
				Camera.main.fieldOfView=60 + (0.25f/(health / maxhealth));
			else
				Camera.main.fieldOfView=62;
		}
		takenDamageThisTick=true;
		}
	}

	void OnGUI()
	{
		if(player.networkView.isMine)
		{
		GUI.Box(new Rect((Screen.width/2)-(Screen.width/4), 10, Screen.width/4, 20),"",skin2.box);
		healthBarLength = (Screen.width / 4) * (health / (float)maxhealth);
			GUI.Box(new Rect((Screen.width/2)-((Screen.width/4)*(health / (float)maxhealth)), 10, ((Screen.width/4)*(health / (float)maxhealth))*(health / (float)maxhealth), 20),health + "/" + maxhealth,skin.box);
		}
	}

	public void Instakill()
	{
		networkView.RPC("RecalcHealthMeter", RPCMode.All, 0f);
		timeUntilRespawn=respawnSpeed;
		playerpos=player.transform.position;
	}

	public void RecalcAmmoMeter()
	{
		if(mainGunOut)
			ammoCount.text="Ammo (Primary): "+ammoInMainGun+" / "+primaryAmmoLeft;
		if(specialGunOut)
			ammoCount.text="Ammo (Special): "+ammoInSpecialGun+" / "+specialAmmoLeft;
		if(heavyGunOut)
			ammoCount.text="Ammo (Heavy): "+ammoInHeavyGun+" / "+heavyAmmoLeft;
		if(!mainGunOut && !specialGunOut && !heavyGunOut)
			ammoCount.text="Ammo (Primary, Special, Heavy): "+(primaryAmmoLeft+ammoInMainGun)+" / "+(specialAmmoLeft+ammoInSpecialGun)+" / "+(heavyAmmoLeft+ammoInHeavyGun);
	}

	void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info)
	{
		int teamS = 0;
		if(stream.isWriting)
		{
			if(team=="Red")
				teamS=0;
			else if(team=="Blue")
				teamS=1;
			stream.Serialize(ref teamS);
		}
		else
		{
			stream.Serialize(ref teamS);
			networkView.RPC("SetTeam", RPCMode.All, teamS);
		}
	}

	void Start()
	{
		RecalcAmmoMeter();
		respawnPoint = transform.position;
		dmg = player.GetComponent("TakeDamage") as TakeDamage;
		if(!networkView.isMine)
		{
			health=0;
			timeUntilRespawn=2;
		}
	}

	public void SetNetworkManager(NetworkManager net)
	{
		netMan=net;
	}

	[RPC] void SetTeam(int teamI)
	{
		if(teamI==0)
		{
			team="Red";
			player.renderer.material.color=new Color(1, 0, 0);
			mainGunR.renderer.material.color=new Color(0.75f, 0.25f, 0.25f);
			specialGunR.renderer.material.color=new Color(0.75f, 0.25f, 0.25f);
			heavyGunR.renderer.material.color=new Color(0.75f, 0.25f, 0.25f);
			respawnPoint=NetworkManager.redSpawnV;
			NetworkManager.redPlayers+=1;
		}
		else if(teamI==1)
		{
			team="Blue";
			player.renderer.material.color=new Color(0, 0, 1);
			mainGunR.renderer.material.color=new Color(0.25f, 0.25f, 0.75f);
			specialGunR.renderer.material.color=new Color(0.25f, 0.25f, 0.75f);
			heavyGunR.renderer.material.color=new Color(0.25f, 0.25f, 0.75f);
			respawnPoint=NetworkManager.blueSpawnV;
			NetworkManager.bluePlayers+=1;
		}
	}
	
	void Update()
	{
		if(player.networkView.isMine)
		{
			takenDamageThisTick=false;
			if(health<1 && timeUntilRespawn==respawnSpeed)
			{
				audio.clip=deathNoise;
				audio.Play();
				if(team=="Red" && teamSubtract)
				{
					NetworkManager.redPlayers-=1f;
					teamSubtract=false;
					netMan.blueTeamScore+=1;
				}
				if(team=="Blue" && teamSubtract)
				{
					NetworkManager.bluePlayers-=1f;
					teamSubtract=false;
					netMan.redTeamScore+=1;
				}
				playerpos=player.transform.position;
				playerpos.y-=1;
			}
			if(health<1)
			{
				ammoCount.color=Color.red;
				ammoCount.text="You're dead!";
				player.transform.position=playerpos;
				if(team=="Red")
				{
					if(netMan.blueTeamScore<netMan.numberOfPointsToWin)
						timeUntilRespawn--;
				}
				else if(team=="Blue")
				{
					if(netMan.redTeamScore<netMan.numberOfPointsToWin)
						timeUntilRespawn--;
				}
				networkView.RPC("EnableGun", RPCMode.All, 0, false);
				mainGunOut=false;
				networkView.RPC("EnableGun", RPCMode.All, 1, false);
				specialGunOut=false;
				networkView.RPC("EnableGun", RPCMode.All, 2, false);
				heavyGunOut=false;
			}
			if(health<1 && timeUntilRespawn<1)
			{
				primaryAmmoLeft=mainGunClipsize*3;
				ammoInMainGun=mainGunClipsize;
				specialAmmoLeft=specialGunClipsize*1;
				ammoInSpecialGun=specialGunClipsize;
				heavyAmmoLeft=heavyGunClipsize*0;
				ammoInHeavyGun=heavyGunClipsize*0;
				timeUntilRespawn=respawnSpeed;
				RecalcAmmoMeter();
				ammoCount.color=Color.white;
				networkView.RPC("RecalcHealthMeter", RPCMode.All, maxhealth);
				if(NetworkManager.redPlayers<NetworkManager.bluePlayers)
				{
					networkView.RPC("SetTeam", RPCMode.All, 0);
				}
				else if(NetworkManager.bluePlayers<NetworkManager.redPlayers)
				{
					networkView.RPC("SetTeam", RPCMode.All, 1);
				}
				else if(NetworkManager.redPlayers==NetworkManager.bluePlayers)
				{
					if(Random.Range(0, 10)<=5)
					{
						networkView.RPC("SetTeam", RPCMode.All, 0);
					}
					else
					{
						networkView.RPC("SetTeam", RPCMode.All, 1);
					}
				}
				teamSubtract=true;
				player.transform.position=respawnPoint;
			}
			if(Input.GetKeyDown("1"))
			{
				if(mainGunOut)
				{
					networkView.RPC("EnableGun", RPCMode.All, 0, false);
					mainGunOut=false;
					RecalcAmmoMeter();
				}
				else
				{
					networkView.RPC("EnableGun", RPCMode.All, 0, true);
					mainGunOut=true;
					networkView.RPC("EnableGun", RPCMode.All, 1, false);
					specialGunOut=false;
					networkView.RPC("EnableGun", RPCMode.All, 2, false);
					heavyGunOut=false;
					RecalcAmmoMeter();
				}
			}
			if(Input.GetKeyDown("2"))
			{
				if(specialGunOut)
				{
					networkView.RPC("EnableGun", RPCMode.All, 1, false);
					specialGunOut=false;
					RecalcAmmoMeter();
				}
				else
				{
					networkView.RPC("EnableGun", RPCMode.All, 0, false);
					mainGunOut=false;
					networkView.RPC("EnableGun", RPCMode.All, 1, true);
					specialGunOut=true;
					networkView.RPC("EnableGun", RPCMode.All, 2, false);
					heavyGunOut=false;
					RecalcAmmoMeter();
				}
			}
			if(Input.GetKeyDown("3"))
			{
				if(heavyGunOut)
				{
					networkView.RPC("EnableGun", RPCMode.All, 2, false);
					heavyGunOut=false;
					RecalcAmmoMeter();
				}
				else
				{
					networkView.RPC("EnableGun", RPCMode.All, 0, false);
					mainGunOut=false;
					networkView.RPC("EnableGun", RPCMode.All, 1, false);
					specialGunOut=false;
					networkView.RPC("EnableGun", RPCMode.All, 2, true);
					heavyGunOut=true;
					RecalcAmmoMeter();
				}
			}
			timeSinceLMF++;
			timeSinceLSF++;
			timeSinceLHF++;
			if(Input.GetButtonUp("Reload"))
			{
				if(mainGunOut && primaryAmmoLeft>0)
				{
					int ammoUsed = mainGunClipsize - ammoInMainGun;
					ammoInMainGun = mainGunClipsize;
					primaryAmmoLeft-=ammoUsed;
					if(primaryAmmoLeft<0)
					{
						ammoInMainGun+=primaryAmmoLeft;
						primaryAmmoLeft=0;
					}
					RecalcAmmoMeter();
					networkView.RPC("PlaySound", RPCMode.All, 1, 0);
				}
				if(specialGunOut && specialAmmoLeft>0)
				{
					int ammoUsed = specialGunClipsize - ammoInSpecialGun;
					ammoInSpecialGun = specialGunClipsize;
					specialAmmoLeft-=ammoUsed;
					if(specialAmmoLeft<0)
					{
						ammoInSpecialGun+=specialAmmoLeft;
						specialAmmoLeft=0;
					}
					RecalcAmmoMeter();
					networkView.RPC("PlaySound", RPCMode.All, 1, 1);
				}
				if(heavyGunOut && heavyAmmoLeft>0)
				{
					int ammoUsed = heavyGunClipsize - ammoInHeavyGun;
					ammoInHeavyGun = heavyGunClipsize;
					heavyAmmoLeft-=ammoUsed;
					if(heavyAmmoLeft<0)
					{
						ammoInHeavyGun+=heavyAmmoLeft;
						heavyAmmoLeft=0;
					}
					RecalcAmmoMeter();
					networkView.RPC("PlaySound", RPCMode.All, 1, 2);
				}
			}
			if(Input.GetButtonUp("Unlock"))
				Screen.lockCursor=false;
			if(Input.GetButtonUp("Fire1"))
			{
				Screen.lockCursor=true;
				if(mainGunOut && timeSinceLMF>mainGunFireSpeed && ammoInMainGun>0)
				{
					GameObject bullet = (GameObject)Network.Instantiate(mainShot, bulletPoint.transform.position, transform.rotation, 1);
					bullet.rigidbody.velocity = transform.TransformDirection(Vector3.forward*mainGunBulletspeed);
					ammoInMainGun--;
					RecalcAmmoMeter();
					timeSinceLMF=0;
					networkView.RPC("PlaySound", RPCMode.All, 0, 0);
				}
				if(specialGunOut && timeSinceLSF>specialGunFireSpeed && ammoInSpecialGun>0)
				{
					GameObject bullet = (GameObject)Network.Instantiate(specialShot, bulletPoint.transform.position, transform.rotation, 1);
					bullet.rigidbody.velocity = transform.TransformDirection(Vector3.forward*specialGunBulletspeed);
					ammoInSpecialGun--;
					RecalcAmmoMeter();
					timeSinceLSF=0;
					networkView.RPC("PlaySound", RPCMode.All, 0, 1);
				}
				if(heavyGunOut && timeSinceLHF>heavyGunFireSpeed && ammoInHeavyGun>0)
				{
					GameObject bullet = (GameObject)Network.Instantiate(heavyShot, bulletPoint.transform.position, transform.rotation, 1);
					bullet.rigidbody.velocity = transform.TransformDirection(Vector3.forward*heavyGunBulletspeed);
					ammoInHeavyGun--;
					RecalcAmmoMeter();
					timeSinceLHF=0;
					networkView.RPC("PlaySound", RPCMode.All, 0, 2);
				}
			}
		}
		else
		{
			ammoCount.enabled=false;
		}
	}

	[RPC] void EnableGun(int gun, bool state)
	{
		if(gun==0)
			mainGun.SetActive(state);
		if(gun==1)
			specialGun.SetActive(state);
		if(gun==2)
			heavyGun.SetActive(state);
	}

	[RPC] void PlaySound(int type, int gun)
	{
		if(type==0)
		{
			if(gun==0)
			{
				audio.clip=fireSounds[0];
				audio.Play();
			}
			if(gun==1)
			{
				audio.clip=fireSounds[1];
				audio.Play();
			}
			if(gun==2)
			{
				audio.clip=fireSounds[2];
				audio.Play();
			}
		}
		if(type==1)
		{
			if(gun==0)
			{
				audio.clip=reloadSounds[0];
				audio.Play();
			}
			if(gun==1)
			{
				audio.clip=reloadSounds[1];
				audio.Play();
			}
			if(gun==2)
			{
				audio.clip=reloadSounds[2];
				audio.Play();
			}
		}
	}
}
