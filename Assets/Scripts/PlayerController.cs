using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Move")]
    [Tooltip("Target top speed when holding left/right.")]
    public float moveSpeed = 8f;
    [Tooltip("How fast horizontal speed ramps up toward the target.")]
    public float acceleration = 55f;
    [Tooltip("How fast you slow down when input is released.")]
    public float deceleration = 40f;
    [Tooltip("Multiplies accel/decel in the air (usually < 1 for less snappy air control).")]
    [Range(0.05f, 1f)]
    public float airControl = 0.65f;

    [Header("Jump")]
    public float jumpForce = 12f;

    [Header("Ground check")]
    [Tooltip("Contact normal vs world up; above this counts as standing on something.")]
    public float groundNormalDot = 0.55f;

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private bool isGrounded;
    private ContactPoint2D[] contactBuffer = new ContactPoint2D[16];
    private float horizontalInput;
    private bool jumpQueued;
    private float currentVx;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        currentVx = rb.velocity.x;
    }

    void Update()
    {
        horizontalInput = Input.GetAxis("Horizontal");
        if (Input.GetKeyDown(KeyCode.Space))
            jumpQueued = true;

        if (sr != null)
        {
            if (rb.velocity.x > 0.1f) sr.flipX = false;
            else if (rb.velocity.x < -0.1f) sr.flipX = true;
        }
    }

    void FixedUpdate()
    {
        isGrounded = false;
        int count = rb.GetContacts(contactBuffer);
        for (int i = 0; i < count; i++)
        {
            if (Vector2.Dot(contactBuffer[i].normal, Vector2.up) >= groundNormalDot)
            {
                isGrounded = true;
                break;
            }
        }

        float targetVx = horizontalInput * moveSpeed;
        float dt = Time.fixedDeltaTime;
        float accel = acceleration;
        float decel = deceleration;
        if (!isGrounded)
        {
            accel *= airControl;
            decel *= airControl;
        }

        if (Mathf.Abs(horizontalInput) > 0.01f)
            currentVx = Mathf.MoveTowards(currentVx, targetVx, accel * dt);
        else
            currentVx = Mathf.MoveTowards(currentVx, 0f, decel * dt);

        rb.velocity = new Vector2(currentVx, rb.velocity.y);

        if (jumpQueued)
        {
            jumpQueued = false;
            if (isGrounded)
                rb.velocity = new Vector2(currentVx, jumpForce);
        }
    }
}
