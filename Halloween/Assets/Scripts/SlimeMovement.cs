using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SlimeMovement : MonoBehaviour {

    [Header("movement")]
    public float runWalkThreshhold;
    public int maxJumps = 2;
    public MovementValues AirMovement;
    public MovementValues GroundMovement;
    public MovementValues RollingAirMovement;
    public MovementValues RollingGroundMovement;

    [Header("Events")]
    public UnityEvent landed;
    public UnityEvent takeOff;

    public UnityEvent startedRolling;
    public UnityEvent stoppedRolling;

    Animator animator;
    new Rigidbody2D rigidbody2D;
    SpriteRenderer spriteRenderer;
    int jumpCount = 0;
    int activeJumpUpCount = 0;
    CapsuleCollider2D capsuleCollider2D;

    bool iOnGround = false;
    public bool onGround
    {
        get {
            return iOnGround;
        }
        set
        {
            if(value!=iOnGround)
            {
                iOnGround = value;
                if(value==true)
                {
                    animator.SetBool("onGround", true);
                    landed.Invoke();
                }
                else
                {
                    animator.SetBool("onGround", false);
                    takeOff.Invoke();
                }
            }
        }
    }
    bool iIsRolling = false;
    public bool isRolling
    {
        get
        {
            return iIsRolling;
        }
        set
        {
            if (value != iIsRolling)
            {
                iIsRolling = value;
                if (value == true)
                {
                    animator.SetBool("isRolling", true);
                    startedRolling.Invoke();
                }
                else
                {
                    animator.SetBool("isRolling", false);
                    stoppedRolling.Invoke();
                }
            }
        }
    }

    [System.Serializable]
    public struct MovementValues
    {
        public float horizontalSpeed;
        public float jumpForce;
        public float jumpDuration;
    }

    LayerMask layerMask;
    // Use this for initialization
    void Awake ()
    {
        animator = GetComponent<Animator>();
        rigidbody2D = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        capsuleCollider2D = GetComponent<CapsuleCollider2D>();
        jumpCount = maxJumps;

        //should be safe???
        
        
        int intMask = 0;
        intMask |= (1 << LayerMask.NameToLayer("Player"));
        layerMask = ~intMask;


        bool selfCast = Physics2D.CapsuleCast(capsuleCollider2D.offset + (Vector2)transform.position, capsuleCollider2D.size*0.95f, CapsuleDirection2D.Vertical, transform.rotation.eulerAngles.z, -transform.up, 0, layerMask);
        bool DownCast = Physics2D.CircleCast(capsuleCollider2D.offset + (Vector2)(transform.position) - new Vector2(0,((capsuleCollider2D.size.y - capsuleCollider2D.size.x)/2f + capsuleCollider2D.size.x/2f)), capsuleCollider2D.size.x * 0.49f, -transform.up, capsuleCollider2D.size.x * 0.05f, layerMask);
        Debug.Log(DownCast + " " + selfCast);
        if (DownCast == true && selfCast == false)
        {
            onGround = true;
        }
        else
        {
            onGround = false;
        }
    }
    

    // Update is called once per frame
    void Update ()
    {
        bool oldOnGround = onGround;
        RaycastHit2D rchData= Physics2D.CapsuleCast(capsuleCollider2D.offset + (Vector2)transform.position, capsuleCollider2D.size * 0.999f, CapsuleDirection2D.Vertical, transform.rotation.eulerAngles.z, -transform.up, capsuleCollider2D.size.x * 0.02f, layerMask);
        if (rchData.distance >0)
        {
            onGround = true;
        }
        else
        {
            onGround = false;
        }

        if (onGround==true && oldOnGround == false)
        {
            jumpCount = maxJumps;
        }

        MovementValues movementValues;
        if(isRolling==false)
        {
            if (onGround == true)
                movementValues = GroundMovement;
            else
                movementValues = AirMovement;
        }
        else
        {
            if (onGround == false)
                movementValues = RollingGroundMovement;
            else
                movementValues = RollingAirMovement;
        }
        
        if(Input.GetButtonDown("Jump") && (onGround==true || jumpCount>0))
        {
            StartCoroutine(Jump(movementValues.jumpForce, movementValues.jumpDuration));
        }

        rigidbody2D.velocity = new Vector2( Input.GetAxis("Horizontal") * movementValues.horizontalSpeed, rigidbody2D.velocity.y);
        if(Mathf.Abs(rigidbody2D.velocity.x)>runWalkThreshhold)
        {
            animator.SetBool("running", true);
            animator.SetBool("walking", false);
            animator.SetBool("idle", false);
        }
        else if (Mathf.Abs(rigidbody2D.velocity.x) > 0)
        {
            animator.SetBool("running", false);
            animator.SetBool("walking", true);
            animator.SetBool("idle", false);
        }
        else
        {
            animator.SetBool("running", false);
            animator.SetBool("walking", false);
            animator.SetBool("idle", true);
        }

        if(rigidbody2D.velocity.x>0)
        {
            spriteRenderer.flipY = false;
        }
        else if (rigidbody2D.velocity.x < 0)
        {
            spriteRenderer.flipY = true;
        }

        if (onGround == false && activeJumpUpCount<=0)
        {
            //fall at double speed
            if (rigidbody2D.velocity.y < 0)
            {
                rigidbody2D.AddForce(new Vector2(0, rigidbody2D.gravityScale));
            }
        }
        
    }

    IEnumerator Jump(float jumpForce, float jumpDuration)
    {
        jumpCount--;
        float targetTime = Time.time + jumpDuration;

        activeJumpUpCount++;
        while (Input.GetAxis("Jump")>0.5 && Time.time<targetTime)
        {
            rigidbody2D.AddForce(new Vector2(0, jumpForce));
            yield return null;
        }
        activeJumpUpCount--;
    }
}
