using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlatformPlayerScript : MonoBehaviour
{
    public Rigidbody2D prb;
    public Transform groundCheck1;
    public Transform groundCheck2;
    public Transform groundCheck3;
    public Transform bufferCheck;
    public Transform wallCheck1;
    public Transform wallCheck2;
    public LayerMask groundLayer;
    public float latSpeed;
    public float jumpStr;
    public float jumpSlowdown = 0.5f;
    public float coyotteTime = 0.2f;
    public float jumpBufferTime = 0.2f;
    public float wallSlidingSpeed = 4f;
    public float wallJumpTime = 0.4f;
    public float wallJumpPushBack;
    public float wallJumpStr;
    public float wallJumpSlowdown = 0.5f;
    public float doubleJumpStr;
    public int doubleJumpMax;

    private PlayerInputScript input;
    private Vector2 movementVector = Vector2.zero;
    private bool isFacingRight = true;
    private float coyotteTimeCounter;
    private float jumpBufferCounter;
    private float jumpCancelBufferCounter;
    private bool isWallSliding;
    private bool isWallJumping;
    private int doubleJumpCounter;

    private void Awake()
    {
        input = new PlayerInputScript();
    }

    private void OnEnable()
    {
        input.Enable();
        input.PlatformInput.Movement.performed += MovementPerformed;
        input.PlatformInput.Movement.canceled += MovementCanceled;
    }

    private void MovementPerformed(InputAction.CallbackContext context)
    {
        movementVector = context.ReadValue<Vector2>();
    }

    private void MovementCanceled(InputAction.CallbackContext context)
    {
        movementVector = Vector2.zero;
    }

    /* Legacy, from the old way jumps were performed. Since these functions only ever executed
    private void JumpPerformed(InputAction.CallbackContext context)
    {
        Jump();
    }
    
    private void JumpCanceled(InputAction.CallbackContext context)
    {
        coyotteTimeCounter = 0f;
        prb.velocity = new Vector2(prb.velocity.x, prb.velocity.y * jumpSlowdown);
    }
    */

    private bool isGrounded()
    {
        bool isGround1 = Physics2D.OverlapCircle(groundCheck1.position, 0.02f, groundLayer);
        bool isGround2 = Physics2D.OverlapCircle(groundCheck2.position, 0.02f, groundLayer);
        bool isGround3 = Physics2D.OverlapCircle(groundCheck3.position, 0.02f, groundLayer);
        return isGround1 || isGround2 || isGround3;
    }

    private bool canBuffer()
    {
        return Physics2D.OverlapCircle(bufferCheck.position, 0.02f, groundLayer);
    }

    private bool isTouchingWall()
    {
        bool isWall1 = Physics2D.OverlapCircle(wallCheck1.position, 0.02f, groundLayer);
        bool isWall2 = Physics2D.OverlapCircle(wallCheck2.position, 0.02f, groundLayer);
        return isWall1 || isWall2;
    }

    private void Move()
    {
        prb.velocity = new Vector2(movementVector.x * latSpeed, prb.velocity.y);
    }

    private void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 localscale = transform.localScale;
        localscale.x *= -1f;
        transform.localScale = localscale;
    }

    private void Jump()
    {
        jumpBufferCounter = 0f;
        prb.velocity = new Vector2(prb.velocity.x, jumpStr);
    }

    private void JumpCancel()
    {
        jumpCancelBufferCounter = 0;
        prb.velocity = new Vector2(prb.velocity.x, prb.velocity.y * jumpSlowdown);
    }

    private void WallSlide()
    {
        if (isTouchingWall() && !isGrounded() && movementVector.x != 0f)
        {
            isWallSliding = true;
            prb.velocity = new Vector2(prb.velocity.x, Mathf.Clamp(prb.velocity.y, -wallSlidingSpeed, float.MaxValue));
        }
        else
        {
            isWallSliding = false;
        }
    }

    private void WallJump()
    {
        jumpBufferCounter = 0f;
        isWallJumping = true;
        Debug.Log(isWallJumping);
        float jumpDirection = -transform.localScale.x;
        prb.velocity = new Vector2(jumpDirection * (wallJumpPushBack + Math.Abs(movementVector.x)), wallJumpStr);
        Invoke(nameof(CancelWallJump), wallJumpTime);
    }

    private void CancelWallJump()
    {
        isWallJumping = false;
        Debug.Log(isWallJumping);
    }

    private void DoubleJump()
    {
        jumpBufferCounter = 0;
        doubleJumpCounter -= 1;
        prb.velocity = new Vector2(prb.velocity.x, doubleJumpStr);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!isFacingRight && movementVector.x > 0f)
        {
            Flip();
        }
        if (isFacingRight && movementVector.x < 0f)
        {
            Flip();
        }

        WallSlide();


        /* This part of the function will reset a bunch of player variables back to their max values. Think air dashes, stamina,
        double jumps... For now though, all it resets is the coyotte time counter (which measures if the player left the ground,
        i.e. walked off a platform, recently enough that they still have their initial jump.) If the player isn't grounded,
        your coyotte time starts running out.*/
        if (isGrounded())
        {
            coyotteTimeCounter = coyotteTime;
            doubleJumpCounter = doubleJumpMax;
            isWallJumping = false;
        }
        else
        {
            coyotteTimeCounter -= Time.deltaTime;
        }

        /* This part starts the jump buffer timer at its max value if the jump key was just pressed, and the player is within "buffer distance" of the ground.
        If the player doesn't hit the jump button, then the timer runs down (i.e. infinitely low if the button is never pressed, but that's unlikely to happen).
        Do note that with this method, the bufferCheck GameObject has to be moved downwards to allow for earlier buffering.*/
        if (input.PlatformInput.Jump.WasPerformedThisFrame())
        {
            jumpBufferCounter = jumpBufferTime;
            
        }
        else
        {
            jumpBufferCounter -= Time.deltaTime;
        }

        /* This part starts the jump cancel buffer timer. It works similarly to the jump buffer timer, except the trigger is once
        the jump key is released.*/
        if (input.PlatformInput.Jump.WasReleasedThisFrame())
        {
            jumpCancelBufferCounter = jumpBufferTime;
        }
        else
        {
            jumpCancelBufferCounter -= Time.deltaTime;
        }

        if (!isWallJumping)
        {
            Move();
        }
        if (isWallJumping && input.PlatformInput.Movement.WasReleasedThisFrame())
        {
            prb.velocity = new Vector2(wallJumpSlowdown*prb.velocity.x, prb.velocity.y);
        }

        /* This part runs the actual jump. It only executes if the jump buffer counter is still higher than zero (so if you're
        still within the timeframe to buffer a jump after pressing), and you have coyotte time left (i.e. you just left the ground
        or are currently grounded).*/
        if (jumpBufferCounter > 0f)
        {
            if (isWallSliding)
            {
                WallJump();
            }
            else if (coyotteTimeCounter > 0f)
            {
                Jump();
            }
            else if (doubleJumpCounter > 0 && !canBuffer())
            {
                Debug.Log(doubleJumpCounter);
                DoubleJump();
            }
        }

        /* This part runs if the jump was canceled (i.e. the key was realeased) within the buffer time frame, and the player is moving upwards.
        It works by multiplying the upwards velocity of the player by something lesser than 1 (i.e. dramatically slowing down their ascent).
        It doesn't allow the player to un-cancel their jump right before landing, but that could certainly be implemented, if necessary.*/
        if (jumpCancelBufferCounter > 0f && prb.velocity.y > 0f)
        {
            JumpCancel();
        }
    }
}
