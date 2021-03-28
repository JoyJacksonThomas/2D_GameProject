
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;
using Rewired.ControllerExtensions;

public enum MovementStates
{
    GROUNDED,
    JUMPING_FROM_GROUND,
    NORMAL_RIDING,
    DRIFTING
}
public class WitchController : MonoBehaviour
{
    int playerId = 0;
    public MovementStates moveState = MovementStates.NORMAL_RIDING;

    [Header("Sprite Stuff")]
    [Space(5)]
    public SpriteRenderer spriteRenderer;
    public float broomTurnRate;
    public Sprite[] pBroomSprites;
    float broomSpriteIndexFloat;
    bool Spinning;

    //*************     INPUT       *****************
    Vector2 leftStick;
    float rightTrigger = 0;
    public bool jump = false;

    //*************     RigidBody   *****************
    Rigidbody2D rb;
    float currentAcceleration = 0;
    float oneOverMass = 0;
    float currentSpeed = 0;

    //*************     GROUNDED MOVEMENT   *****************
    [Space(10)]
    [Header("Grounded Movement")]
    public float moveForce_Ground;
    public float maxSpeed_Ground = 0;
    public float slowDownFactor_Ground;
    public float jumpForce_Ground;
    public float airInfluence;
    public float maxSpeed_Air;

    //*************     BROOM MOVEMENT  *****************
    [Space(10)]
    [Header("Broomstick Movement")]
    public float broomForce;
    public int[] turnDegrees;
    Transform broomTrans;
    public float rotDir = 0;
    public float turnSpeed_Broom;
    public float maxSpeed_Broom = 0;


    private Rewired.Player player { get { return ReInput.players.GetPlayer(playerId); } }

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        broomTrans = spriteRenderer.transform.parent;
        oneOverMass = 1 / rb.mass;
    }

    // ****************************************      UPDATE     ************************************************************
    // Update is called once per frame
    void Update()
    {
        getInput();
        AdjustRotation();

    }

    void AdjustRotation()
    {
        switch (moveState)
            {
            case MovementStates.NORMAL_RIDING:
                if (leftStick.sqrMagnitude > .8 && currentSpeed > 0)
                {
                    float angle = Mathf.Atan2(leftStick.y, leftStick.x) * Mathf.Rad2Deg;
                    Quaternion q = Quaternion.AngleAxis(angle, Vector3.forward);
                    broomTrans.rotation = Quaternion.Slerp(broomTrans.rotation, q, Time.deltaTime * turnSpeed_Broom);
                    calculateBroomSprite_3();
                }
                break;
            case MovementStates.DRIFTING:
                break;
        }
        
    }

    void calculateBroomSprite_3()
    {
        if (leftStick.sqrMagnitude > .8)
        {
            

            float currentBroomSpriteIndexFloat = broomSpriteIndexFloat + pBroomSprites.Length;

            rotDir = 0;
            if (leftStick.sqrMagnitude > .1f)// && Vector2.Dot(leftStick, spriteRenderer.transform.parent.right) < .9)
            {
                if ((spriteRenderer.transform.parent.InverseTransformPoint(leftStick + (Vector2)transform.position)).y >= 0) rotDir = -1;
                else rotDir = 1;
            }

            if((leftStick.x < 0 && rb.velocity.x >= 0) || (leftStick.x > 0 && rb.velocity.x <= 0))
            {
                Spinning = true;
            }
            

            if(Spinning)
            {
                currentBroomSpriteIndexFloat += broomTurnRate * rotDir;

                if ((int)broomSpriteIndexFloat % (pBroomSprites.Length + 1) != (int)currentBroomSpriteIndexFloat % (pBroomSprites.Length + 1))
                {
                    int index = (int)currentBroomSpriteIndexFloat % (pBroomSprites.Length);
                    //if (index == -1) index = pBroomSprites.Length - 1;
                    spriteRenderer.sprite = pBroomSprites[index];
                }

                broomSpriteIndexFloat = currentBroomSpriteIndexFloat % (pBroomSprites.Length);

                if((rb.velocity.x < 0 && spriteRenderer.sprite == pBroomSprites[5]) || (rb.velocity.x > 0 && spriteRenderer.sprite == pBroomSprites[0]))
                {
                    Spinning = false;
                }
            }
        }
    }
    // ***************************************************    FIXED UPDATE    ************************************************************
    private void FixedUpdate()
    {
        
        Jump();
        Accelerate();
        
    }

    void getInput()
    {
        if (!ReInput.isReady) return;


        leftStick.x = player.GetAxis("MoveHorizontal");
        leftStick.y = player.GetAxis("MoveVertical");

        rightTrigger = player.GetAxis("Accelerate");

        if(jump != true)
        {
            jump = player.GetButtonDown("Jump");
        }
        
    }

    void Accelerate()
    {
        switch (moveState)
        {
            case MovementStates.GROUNDED:
                rb.AddForce(new Vector2(moveForce_Ground * leftStick.x, 0));

                if (leftStick.x == 0)
                {
                    float xSpeed = rb.velocity.x;
                    xSpeed *= slowDownFactor_Ground;
                    rb.velocity = new Vector2(xSpeed, rb.velocity.y);
                }

                if (Mathf.Abs(rb.velocity.x) > maxSpeed_Ground)
                {
                    Vector3 newVel = rb.velocity;
                    if (newVel.x > 0)
                    {
                        newVel.x = maxSpeed_Ground;
                    }
                    else
                    {
                        newVel.x = -maxSpeed_Ground;
                    }
                    rb.velocity = newVel;
                }
                break;
            case MovementStates.JUMPING_FROM_GROUND:
                rb.AddForce(new Vector2(airInfluence * leftStick.x, 0));

                //if (leftStick.x == 0)
                //{
                //    float xSpeed = rb.velocity.x;
                //    xSpeed *= slowDownFactor_Ground;
                //    rb.velocity = new Vector2(xSpeed, rb.velocity.y);
                //}

                if (Mathf.Abs(rb.velocity.x) > maxSpeed_Air)
                {
                    Vector3 newVel = rb.velocity;
                    if (newVel.x > 0)
                    {
                        newVel.x = maxSpeed_Air;
                    }
                    else
                    {
                        newVel.x = -maxSpeed_Air;
                    }
                    rb.velocity = newVel;
                }
                break;
            case MovementStates.NORMAL_RIDING:
                if (currentSpeed < maxSpeed_Broom)
                {
                    currentAcceleration = rightTrigger * broomForce * oneOverMass;

                    currentSpeed += currentAcceleration * Time.deltaTime;
                }
                else
                {
                    currentSpeed = maxSpeed_Broom;
                }
                rb.velocity = broomTrans.right * currentSpeed;
                break;
            case MovementStates.DRIFTING:
                break;
        }
        
    }

    void Jump()
    {
        if(jump == true)
        {
            switch (moveState)
            {
                case MovementStates.GROUNDED:
                    rb.AddForce(new Vector2(0, jumpForce_Ground), ForceMode2D.Impulse);
                    moveState = MovementStates.JUMPING_FROM_GROUND;
                    jump = false;
                    break;
                case MovementStates.NORMAL_RIDING:

                    break;
                case MovementStates.DRIFTING:
                    break;
            }
        }
        
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Ground")
        {
            moveState = MovementStates.GROUNDED;

        }
    }
    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Ground")
        {
            
        }
    }
}

