using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;

public class GameSpawner : MonoBehaviourPunCallbacks
{
    public static GameSpawner Instance;

    [SerializeField] GameObject[] playerSpawns;

    private List<GameObject> spawnedPlayers = new List<GameObject>();

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        int team = (PhotonNetwork.CurrentRoom.PlayerCount % 2 == 1) ? 1 : 2;

        ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable();
        props["team"] = team;

        PhotonNetwork.LocalPlayer.SetCustomProperties(props);

        SpawnPlayer(team);
    }

    void SpawnPlayer(int team)
    {
        int spawnIndex = PhotonNetwork.LocalPlayer.ActorNumber - 1;
        if (spawnIndex >= 0 && spawnIndex < playerSpawns.Length)
        {
            GameObject playerSpawn = playerSpawns[spawnIndex];

            GameObject player = PhotonNetwork.Instantiate("Player", playerSpawn.transform.position, Quaternion.identity);
            spawnedPlayers.Add(player);

            PhotonView pv = player.GetComponent<PhotonView>();
            pv.RPC("SetColorRPC", RpcTarget.AllBuffered, team);
        }
    }

    public override void OnLeftRoom()
    {
        ClearSpawnedPlayers();
    }

    public void ClearSpawnedPlayers()
    {
        foreach (var player in spawnedPlayers)
        {
            if (player != null && player.GetComponent<PhotonView>() != null)
                PhotonNetwork.Destroy(player);
        }

        spawnedPlayers.Clear();
    }

}
