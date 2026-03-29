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

    [SerializeField] private bool isWallSliding;
    private float wallSlidingSpeed = 1f;

    [SerializeField] private bool isWallJumping;
    private float wallJumpDirection;
    private float wallJumpTime = 0.2f;
    private float wallJumpCount;
    [SerializeField] private float wallJumpDuration = 0.4f;
    private Vector2 wallJumpPower = new Vector2(8f, 16f);
    private bool isFacingRight = true;

    [Header("Ground check")]
    [Tooltip("Contact normal vs world up; above this counts as standing on something.")]
    public float groundNormalDot = 0.55f;

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private ContactPoint2D[] contactBuffer = new ContactPoint2D[16];
    private float horizontalInput;
    private bool jumpQueued;
    private float currentVx;

    [SerializeField] private bool isGrounded;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private bool isWalled;
    [SerializeField] private Transform wallCheck;
    [SerializeField] private LayerMask wallLayer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        currentVx = rb.velocity.x;
    }

    void Update()
    {
        isGrounded = false;
        horizontalInput = Input.GetAxis("Horizontal");
        // if (Input.GetKeyDown(KeyCode.Space))
        //     jumpQueued = true;

        // if (sr != null)
        // {
        //     if (rb.velocity.x > 0.1f) sr.flipX = false;
        //     else if (rb.velocity.x < -0.1f) sr.flipX = true;
        // }

        if (Input.GetKeyDown(KeyCode.Space) && IsGrounded())
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        }


        WallSlide();
        WallJump();

        if(!isWallJumping)
        {
            Flip();
        }
    }

    void FixedUpdate()
    {
        // int count = rb.GetContacts(contactBuffer);
        // for (int i = 0; i < count; i++)
        // {
        //     if (Vector2.Dot(contactBuffer[i].normal, Vector2.up) >= groundNormalDot)
        //     {
        //         isGrounded = true;
        //         break;
        //     }
        // }

        float targetVx = horizontalInput * moveSpeed;
        float dt = Time.fixedDeltaTime;
        float accel = acceleration;
        float decel = deceleration;
        
        // if(!isWallJumping)
        // {
        //     if (!IsGrounded())
        //     {
        //         accel *= airControl;
        //         decel *= airControl;
        //     }

        
        //     if (Mathf.Abs(horizontalInput) > 0.01f)
        //         currentVx = Mathf.MoveTowards(currentVx, targetVx, accel * dt);
        //     else
        //         currentVx = Mathf.MoveTowards(currentVx, 0f, decel * dt);

        //     rb.velocity = new Vector2(currentVx, rb.velocity.y);
        

        //     if (jumpQueued)
        //     {
        //         jumpQueued = false;
        //         if (IsGrounded())
        //             rb.velocity = new Vector2(currentVx, jumpForce);
        //     }
        // }
        if (!isWallJumping)
        {
            rb.velocity = new Vector2(horizontalInput * moveSpeed, rb.velocity.y);
        }
    }

    private bool IsGrounded()
    {
        if(Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer))
        {
            isGrounded = true;
        }
        else
        {
            isGrounded = false;
        }
        return isGrounded;
    }

    private bool IsWalled()
    {
        if(Physics2D.OverlapCircle(wallCheck.position, 0.2f, wallLayer))
        {
            isWalled = true;
        }
        else
        {
            isWalled = false;
        }
        return isWalled;
    }

    private void WallSlide()
    {
        if (IsWalled() && !IsGrounded() && horizontalInput != 0f)
        {
            isWallSliding = true;
            rb.velocity = new Vector2(rb.velocity.x, Mathf.Clamp(rb.velocity.y, -wallSlidingSpeed, float.MaxValue));
        }
        else
        {
            isWallSliding = false;
        }
    }

    private void WallJump()
    {
        if (isWallSliding)
        {
            isWallJumping = false;
            wallJumpDirection = -transform.localScale.x;
            wallJumpCount = wallJumpTime;

            CancelInvoke(nameof(StopWallJumping));
        }
        else
        {
            wallJumpCount -= Time.deltaTime;
        }

        if (Input.GetButtonDown("Jump") && wallJumpCount > 0f)
        {
            isWallJumping = true;
            rb.velocity = new Vector2(wallJumpDirection * wallJumpPower.x, wallJumpPower.y);
            wallJumpCount = 0f;

            if (transform.localScale.x != wallJumpDirection)
            {
                isFacingRight = !isFacingRight;
                Vector3 localScale = transform.localScale;
                localScale.x *= -1f;
                transform.localScale = localScale;
            }

            Invoke(nameof(StopWallJumping), wallJumpDuration);
        }
    }

    private void StopWallJumping()
    {
        isWallJumping = false;
    }

    private void Flip()
    {
        if (isFacingRight && horizontalInput < 0f || !isFacingRight && horizontalInput > 0f)
        {
            isFacingRight = !isFacingRight;
            Vector3 localScale = transform.localScale;
            localScale.x *= -1f;
            transform.localScale = localScale;
        }
    }
}
