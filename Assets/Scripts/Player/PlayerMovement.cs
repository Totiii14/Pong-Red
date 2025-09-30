using UnityEngine;
using Photon.Pun;
using System.Collections;

public class PlayerMovement : MonoBehaviourPunCallbacks
{
    [Header("Movement")]
    [SerializeField] float moveSpeed = 5f;

    private void Update()
    {
        if (!photonView.IsMine)
            return;

        if (Input.GetKeyDown(KeyCode.W))
        {
            transform.Translate(0, 1 * moveSpeed, 0);
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            transform.Translate(0, -1 * moveSpeed, 0);

        }
    }
}