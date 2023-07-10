using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlatformPlayerScript : MonoBehaviour
{
    public Rigidbody2D prb;
    public Transform groundCheck;
    public LayerMask groundLayer;
    public float latSpeed = 5;
    public float jumpStr = 5;
    public float jumpSlowdown = 0.5f;
    public float coyotteTime = 5f;

    private PlayerInputScript input;
    private Vector2 movementVector = Vector2.zero;
    private bool isFacingRight = true;
    private float coyotteTimer;
    private int spaceBarPresses;

    private void Awake()
    {
        input = new PlayerInputScript();
        coyotteTimer = 0f;
    }

    private void OnEnable()
    {
        input.Enable();
        input.PlatformInput.Movement.performed += Movement;
        input.PlatformInput.Movement.canceled += MovementCanceled;
        input.PlatformInput.Jump.performed += Jump;
        input.PlatformInput.Jump.canceled += JumpCanceled;
    }

    private void Movement(InputAction.CallbackContext context)
    {
        movementVector = context.ReadValue<Vector2>();
    }

    private void MovementCanceled(InputAction.CallbackContext context)
    {
        movementVector = Vector2.zero;
    }

    private void Jump(InputAction.CallbackContext context)
    {
        if (isAllowedJump() && spaceBarPresses < 1)
        {
            spaceBarPresses++;
            prb.velocity = new Vector2(prb.velocity.x, jumpStr);
        }
    }

    private void JumpCanceled(InputAction.CallbackContext context)
    {
        if (prb.velocity.y > 0f && spaceBarPresses <=1)
        {
            prb.velocity = new Vector2(prb.velocity.x, prb.velocity.y * jumpSlowdown);
        }
    }

    private bool isGrounded()
    {
        return Physics2D.OverlapCircle(groundCheck.position, 0.02f, groundLayer);
    }

    private bool isAllowedJump()
    {
        return coyotteTimer < coyotteTime;
    }

    private void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 localscale = transform.localScale;
        localscale.x = -1f;
        transform.localScale = localscale;
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
        if (isGrounded())
        {
            spaceBarPresses = 0;
            coyotteTimer = 0f;
        }
        if (!isGrounded())
        {
            coyotteTimer += Time.deltaTime;
        }
    }
}
