using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager Instance;

    [Header("UI")]
    [SerializeField] TMP_Text scoreText;
    [SerializeField] GameObject readyPanel;
    [SerializeField] TMP_Text readyText;
    [SerializeField] TMP_Text playerCountText;
    [SerializeField] Button readyButton;

    [Header("End Game UI")]
    [SerializeField] GameObject endGamePanel;
    [SerializeField] TMP_Text winnerText;
    [SerializeField] Button restartButton;
    [SerializeField] Button menuButton;

    [Header("Disconnection UI")]
    [SerializeField] GameObject disconnectionPanel;
    [SerializeField] TMP_Text disconnectionText;
    [SerializeField] Button leaveButton;

    [Header("Game State")]
    [SerializeField] GameObject ballPrefab;

    int scoreTeam1 = 0;
    int scoreTeam2 = 0;
    private bool gameStarted = false;
    private bool gameEnded = false;
    private bool gamePaused = false;
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
        gameEnded = true;
        DisableAllPlayersMovement();

        if (ball != null)
        {
            BallMovement ballMovement = ball.GetComponent<BallMovement>();
            if (ballMovement != null)
            {
                ballMovement.PauseBall();
            }
        }

        ShowEndGamePanel(winnerTeam);
        PhotonNetwork.CurrentRoom.IsOpen = false;
    }

    void ShowEndGamePanel(int winnerTeam)
    {
        if (endGamePanel != null)
        {
            endGamePanel.SetActive(true);

            if (winnerText != null)
            {
                string teamColor = (winnerTeam == 1) ? "<color=#00FF00>1</color>" : "<color=#0000FF>2</color>";
                winnerText.text = $"¡EQUIPO {teamColor} GANA!";
            }

            if (restartButton != null)
            {
                restartButton.gameObject.SetActive(PhotonNetwork.IsMasterClient);
            }
        }
    }

    public void RestartGame()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        photonView.RPC("RestartGameRPC", RpcTarget.All);
    }

    [PunRPC]
    void RestartGameRPC()
    {
        scoreTeam1 = 0;
        scoreTeam2 = 0;
        gameStarted = false;
        gameEnded = false;
        gamePaused = false;

        scoreText.text = "0 - 0";

        if (endGamePanel != null) endGamePanel.SetActive(false);
        if (disconnectionPanel != null) disconnectionPanel.SetActive(false);

        if (readyPanel != null) readyPanel.SetActive(true);

        playersReady.Clear();
        UpdateReadyUI();

        if (ball != null && PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.Destroy(ball);
            ball = null;
        }

        DisableAllPlayersMovement();
        ResetPlayersPosition();
    }

    public void ReturnToMenu()
    {
        PhotonNetwork.LeaveRoom();
    }

    void ResetPlayersPosition()
    {
        PlayerMovement[] allPlayers = FindObjectsOfType<PlayerMovement>();
        foreach (PlayerMovement player in allPlayers)
        {
            player.transform.position = GetSpawnPosition(player.photonView.OwnerActorNr);
        }
    }

    Vector3 GetSpawnPosition(int actorNumber)
    {
        int team = (actorNumber % 2 == 1) ? 1 : 2;
        float xPos = (team == 1) ? -7f : 7f;
        return new Vector3(xPos, 0, 0);
    }

    void DisableAllPlayersMovement()
    {
        PlayerMovement[] allPlayers = FindObjectsOfType<PlayerMovement>();
        foreach (PlayerMovement player in allPlayers)
        {
            player.DisableMovement();
        }
    }

    void ShowDisconnectionPanel()
    {
        if (disconnectionPanel != null)
        {
            disconnectionPanel.SetActive(true);
            gamePaused = true;

            if (disconnectionText != null)
            {
                disconnectionText.text = "JUGADOR DESCONECTADO";
            }
        }
    }

    void HideDisconnectionPanel()
    {
        if (disconnectionPanel != null)
        {
            disconnectionPanel.SetActive(false);
            gamePaused = false;

            if (GetTeamPlayerCount(1) >= 1 && GetTeamPlayerCount(2) >= 1)
            {
                ResumeGame();
            }
        }
    }

    void ResumeGame()
    {
        if (!gamePaused) return;

        EnableAllPlayersMovement();

        if (ball != null)
        {
            BallMovement ballMovement = ball.GetComponent<BallMovement>();
            if (ballMovement != null)
            {
                ballMovement.ResumeBall();

                if (!ballMovement.IsMoving())
                {
                    ballMovement.StartBall();
                }
            }
        }

        gamePaused = false;
        Debug.Log("Juego reanudado");
    }

    void CheckGameContinuation()
    {
        int team1Count = GetTeamPlayerCount(1);
        int team2Count = GetTeamPlayerCount(2);

        if (team1Count == 0 || team2Count == 0)
        {
            if (disconnectionPanel != null && disconnectionText != null)
            {
                disconnectionText.text = "PARTIDA TERMINADA\nUn equipo se quedó sin jugadores";
            }
        }
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
        if (!gameStarted && !gameEnded)
        {
            UpdatePlayerCount();
            photonView.RPC("SetPlayerReady", RpcTarget.All, newPlayer.ActorNumber, false);

            if (gamePaused && GetTeamPlayerCount(1) >= 1 && GetTeamPlayerCount(2) >= 1)
            {
                HideDisconnectionPanel();
            }
        }
    }

    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        if (!gameStarted && !gameEnded)
        {
            if (playersReady.ContainsKey(otherPlayer.ActorNumber))
            {
                playersReady.Remove(otherPlayer.ActorNumber);
            }
            UpdatePlayerCount();
            UpdateReadyUI();
        }
        else if (gameStarted && !gameEnded)
        {
            PauseGame();
        }
    }

    void PauseGame()
    {
        if (gamePaused || gameEnded) return;

        DisableAllPlayersMovement();

        if (ball != null)
        {
            BallMovement ballMovement = ball.GetComponent<BallMovement>();
            if (ballMovement != null)
            {
                ballMovement.PauseBall();
            }
        }

        ShowDisconnectionPanel();
        CheckGameContinuation();
    }

    public override void OnLeftRoom()
    {
        SceneManager.LoadScene("Pre-Lobby");
    }
}