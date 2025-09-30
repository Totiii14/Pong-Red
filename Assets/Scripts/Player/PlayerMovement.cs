using UnityEngine;
using Photon.Pun;
using System.Collections;

public class PlayerMovement : MonoBehaviourPunCallbacks
{
    [Header("Movement")]
    [SerializeField] float moveSpeed = 5f;

    void Update()
    {
        if (!photonView.IsMine) return;

        float input = 0f;
        if (Input.GetKey(KeyCode.W)) input = 1f;
        if (Input.GetKey(KeyCode.S)) input = -1f;

        Vector3 move = new Vector3(0, input, 0) * moveSpeed * Time.deltaTime;
        transform.Translate(move);
    }
}