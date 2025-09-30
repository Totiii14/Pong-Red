using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class RoomList : MonoBehaviourPunCallbacks
{
    [SerializeField] GameObject roomPrefab;
    [SerializeField] GameObject[] allRooms;
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        for (int i = 0; i < allRooms.Length; i++)
        {
            if (allRooms[i] != null)
            {
                Destroy(allRooms[i]);
            }
        }

        allRooms = new GameObject[roomList.Count];

        for (int i = 0; i < roomList.Count; i++)
        {
            if (roomList[i].IsOpen && roomList[i].IsVisible && roomList[i].PlayerCount >= 1)
            {
                GameObject Room = Instantiate(roomPrefab, Vector3.zero, Quaternion.identity, GameObject.Find("Content").transform);
                Room.GetComponent<Room>().Name.text = roomList[i].Name;
                allRooms[i] = Room;
            }
        }
    }

    public void RefreshRoomListManually()
    {
        if (PhotonNetwork.InLobby)
        {
            PhotonNetwork.LeaveLobby();
            Invoke(nameof(RejoinLobby), 0.3f);
        }
        else
        {
            PhotonNetwork.JoinLobby();
        }
    }

    private void RejoinLobby()
    {
        PhotonNetwork.JoinLobby();
    }
}
