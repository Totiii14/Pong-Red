using Photon.Pun;
using UnityEngine;

public class PlayerColor : MonoBehaviourPun
{
    [PunRPC]
    public void SetColorRPC(float r, float g, float b)
    {
        Renderer rend = GetComponent<Renderer>();
        if (rend != null)
        {
            Color playerColor = new Color(r, g, b);
            rend.material.color = playerColor;
        }
    }
}
