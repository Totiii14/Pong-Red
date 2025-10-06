using UnityEngine;
using Photon.Pun;

public class PlayerMovement : MonoBehaviourPunCallbacks
{
    [Header("Movement")]
    [SerializeField] float moveSpeed = 5f;

    private Rigidbody2D rb;
    private bool canMove = false;
    private float minY, maxY;
    private Camera mainCamera;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        mainCamera = Camera.main;

        CalculateCameraBounds();
    }

    void CalculateCameraBounds()
    {
        if (mainCamera != null)
        {
            float playerHeight = GetComponent<Collider2D>().bounds.extents.y;
            float cameraHeight = mainCamera.orthographicSize;
            float cameraY = mainCamera.transform.position.y;

            maxY = cameraY + cameraHeight - playerHeight;
            minY = cameraY - cameraHeight + playerHeight;
        }
    }

    void Update()
    {
        if (!photonView.IsMine) return;
        if (!canMove) return;

        float input = 0f;
        if (Input.GetKey(KeyCode.W)) input = 1f;
        if (Input.GetKey(KeyCode.S)) input = -1f;

        Vector2 move = new Vector2(0, input) * moveSpeed * Time.deltaTime;
        transform.Translate(move);

        KeepInBounds();
    }

    void KeepInBounds()
    {
        Vector3 pos = transform.position;
        pos.y = Mathf.Clamp(pos.y, minY, maxY);
        transform.position = pos;
    }

    public void EnableMovement()
    {
        canMove = true;
    }

    public void DisableMovement()
    {
        canMove = false;
    }

    void OnEnable()
    {
        CalculateCameraBounds();
    }
}