# 2D-Platformer-in-Unity

A custom 2D platformer built in Unity featuring advanced movement mechanics and smooth animations.

## Features
- Custom movement system with precise controls
- Wall jumping mechanics
- Double jump mechanics
- Roll abilities  
- Collision detection system (Wall and Ground)
- Sprite animations


## Technical Details
- **Engine:** Unity 3.12.1
- **Language:** C#
- **Development Time:** 1 Week

## Controls
- A, D, Left Arrow, Right Arrow - Move left and right
- Space, Up Arrow - Jump
- Space while wall colliding - Wall Jump
- R - Roll

## Screenshots
![Gameplay Demo](2D Platformer Gameplay.mp4)
## How to Play

## Code Highlights
Momentum Based Movement
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

Wall & Jump

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
