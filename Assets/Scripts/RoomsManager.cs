using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class RoomsManager : MonoBehaviourPunCallbacks
{
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        foreach (var obj in FindObjectsOfType<PhotonView>())
        {
            if (obj.Owner == otherPlayer)
            {
                PhotonNetwork.Destroy(obj.gameObject);
            }
        }
    }
}
