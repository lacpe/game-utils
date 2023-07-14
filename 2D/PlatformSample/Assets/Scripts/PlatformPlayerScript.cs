using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlatformPlayerScript : MonoBehaviour
{
    public Rigidbody2D prb;
    public Transform groundCheck;
    public Transform bufferCheck;
    public Transform wallCheck;
    public LayerMask groundLayer;
    public float latSpeed = 5;
    public float jumpStr = 5;
    public float jumpSlowdown = 0.5f;
    public float coyotteTime = 0.2f;
    public float jumpBufferTime = 0.2f;
    public float wallSlidingSpeed = 4f;

    private PlayerInputScript input;
    private Vector2 movementVector = Vector2.zero;
    private bool isFacingRight = true;
    private float coyotteTimeCounter;
    private float jumpBufferCounter;
    private float jumpCancelBufferCounter;
    private bool isWallSliding;

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
        return Physics2D.OverlapCircle(groundCheck.position, 0.02f, groundLayer);
    }

    private bool canBuffer()
    {
        return Physics2D.OverlapCircle(bufferCheck.position, 0.02f, groundLayer);
    }

    private bool isTouchingWall()
    {
        return Physics2D.OverlapCircle(wallCheck.position, 0.02f, groundLayer);
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
        prb.velocity = new Vector2(prb.velocity.x, jumpStr);
        jumpBufferCounter = 0f;
    }

    private void wallSlide()
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

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        prb.velocity = new Vector2(movementVector.x * latSpeed, prb.velocity.y);
        if (!isFacingRight && movementVector.x > 0f)
        {
            Flip();
        }
        if (isFacingRight && movementVector.x < 0f)
        {
            Flip();
        }

        wallSlide();

        /* This part of the function will reset a bunch of player variables back to their max values. Think air dashes, stamina,
        double jumps... For now though, all it resets is the coyotte time counter (which measures if the player left the ground,
        i.e. walked off a platform, recently enough that they still have their initial jump.) If the player isn't grounded,
        your coyotte time starts running out.*/
        if (isGrounded())
        {
            coyotteTimeCounter = coyotteTime;
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
            if (!canBuffer() && coyotteTimeCounter < 0f)
            {
                // Put double jump function here

            }
            else
            {
                jumpBufferCounter = jumpBufferTime;
            }
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

        /* This part runs the actual jump. It only executes if the jump buffer counter is still higher than zero (so if you're
        still within the timeframe to buffer a jump after pressing), and you have coyotte time left (i.e. you just left the ground
        or are currently grounded).*/
        if (coyotteTimeCounter > 0f && jumpBufferCounter > 0f)
        {
            jumpBufferCounter = 0;
            prb.velocity = new Vector2(prb.velocity.x, jumpStr);
        }

        /* This part runs if the jump was canceled (i.e. the key was realeased) within the buffer time frame, and the player is moving upwards.
        It works by multiplying the upwards velocity of the player by something lesser than 1 (i.e. dramatically slowing down their ascent).
        It doesn't allow the player to un-cancel their jump right before landing, but that could certainly be implemented, if necessary.*/
        if (jumpCancelBufferCounter > 0f && prb.velocity.y > 0f)
        {
            jumpCancelBufferCounter = 0;
            prb.velocity = new Vector2(prb.velocity.x, prb.velocity.y * jumpSlowdown);
        }
    }
}
