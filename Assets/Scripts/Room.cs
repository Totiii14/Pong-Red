using UnityEngine;
using TMPro;

public class Room : MonoBehaviour
{
    public TMP_Text Name;

    public void JoinRoom()
    {
        GameObject.Find("CreateAndJoin").GetComponent<CreateAndJoin>().JoinRoomInList(Name.text);
    }
}
