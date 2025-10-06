using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager Instance;

    [Header("UI")]
    [SerializeField] TMP_Text scoreText;
    [SerializeField] GameObject readyPanel;
    [SerializeField] TMP_Text readyText;
    [SerializeField] TMP_Text playerCountText;
    [SerializeField] Button readyButton;

    [Header("Game State")]
    [SerializeField] GameObject ballPrefab;

    int scoreTeam1 = 0;
    int scoreTeam2 = 0;
    private bool gameStarted = false;
    private Dictionary<int, bool> playersReady = new Dictionary<int, bool>();
    private GameObject ball;

    private void Awake() => Instance = this;

    void Start()
    {
        UpdatePlayerCount();
        UpdateReadyUI();
    }

    void Update()
    {
        if (!gameStarted)
        {
            UpdatePlayerCount();
        }
    }

    [PunRPC]
    public void AddScore(int team)
    {
        if (team == 1) scoreTeam1++;
        else scoreTeam2++;

        scoreText.text = $"{scoreTeam1} - {scoreTeam2}";

        if (scoreTeam1 >= 5 || scoreTeam2 >= 5)
        {
            EndGame(team);
        }

    }

    void EndGame(int winnerTeam)
    {
        Debug.Log("Gano el equipo " + winnerTeam);
        PhotonNetwork.CurrentRoom.IsOpen = false;
    }

    public void ToggleReady()
    {
        photonView.RPC("SetPlayerReady", RpcTarget.All, PhotonNetwork.LocalPlayer.ActorNumber, !IsPlayerReady(PhotonNetwork.LocalPlayer.ActorNumber));
    }

    [PunRPC]
    void SetPlayerReady(int actorNumber, bool ready)
    {
        if (playersReady.ContainsKey(actorNumber))
            playersReady[actorNumber] = ready;
        else
            playersReady.Add(actorNumber, ready);

        UpdateReadyUI();
        CheckStartGame();
    }

    bool IsPlayerReady(int actorNumber)
    {
        return playersReady.ContainsKey(actorNumber) && playersReady[actorNumber];
    }

    void UpdateReadyUI()
    {
        if (readyText != null)
        {
            int readyCount = playersReady.Count(p => p.Value);
            int totalPlayers = PhotonNetwork.CurrentRoom.PlayerCount;
            readyText.text = $"Jugadores listos: {readyCount}/{totalPlayers}";

            if (readyButton != null)
            {
                bool isReady = IsPlayerReady(PhotonNetwork.LocalPlayer.ActorNumber);
                readyButton.GetComponentInChildren<TMP_Text>().text = isReady ? "No Listo" : "Listo";
            }
        }
    }

    void UpdatePlayerCount()
    {
        if (playerCountText != null)
        {
            int team1Count = GetTeamPlayerCount(1);
            int team2Count = GetTeamPlayerCount(2);
            playerCountText.text = $"Equipo 1: {team1Count} | Equipo 2: {team2Count}";

            if (team1Count == 0 || team2Count == 0)
            {
                playerCountText.color = Color.red;
            }
            else
            {
                playerCountText.color = Color.white;
            }
        }
    }

    int GetTeamPlayerCount(int team)
    {
        int count = 0;
        foreach (var player in PhotonNetwork.PlayerList)
        {
            if (player.CustomProperties.ContainsKey("team") && (int)player.CustomProperties["team"] == team)
            {
                count++;
            }
        }
        return count;
    }

    void CheckStartGame()
    {
        if (gameStarted) return;
        if (!PhotonNetwork.IsMasterClient) return;

        int team1Count = GetTeamPlayerCount(1);
        int team2Count = GetTeamPlayerCount(2);
        int readyCount = playersReady.Count(p => p.Value);

        if (team1Count >= 1 && team2Count >= 1 && readyCount == PhotonNetwork.CurrentRoom.PlayerCount)
        {
            photonView.RPC("StartGame", RpcTarget.All);
        }
    }

    [PunRPC]
    void StartGame()
    {
        gameStarted = true;
        readyPanel.SetActive(false);

        PhotonNetwork.CurrentRoom.IsOpen = false;

        EnableAllPlayersMovement();

        if (PhotonNetwork.IsMasterClient)
        {
            ball = PhotonNetwork.Instantiate(ballPrefab.name, Vector3.zero, Quaternion.identity);
            Invoke("StartBallMovement", 0.5f);
        }
    }

    void StartBallMovement()
    {
        if (ball != null)
        {
            BallMovement ballMovement = ball.GetComponent<BallMovement>();
            if (ballMovement != null)
            {
                ballMovement.StartBall();
            }
        }
    }

    void EnableAllPlayersMovement()
    {
        PlayerMovement[] allPlayers = FindObjectsOfType<PlayerMovement>();
        foreach (PlayerMovement player in allPlayers)
        {
            player.EnableMovement();
        }
    }

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        if (!gameStarted)
        {
            UpdatePlayerCount();
            photonView.RPC("SetPlayerReady", RpcTarget.All, newPlayer.ActorNumber, false);
        }
    }

    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        if (!gameStarted)
        {
            if (playersReady.ContainsKey(otherPlayer.ActorNumber))
            {
                playersReady.Remove(otherPlayer.ActorNumber);
            }
            UpdatePlayerCount();
            UpdateReadyUI();
        }
        else
        {
            PauseGame();
        }
    }

    void PauseGame()
    {
        PlayerMovement[] allPlayers = FindObjectsOfType<PlayerMovement>();
        foreach (PlayerMovement player in allPlayers)
        {
            player.DisableMovement();
        }

        if (ball != null)
        {
            BallMovement ballMovement = ball.GetComponent<BallMovement>();
            if (ballMovement != null)
            {
                ballMovement.PauseBall(); 
            }
        }

        // Mostrar UI de pausa por desconexión
        // (puedes implementar esto después)
    }
}
