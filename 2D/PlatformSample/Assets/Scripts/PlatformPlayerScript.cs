/*
Part of this code were taken from DawnosaurDev's public repository here: https://github.com/DawnosaurDev/platformer-movement/blob/main/Scripts/Run%20Only/PlayerRun.cs
Other parts were taken from tutorials by bendux: https://www.youtube.com/@bendux
Other stuff was my own customization.
*/

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlatformPlayerScript : MonoBehaviour
{
    [Header("Script-controlled components")]
    #region Components
    // Using DawnosaurDev's method, I have a script that contains all of the player's stats. Or at the very least, it will contain all of that later.
    public PlatformPlayerData Data;
    public Rigidbody2D prb;
    public AudioSource audio;
    #endregion

    [Header("State-check GameObjects")]
    #region State Checks
    public Transform groundCheck1;
    public Transform groundCheck2;
    public Transform groundCheck3;
    public Transform bufferCheck;
    public Transform wallCheck1;
    public Transform wallCheck2;
    public LayerMask groundLayer;
    #endregion

    private PlayerInputScript input;
    private Vector2 movementVector = Vector2.zero;

    #region General purpose booleans
    private bool isFacingRight = true;
    private bool isWallSliding;
    private bool isJumping;
    private bool isWallJumping;
    private bool isDoubleJumping;
    private bool wantsDash;
    private bool isDashing;
    private bool isDashCancel;
    #endregion

    #region Timers
    private float coyotteTimeCounter;
    private float jumpBufferCounter;
    private float jumpCancelBufferCounter;
    private int doubleJumpCounter;
    private float dashCooldownTimer;
    private int dashCounter;
    #endregion

    [HideInInspector] public static InputSettings settings;

    private void Awake()
    {
        input = new PlayerInputScript();
        isJumping = false;
    }

    private void OnEnable()
    {
        input.Enable();
        input.PlatformInput.Movement.performed += MovementPerformed;
        input.PlatformInput.Movement.canceled += MovementCanceled;
        input.PlatformInput.Dash.performed += DashPerformed;
    }

    private void MovementPerformed(InputAction.CallbackContext context)
    {
        movementVector = context.ReadValue<Vector2>();
    }

    private void MovementCanceled(InputAction.CallbackContext context)
    {
        movementVector = Vector2.zero;
    }

    private void DashPerformed(InputAction.CallbackContext context)
    {
        wantsDash = true;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        isDashCancel = false;
        Debug.Log(isDashCancel);
    }

    /* Legacy, from the old way jumps were performed. Since these functions only ever executed when the jump input was pressed,
       input buffering was completely impossible using this method.
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
        float targetSpeed = movementVector.x * Data.maxGroundSpeed;

        #region Calculating the accelaration rate
        float accelRate;
        if (isGrounded())
            accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? Data.groundAccelAmount : Data.groundDeccelAmount;
        else
            accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? Data.groundAccelAmount * Data.airAccelModifier : Data.groundDeccelAmount * Data.airDeccelModifier;
        #endregion

        #region Slowing player down when wall kicking
        if (isWallJumping)
            accelRate *= Data.wallAccelModifier;
        #endregion

        #region Momentum conservation
        //Prevent any deceleration, or conserve momentum. Might want to look into allowing players to speed up slightly.
        if (Data.conserveMomentum && Mathf.Abs(prb.velocity.x) > Mathf.Abs(targetSpeed) && Mathf.Sign(prb.velocity.x) == Mathf.Sign(targetSpeed) && targetSpeed > 0.01f && isGrounded())
            accelRate = 0;
        #endregion

        if (Data.conserveMomentum && isDashCancel && (Mathf.Sign(prb.velocity.x) == Mathf.Sign(targetSpeed) || targetSpeed == 0))
            accelRate = 0;

        #region Calculating acceleration & force
        float speedDif = targetSpeed - prb.velocity.x; // Difference between current velocity and desired velocity
        float moveForce = speedDif * accelRate; // Caculate the amount of force to be applied
        #endregion

        prb.AddForce(moveForce * Vector2.right, ForceMode2D.Force);

        /* Legacy way to do movement. This was cool because it featured acceleration, but it didn't use forces which was bound to cause problems down the line.
        if (input.PlatformInput.Movement.IsPressed())
        {
            currentSpeed += acceleration * Time.deltaTime;
        }
        else
        {
            currentSpeed = 0;
        }
        prb.velocity = new Vector2(movementVector.x * Mathf.Min(currentSpeed, latSpeed), prb.velocity.y);
        */
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
        isJumping = true;
        if (isDashing)
        {
            isDashCancel = true;
            isDashing = false;
        }
        prb.AddForce(Vector2.up * Data.jumpPower, ForceMode2D.Impulse);
    }

    private void JumpCancel()
    {
        jumpCancelBufferCounter = 0;
        prb.velocity = new Vector2(prb.velocity.x, prb.velocity.y * Data.jumpSlowdown);
    }

    private void WallSlide()
    {
        if (isTouchingWall() && !isGrounded() && movementVector.x != 0f)
        {
            isWallSliding = true;
            prb.velocity = new Vector2(prb.velocity.x, Mathf.Clamp(prb.velocity.y, -Data.wallSlidingSpeed, float.MaxValue));
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
        float jumpDirection = -transform.localScale.x;
        if (isDashing && prb.velocity.y > 0f)
        {
            prb.AddForce(new Vector2(jumpDirection * (Data.wallKickOff), Data.wallDashPower), ForceMode2D.Impulse);
            //isDashCancel = true;
            isDashing = false;
        }
        else
        {
            prb.velocity = new Vector2(prb.velocity.x, 0);
            prb.AddForce(new Vector2(jumpDirection * (Data.wallKickOff), Data.wallJumpPower), ForceMode2D.Impulse);
        }

        Invoke(nameof(CancelWallJump), Data.wallAccelTime);
    }

    private void CancelWallJump()
    {
        isWallJumping = false;
    }

    private void DoubleJump()
    {
        jumpBufferCounter = 0;
        doubleJumpCounter -= 1;
        isDoubleJumping = true;
        if (prb.velocity.y < 0f)
            prb.velocity = new Vector2(prb.velocity.x, 0);
        prb.AddForce(Vector2.up * Data.doubleJumpPower, ForceMode2D.Impulse);
    }

    private void Dash()
    {
        #region Setting all state parameters
        wantsDash = false;
        dashCounter -= 1;
        isDashing = true;
        dashCooldownTimer = Data.dashCooldown;
        #endregion

        #region Getting dash direction
        Vector2 dashDirection;
        if (movementVector == Vector2.zero)
            dashDirection = new Vector2(transform.localScale.x, 0);
        else
            dashDirection = movementVector.normalized;
        #endregion

        #region Doing physics
        prb.velocity = Vector2.zero; // Necessary so previously existing upwards momentum doesn't mess with the dash
        prb.gravityScale = 0; // Necessary so that dash travels in a straight line
        prb.AddForce(dashDirection * Data.dashPower, ForceMode2D.Impulse);
        #endregion

        Invoke(nameof(DashEnded), Data.dashDuration);
    }

    private void DashEnded()
    {
        if (isDashing)
        {
            Debug.Log("I ran");
            prb.velocity = Vector2.zero;
            if (!isGrounded())
                Invoke(nameof(DashGraceEnded), Data.dashGraceTime);
            else
                isDashing = false;
        }
    }

    private void DashGraceEnded()
    {
        isDashing = false;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }
    private void FixedUpdate()
    {
        #region Ground check
        /* This part of the function will reset a bunch of player variables back to their max values. Think air dashes, stamina,
        double jumps... For now though, all it resets is the coyotte time counter (which measures if the player left the ground,
        i.e. walked off a platform, recently enough that they still have their initial jump.) If the player isn't grounded,
        your coyotte time starts running out.*/
        if (isGrounded())
        {
            coyotteTimeCounter = Data.coyotteTime;
            doubleJumpCounter = Data.doubleJumpMax;
            dashCounter = Data.dashMax;
            isJumping = false;
            isWallJumping = false;
            isDoubleJumping = false;
        }
        else
        {
            coyotteTimeCounter -= Time.deltaTime;
        }
        #endregion

        #region Setting gravity scale based on situation
        // If the player has no vertical velocity (i.e. they are grounded) or going upwards, gravity is lessened.
        if (prb.velocity.y >= 0f)
            prb.gravityScale = Data.jumpGravity;
        // If the player is moving downwards (i.e. they are falling), the gravity is raised.
        else if (prb.velocity.y < 0f)
            prb.gravityScale = Data.jumpGravity * Data.fallGravityModifier;
        /* If the player is currently jumping, and their vertical velocity is low
           (i.e. they are at the apex of their jump), gravity is drastically reduced to give them more hang time. */
        if ((isJumping || isWallJumping || isDoubleJumping) && Mathf.Abs(prb.velocity.y) < Data.hangAbsVelocity)
            prb.gravityScale *= Data.hangGravityModifier;
        // If the player is currently performing a dash, there is no gravity at all.
        if (isDashing)
            prb.gravityScale = 0;
        /* Note that because of the order of if statements, the priority always goes dash gravity > hang time gravity > rising/falling gravity.*/
        #endregion

        /* This part runs the actual jump. It only executes if the jump buffer counter is still higher than zero (so if you're
        still within the timeframe to buffer a jump after pressing), and you have coyotte time left (i.e. you just left the ground
        or are currently grounded).*/
        if (jumpBufferCounter > 0f)
        {
            if (isTouchingWall() && !isGrounded())
            {
                WallJump();
            }
            else if (coyotteTimeCounter > 0f)
            {
                Jump();
            }
            else if (doubleJumpCounter > 0 && !canBuffer() && !isDashing)
            {
                DoubleJump();
            }
        }

        /* This part runs if the jump was canceled (i.e. the key was realeased) within the buffer time frame, and the player is moving upwards.
        It works by multiplying the upwards velocity of the player by something lesser than 1 (i.e. dramatically slowing down their ascent).
        It doesn't allow the player to un-cancel their jump right before landing, but that could certainly be implemented, if necessary.*/
        if (jumpCancelBufferCounter > 0f && prb.velocity.y > 0f && !isWallJumping)
        {
            JumpCancel();
        }

        if (wantsDash && dashCooldownTimer < 0f && dashCounter > 0)
            Dash();

        if (!isDashing)
            Move();

        if (prb.velocity.y < 0f)
            prb.velocity = new Vector2(prb.velocity.x, Mathf.Max(-Data.maxFallSpeed, prb.velocity.y));
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

        /* This part starts the jump buffer timer at its max value if the jump key was just pressed, and the player is within "buffer distance" of the ground.
        If the player doesn't hit the jump button, then the timer runs down (i.e. infinitely low if the button is never pressed, but that's unlikely to happen).
        Do note that with this method, the bufferCheck GameObject has to be moved downwards to allow for earlier buffering.*/
        if (input.PlatformInput.Jump.WasPerformedThisFrame())
        {
            jumpBufferCounter = Data.jumpBufferTime;
            jumpCancelBufferCounter = 0;
        }
        else
        {
            jumpBufferCounter -= Time.deltaTime;
        }

        /* This part starts the jump cancel buffer timer. It works similarly to the jump buffer timer, except the trigger is once
        the jump key is released.*/
        if (input.PlatformInput.Jump.WasReleasedThisFrame())
        {
            jumpCancelBufferCounter = Data.jumpBufferTime;
        }
        else
        {
            jumpCancelBufferCounter -= Time.deltaTime;
        }

        if (!isDashing)
            dashCooldownTimer -= Time.deltaTime;

        /* Old idea that I removed
        if (isWallJumping && input.PlatformInput.Movement.WasReleasedThisFrame())
        {
            prb.velocity = new Vector2(wallJumpSlowdown*prb.velocity.x, prb.velocity.y);
        }
        */
    }
}
