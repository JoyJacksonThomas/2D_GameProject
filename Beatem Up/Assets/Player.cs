using System.Collections;
using System.Collections.Generic;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Assertions.Must;

public class Player : MonoBehaviour
{
    Rigidbody2D rb;
    SpriteRenderer spriteRenderer;
    Animator animator;

    public float MaxSpeed;
    public float MoveForce;
    public float JumpForce;
    public float SlowDownFactor;

    bool facingRight = true;

    float horizontal;
    bool jump;
    public bool grounded;

    public Vector2 acceleration;
    float oneOverFixedTimeStep;
    public float PeekJumpThreshold;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        Time.timeScale = 1f;

        oneOverFixedTimeStep = 1 / Time.fixedDeltaTime;
    }

    // Update is called once per frame
    void Update()
    {
        getInput();

        calcAcceleration();
        if (rb.velocity.y < PeekJumpThreshold && animator.GetCurrentAnimatorStateInfo(0).IsName("JumpAscending"))
        {
            animator.SetBool("PeekJump", true);
        }
    }

    private void FixedUpdate()
    {

        

        if(jump && grounded)
        {
            animator.SetBool("StartJump", true);
            jump = false;
        }

        horizontalMovement();
    }

    void getInput()
    {
        horizontal = Input.GetAxis("Horizontal");

        if (Mathf.Abs(horizontal) < .5f && animator.GetCurrentAnimatorStateInfo(0).IsName("Run"))
        {
            animator.SetBool("Running", false);
            animator.SetBool("Sliding", true);
        }
        else if(Mathf.Abs(horizontal) >= .5f && !animator.GetCurrentAnimatorStateInfo(0).IsName("Run"))
        {
            animator.SetBool("Running", true);
        }

        if (!jump) //jump is set to false once the action is performed in the fixed update.
        {
            jump = Input.GetButtonDown("Jump");
        }
        
    }

    void horizontalMovement()
    {
        //if(animator.GetCurrentAnimatorStateInfo(0).IsName("JumpStart"))
        //{
        //    rb.velocity = Vector2.zero;
        //    return;
        //}

        if (facingRight && rb.velocity.x < 0)
        {
            facingRight = false;
            spriteRenderer.flipX = true;
        }
        else if (!facingRight && rb.velocity.x > 0)
        {
            facingRight = true;
            spriteRenderer.flipX = false;
        }

        rb.AddForce(new Vector2(MoveForce * horizontal, 0));

        if (horizontal == 0)
        {
            float xSpeed = rb.velocity.x;
            xSpeed *= SlowDownFactor;
            rb.velocity = new Vector2(xSpeed, rb.velocity.y);
        }

        if (Mathf.Abs(rb.velocity.x) > MaxSpeed)
        {
            Vector3 newVel = rb.velocity;
            if (newVel.x > 0)
            {
                newVel.x = MaxSpeed;
            }
            else
            {
                newVel.x = -MaxSpeed;
            }
            rb.velocity = newVel;
        }
    }

    void jumpOffGround()
    {
        rb.AddForce(new Vector2(0, JumpForce), ForceMode2D.Impulse);

        animator.SetBool("StartJump", false);
        animator.SetBool("PeekJump", false);
        animator.SetBool("LandJump", false);
    }

    public void toggleAnimationBool(string  boolName)
    {
        bool value = animator.GetBool(boolName);
        animator.SetBool(boolName, false);
    }

    Vector2 lastVel;
    void calcAcceleration()
    {
        acceleration = (rb.velocity - lastVel) * oneOverFixedTimeStep;
        lastVel = rb.velocity;
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.tag == "Ground")
        {
            grounded = true;
            animator.SetBool("LandJump", true);
            
        }
    }
    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Ground")
        {
            grounded = false;
        }
    }
}
