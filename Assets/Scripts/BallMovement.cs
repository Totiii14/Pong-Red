using UnityEngine;
using Photon.Pun;

public class BallMovement : MonoBehaviourPunCallbacks
{
    [Header("Ball Settings")]
    [SerializeField] float speed = 5f;
    [SerializeField] float maxStartAngle = 45f;

    private Vector2 direction;
    private bool isMoving = false;
    private bool isPaused = false;
    private Rigidbody2D rb;
    private Vector2 lastVelocity;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (PhotonNetwork.IsMasterClient)
        {
            InitializeBall();
        }
    }

    void Update()
    {
        if (!photonView.IsMine) return;
    }


    void FixedUpdate()
    {
        if (!photonView.IsMine) return;
        if (isPaused) return;

        if (isMoving)
        {
            rb.velocity = direction * speed;
        }
    }

    public void PauseBall()
    {
        isPaused = true;
        rb.velocity = Vector2.zero;
    }

    public void ResumeBall()
    {
        isPaused = false;
    }

    public void StartBall()
    {
        if (photonView.IsMine)
        {
            StartBallMovement();
        }
    }

    void StartBallMovement()
    {
        float randomAngle = Random.Range(-maxStartAngle, maxStartAngle);
        int side = Random.Range(0, 2) * 2 - 1;

        direction = Quaternion.Euler(0, 0, randomAngle) * Vector2.right * side;
        direction.Normalize();
        isMoving = true;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!photonView.IsMine) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            ContactPoint2D contact = collision.contacts[0];
            Vector2 normal = contact.normal;

            direction = Vector2.Reflect(direction, normal).normalized;

            if (Mathf.Abs(direction.x) < 0.3f)
            {
                direction.x = Mathf.Sign(direction.x) * 0.3f;
                direction = direction.normalized;
            }
        }
        else if (collision.gameObject.CompareTag("Wall"))
        {
            direction = new Vector2(direction.x, -direction.y);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!photonView.IsMine) return;

        if (other.CompareTag("Goal"))
        {
            ScoreGoal(other.gameObject.name);
            ResetBall();
        }
    }

    void ScoreGoal(string goalName)
    {
        int scoringTeam = goalName.Contains("Left") ? 2 : 1;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.photonView.RPC("AddScore", RpcTarget.All, scoringTeam);
        }
    }

    void ResetBall()
    {
        if (!photonView.IsMine) return;

        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;
        transform.position = Vector3.zero;
        isMoving = false;

        Invoke("StartBallMovement", 1f);
    }

    void InitializeBall()
    {
        transform.position = Vector3.zero;
        isMoving = false;
        rb.velocity = Vector2.zero;
    }

    public bool IsMoving()
    {
        return isMoving;
    }
}