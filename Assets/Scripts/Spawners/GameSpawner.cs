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
        SpawnPlayer();
    }

    void SpawnPlayer()
    {
        int actorNumber = PhotonNetwork.LocalPlayer.ActorNumber;
        string prefabToSpawn = "Player";

        switch (actorNumber)
        {
            case 1: prefabToSpawn = "Player"; break;
            case 2: prefabToSpawn = "Player 2"; break;
            case 3: prefabToSpawn = "Player 3"; break;
            case 4: prefabToSpawn = "Player 4"; break;
        }

        int spawnIndex = PhotonNetwork.LocalPlayer.ActorNumber - 1;
        if (spawnIndex >= 0 && spawnIndex < playerSpawns.Length)
        {
            GameObject playerSpawn = playerSpawns[spawnIndex];
            GameObject player = PhotonNetwork.Instantiate(prefabToSpawn, playerSpawn.transform.position, Quaternion.identity);
            spawnedPlayers.Add(player);
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
