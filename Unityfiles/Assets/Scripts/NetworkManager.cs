using UnityEngine;
using System.Collections;

public class NetworkManager : MonoBehaviour
{
	private const string typeName = "BeansVsBeans";
	private string gameName = "Beans Vs Beans server";
	private string directIP = "127.0.1";
	public int port = 25565;
	public int maxPlayers = 50;
	private HostData[] hostList;
	public GameObject playerPrefab;

	private void RefreshHostList()
	{
		MasterServer.RequestHostList(typeName);
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

	void OnConnectedToServer()
	{
		SpawnPlayer();
		Debug.Log("Clickity click, connected.");
	}

	private void StartServer()
	{
		Network.InitializeServer(maxPlayers, port, !Network.HavePublicAddress());
		MasterServer.RegisterHost(typeName, gameName);
	}

	void OnServerInitialized()
	{
		Debug.Log("Server is UP!  Sweet!");
		SpawnPlayer();
	}

	private void SpawnPlayer()
	{
		Network.Instantiate(playerPrefab, transform.position, Quaternion.identity, 0);
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
