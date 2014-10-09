using UnityEngine;
using System.Collections;

public class NetworkManager : MonoBehaviour
{
	public const string typeName = "BeansVsBeans";
	public string gameName = "Beans Vs Beans server";
	public string directIP = "127.0.1";
	public int port = 25565;
	public int maxPlayers = 50;
	private HostData[] hostList;
	public GameObject playerPrefab;
	public int redTeamScore = 0;
	public int blueTeamScore = 0;
	public int numberOfPointsToWin = 500;
	public float redPlayers = 0;
	public float bluePlayers = 0;
	public GameObject redSpawnpoint;
	public GameObject blueSpawnpoint;
	public Vector3 redSpawnV;
	public Vector3 blueSpawnV;
	public bool lostPlayerThisTick = false;
	public GUIText redScoreText;
	public GUIText blueScoreText;
	public int timeUntilScoreReset=-1337;
	public bool doOnceWin = false;

	private void RefreshHostList()
	{
		MasterServer.RequestHostList(typeName);
	}

	void Update()
	{
		if(!doOnceWin)
		{
			redScoreText.text="Team A: "+redTeamScore+"/"+numberOfPointsToWin;
			blueScoreText.text="Team B: "+blueTeamScore+"/"+numberOfPointsToWin;
		}
		if(timeUntilScoreReset!=-1337)
			timeUntilScoreReset--;
		if(timeUntilScoreReset==0)
		{
			timeUntilScoreReset=-1337;
			redTeamScore=0;
			blueTeamScore=0;
			doOnceWin = false;
		}
		if(redTeamScore/numberOfPointsToWin>0.75 && !doOnceWin)
		{
			redScoreText.text+=", TEAM A IS CLOSE TO WINNING";
		}
		if(blueTeamScore/numberOfPointsToWin>0.75 && !doOnceWin)
		{
			blueScoreText.text+=", TEAM B IS CLOSE TO WINNING";
		}
		if(redTeamScore>=numberOfPointsToWin && !doOnceWin)
		{
			timeUntilScoreReset=30*60;
			redScoreText.text="TEAM A WINS!";
			doOnceWin=true;
		}
		if(blueTeamScore>=numberOfPointsToWin && !doOnceWin)
		{
			timeUntilScoreReset=30*60;
			blueScoreText.text="TEAM B WINS!";
			doOnceWin=true;
		}
	}

	void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info)
	{
		float rPs = 0;
		int rS = 0;
		Vector3 rSP = Vector3.down;
		float bPs = 0;
		int bS = 0;
		Vector3 bSP = Vector3.down;
		int sTW = 0;
		if(stream.isWriting)
		{
			rPs=redPlayers;
			stream.Serialize(ref rPs);
			rS=redTeamScore;
			stream.Serialize(ref rS);
			rSP=redSpawnV;
			stream.Serialize(ref rSP);
			bPs=bluePlayers;
			stream.Serialize(ref bPs);
			bS=blueTeamScore;
			stream.Serialize(ref bS);
			bSP=blueSpawnV;
			stream.Serialize(ref bSP);
			sTW=numberOfPointsToWin;
			stream.Serialize(ref sTW);
		}
		else
		{
			stream.Serialize(ref rPs);
			redPlayers=rPs;
			stream.Serialize(ref rS);
			redTeamScore=rS;
			stream.Serialize(ref rSP);
			redSpawnV=rSP;
			stream.Serialize(ref bPs);
			bluePlayers=bPs;
			stream.Serialize(ref bS);
			blueTeamScore=bS;
			stream.Serialize(ref bSP);
			blueSpawnV=bSP;
			stream.Serialize(ref sTW);
			numberOfPointsToWin=sTW;
		}
	}

	void OnMasterServerEvent(MasterServerEvent msEvent)
	{
		if(msEvent == MasterServerEvent.HostListReceived)
		{
			hostList = MasterServer.PollHostList();
		}
	}
	
	void OnPlayerDisconnected(NetworkPlayer player)
    {
        Debug.Log("Clean up after player " + player);
        Network.RemoveRPCs(player);
        Network.DestroyPlayerObjects(player);
    }

	private void JoinServer(HostData hostData)
	{
		Network.Connect(hostData);
	}

	[RPC] void AddPlayers(int team, int amount)
	{
		if(team==0)
		{
			redPlayers+=amount;
		}
		else if(team==1)
		{
			bluePlayers+=amount;
		}
	}

	[RPC] void AddScore(int team, int amount)
	{
		if(team==0)
		{
			redTeamScore+=amount;
		}
		else if(team==1)
		{
			blueTeamScore+=amount;
		}
	}

	void OnConnectedToServer()
	{
		SpawnPlayer();
		Debug.Log("Clickity click, connected.");
	}

	private void StartServer()
	{
		Network.InitializeServer(maxPlayers, port, !Network.HavePublicAddress());
		MasterServer.RegisterHost(typeName, gameName);
		networkView.RPC("UpdateSpawnpoints", RPCMode.All);
	}

	void OnServerInitialized()
	{
		Debug.Log("Server is UP!  Sweet!");
		SpawnPlayer();
	}

	[RPC] void UpdateSpawnpoints()
	{
		redSpawnV=redSpawnpoint.transform.position;
		blueSpawnV=blueSpawnpoint.transform.position;
	}

	private void SpawnPlayer()
	{
		GameObject player = Network.Instantiate(playerPrefab, transform.position, Quaternion.identity, 0) as GameObject;
		TakeDamage damage = player.GetComponent("TakeDamage") as TakeDamage;
		Shoot shootScript = damage.shootScript;
		player.audio.clip=damage.pickupSound;
		Music musicScript = Camera.main.GetComponent<Music>();
		Camera.main.audio.clip=musicScript.normal[0];
		shootScript.gameObject.audio.clip=shootScript.reloadSounds[1];
		shootScript.SetNetworkManager(this);
	}

	void OnGUI()
	{
		if(!Network.isClient && !Network.isServer)
		{
			if(GUI.Button(new Rect(100, 100, 250, 100), "Start Server"))
			{
				StartServer();
			}

			if(GUI.Button(new Rect(100, 250, 250, 100), "Refresh hosts"))
			{
				RefreshHostList();
			}

			if(GUI.Button(new Rect(750, 250, 250, 100), "Connect to IP specified"))
			{
				Network.Connect(directIP, port);
			}

			gameName = GUI.TextField(new Rect(100, 400, 200, 20), gameName, 30);
			port = int.Parse(GUI.TextField(new Rect(100, 450, 200, 20), port.ToString(), 30));
			maxPlayers = int.Parse(GUI.TextField(new Rect(100, 500, 200, 20), maxPlayers.ToString(), 30));
			directIP = GUI.TextField(new Rect(100, 550, 200, 20), directIP, 30);
			numberOfPointsToWin = int.Parse(GUI.TextField(new Rect(100, 600, 200, 20), numberOfPointsToWin.ToString(), 30));

			if(hostList != null)
			{
				for(int i=0; i<hostList.Length; i++)
				{
					if(GUI.Button (new Rect(400, 100+(110*i), 300, 100), hostList[i].gameName))
					{
						JoinServer(hostList[i]);
					}
				}
			}
		}
	}
}
