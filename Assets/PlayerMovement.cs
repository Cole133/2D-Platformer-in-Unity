using UnityEngine;
using UnityEngine.InputSystem;


public class PlayerMovement : MonoBehaviour
{
    public Rigidbody2D rb;
    public Animator animator;
    bool isFacingRight = true;

    [Header("Movement")]
    public float moveSpeed = 5f;
    public float acceleration = 20f;
    public float deceleration = 35f;
    public float airControl = 1.1f; // How much control the player has in the air
    float horizontalMovement;

    [Header("Jumping")]
    public float jumpForce = 8f;
    public int maxJumpCount = 2;
    private int currentJumpCount = 0;

    [Header("Ground Check")]
    public Transform groundCheckPos;
    public Vector2 groundCheckSize = new Vector2(0.5f, 0.05f);
    public LayerMask groundLayer;
    bool onGround;

    [Header("Gravity")]
    public float gravityScale = 1.5f;
    public float maxFallSpeed = 4f;
    public float fallMultiplier = 2f;

    [Header("Wall Check")]
    public Transform wallCheckPos;
    public Vector2 wallCheckSize = new Vector2(0.5f, 0.05f);
    public LayerMask wallLayer;

    [Header("WallMovement")]
    public float wallSlideSpeed = 2f;
    bool isWallSliding;
    bool isWallJumping;
    float wallJumpDirection;
    float wallJumpTime = 0.5f;
    float wallJumpTimer;
    public Vector2 wallJumpForce = new Vector2(5f, 8f);

    [Header("Rolling")]
    public float rollSpeed = 10f;
    public float rollAccelerationMult = 2f;
    public float rollJumpMult = 0.5f;
    private bool isRolling = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        
        isGrounded();
        Gravity();
        ProccessWallSliding();
        ProccessWallJump();

        if (!isWallJumping)
        {
            MomentumMovment();
            Flip();
        }

        float horizontalSpeed = Mathf.Abs(rb.linearVelocity.x);
        animator.SetFloat("magnitude", horizontalSpeed);
        animator.SetFloat("yVelocity", rb.linearVelocity.y);
        animator.SetBool("isRolling", isRolling);
        
    }

    public void Gravity()
    {
        if (rb.linearVelocity.y < 0)
        {
            rb.gravityScale = gravityScale * fallMultiplier;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, Mathf.Max(rb.linearVelocity.y, -maxFallSpeed));
        }
        else
        {
            rb.gravityScale = gravityScale;
        }
    }

    public void Move(InputAction.CallbackContext context)
    {
        horizontalMovement = context.ReadValue<Vector2>().x;
    }

    public void Roll(InputAction.CallbackContext context)
    {
        if(context.performed && onGround)
        {
            isRolling = true;
            Debug.Log("Roll PERFORMED isRolling = true");
        }
        else if (context.canceled)
        {
            isRolling = false;
            Debug.Log("Roll PERFORMED isRolling = true");
        }
    }

    private void MomentumMovment()
    {
        float targetSpeed = horizontalMovement * (isRolling ? rollSpeed : moveSpeed);
        float speedDifference = targetSpeed - rb.linearVelocity.x;
        float accelerationRate = (Mathf.Abs(targetSpeed) > 0.01f) ? acceleration : deceleration;

        accelerationRate *= (onGround ? 1f : airControl);

        if (isRolling)
        {
            accelerationRate *= rollAccelerationMult;
        }

        float movement = Mathf.Sign(speedDifference) * accelerationRate * Time.deltaTime;

        if(Mathf.Abs(speedDifference) < Mathf.Abs(movement))
        {
            movement = speedDifference;
        }

        rb.linearVelocity = new Vector2(rb.linearVelocity.x + movement, rb.linearVelocity.y);
    }

    public void Jump(InputAction.CallbackContext context)
    {
        if(currentJumpCount > 0)
        {
            if (context.performed)
            {
                float rollModifier = isRolling ? jumpForce * rollJumpMult : jumpForce;
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, rollModifier);
                currentJumpCount--;
                animator.SetTrigger("jump");
            }
            else if (context.canceled)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * 0.4f);
                currentJumpCount--;
                animator.SetTrigger("jump");
            }
        }
        
        if(context.performed && wallJumpTimer > 0f)
        {
            isWallJumping = true;
            rb.linearVelocity = new Vector2(wallJumpDirection * wallJumpForce.x, wallJumpForce.y);
            wallJumpTimer = 0f;
            animator.SetTrigger("jump");

            if (transform.localScale.x != wallJumpDirection)
            {
                isFacingRight = !isFacingRight;
                Vector3 scale = transform.localScale;
                scale.x *= -1f;
                transform.localScale = scale;
            }

            Invoke(nameof(CanelWallJump), wallJumpTime + 0.1f);
        }
    }

    private void isGrounded()
    {
        if(Physics2D.OverlapBox(groundCheckPos.position, groundCheckSize, 0f, groundLayer))
        {
            currentJumpCount = maxJumpCount;
            onGround = true;
        }
        else
        {
            onGround = false;
        }
        
    }
    private bool WallCheck()
    {
        if (Physics2D.OverlapBox(wallCheckPos.position, wallCheckSize, 0f, wallLayer))
        {
            return true;
        }
        return false;
    }

    private void ProccessWallSliding()
    {
        if (!onGround & WallCheck() & horizontalMovement != 0)
        {
            isWallSliding = true;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, Mathf.Max(rb.linearVelocity.y, -wallSlideSpeed));
        }
        else
        {
            isWallSliding = false;
        }
    }

    private void ProccessWallJump()
    {
        if (isWallSliding)
        {
            isWallJumping = false;
            wallJumpDirection = -transform.localScale.x;
            wallJumpTimer = wallJumpTime;

            CancelInvoke(nameof(CanelWallJump));
        }
        else if (wallJumpTimer > 0f)
        {
            wallJumpTimer -= Time.deltaTime;
        }
    }

    private void CanelWallJump()
    {
        isWallJumping = false;
    }

    private void Flip()
    {
        if (isFacingRight && horizontalMovement < 0 || !isFacingRight && horizontalMovement > 0)
        {
            isFacingRight = !isFacingRight;
            Vector3 scale = transform.localScale;
            scale.x *= -1f;
            transform.localScale = scale;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawCube(groundCheckPos.position, groundCheckSize);

        Gizmos.color = Color.blue;
        Gizmos.DrawCube(wallCheckPos.position, wallCheckSize);
    }
}
