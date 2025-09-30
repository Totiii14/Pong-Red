using Photon.Pun;
using UnityEngine;

public class PlayerColor : MonoBehaviourPun
{
    [PunRPC]
    public void SetColorRPC(int team)
    {
        Renderer rend = GetComponent<Renderer>(); 
        if (rend != null)
            rend.material.color = (team == 1) ? Color.white : Color.black;
    }
}
