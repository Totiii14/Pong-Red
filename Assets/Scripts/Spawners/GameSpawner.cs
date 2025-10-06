using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;

public class GameSpawner : MonoBehaviourPunCallbacks
{
    public static GameSpawner Instance;

    [SerializeField] GameObject[] playerSpawns;

    private List<GameObject> spawnedPlayers = new List<GameObject>();
    private List<Color> usedColors = new List<Color>();

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

            AssignPlayerColor(player);
        }
    }
    void AssignPlayerColor(GameObject player)
    {
        PhotonView pv = player.GetComponent<PhotonView>();
        Color playerColor = GetPlayerColor();

        if (IsColorAvailable(playerColor))
        {
            usedColors.Add(playerColor);
            pv.RPC("SetColorRPC", RpcTarget.AllBuffered, playerColor.r, playerColor.g, playerColor.b);
        }
    }

    Color GetPlayerColor()
    {
        if (PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("playerColor"))
        {
            Vector3 colorVector = (Vector3)PhotonNetwork.LocalPlayer.CustomProperties["playerColor"];
            return new Color(colorVector.x, colorVector.y, colorVector.z);
        }

        return Color.white;
    }

    bool IsColorAvailable(Color color)
    {
        foreach (Color usedColor in usedColors)
        {
            if (ColorSimilar(usedColor, color, 0.1f))
                return false;
        }
        return true;
    }

    bool ColorSimilar(Color a, Color b, float tolerance)
    {
        return Mathf.Abs(a.r - b.r) < tolerance &&
               Mathf.Abs(a.g - b.g) < tolerance &&
               Mathf.Abs(a.b - b.b) < tolerance;
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
