using Photon.Pun;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager Instance;

    [SerializeField] TMP_Text scoreText;
    int scoreTeam1 = 0;
    int scoreTeam2 = 0;

    private void Awake() => Instance = this;

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
}
