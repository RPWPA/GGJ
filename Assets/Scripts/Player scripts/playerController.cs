﻿using UnityEngine;

public class playerController : StateChanger
{
    float gizRange;
    bool dashAllowance, isDashing;
    const float dashAnimationTiming = 0.3f;
    public float runSpeed, jumpForce, fireForce, dashForce;
    float moveInput;
    bool facingRight = true;
    private Animator anim;
    Vector2 range = new Vector2(0.18f, 1f);
    public Transform groundCheck;
    public LayerMask groundLayer;
    public AudioClip jumpSound;
    public AudioClip movementSound;
    public AudioClip dashSound;

    Rigidbody2D myBody;
    int floatAttr;
    void Awake()
    {
        myBody = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        dashAllowance = true;
        gizRange = 0.3f;
    }
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.black;
        Gizmos.DrawWireSphere(groundCheck.position, gizRange);
    }
    void checkColForJump()
    {
        Collider2D bottomHit = Physics2D.OverlapBox(groundCheck.position, range, 0.0f, groundLayer);
        if (bottomHit != null)
        {
            if (bottomHit.gameObject.tag == "Ground" && Input.GetKeyDown(KeyCode.Space))
            {
                int jumpAttr = MechanicsUpdater.GetValueOf("Jump");
                float jumpForce = jumpAttr == 0 ? 0 : jumpAttr + 5;
                myBody.velocity = new Vector2(myBody.velocity.x, jumpForce);
                anim.SetBool("jumping", true);
                soundManager.instance.playSfx(jumpSound, 0.3f);
            }
            else
            {
                anim.SetBool("jumping", false);
            }
        }

    }
    void Update()
    {
        CheckIfRevertRequested();
        //if (anim.GetCurrentAnimatorStateInfo(0).IsName("Death")) return;
        movement();
        checkColForJump();
        // dealing with dashing.
        if (Input.GetKeyDown(KeyCode.Z))
        {
            if (dashAllowance)
            {
                dash();
                anim.SetTrigger("dashing");
                soundManager.instance.playSfx(dashSound, 0.5f);
                Invoke("endDash", dashAnimationTiming);
            }
        }
        if (isDashing)
        {
            float dash = floatAttr == 0 ? 0 : ((floatAttr / 10.0f) * 40) + 10;
            if (facingRight)
            {
                myBody.velocity = new Vector2(dash, 0);
            }
            else
                myBody.velocity = new Vector2(-dash, 0);

            if (dashAllowance)
            {
                dashAllowance = false;
                Invoke("allowDashing", 2.0f);
            }
        }

        // Dealing with falling animation.
        if (myBody.velocity.y < -1) anim.SetBool("falling", true);
        else anim.SetBool("falling", false);
    }
    void movement()
    {
        moveInput = Input.GetAxisRaw("Horizontal") * runSpeed;
        if (moveInput != 0)
        {
            anim.SetBool("moving", true);
            soundManager.instance.playSfx(movementSound, 0.5f);
        }
        else
        {
            anim.SetBool("moving", false);
        }
        myBody.velocity = new Vector2(moveInput, myBody.velocity.y);
        if (moveInput > 0 && !facingRight || moveInput < 0 && facingRight)
        {
            flip();
        }
    }
    void flip()
    {
        facingRight = !facingRight;
        Vector2 transformScale = transform.localScale;
        transformScale.x *= -1;
        transform.localScale = transformScale;
    }
    void allowDashing()
    {
        dashAllowance = true;
    }
    void dash()
    {
        isDashing = true;
        floatAttr = MechanicsUpdater.GetValueOf("Dash");
    }
    void endDash()
    {
        isDashing = false;
        Invoke("allowDashing", 1.0f);
    }
    protected override void RevertState()
    {
        transform.position = new Vector3(-7, 1, 0);
        myBody.isKinematic = true;
        myBody.velocity = Vector2.zero;
        myBody.isKinematic = false;
        dashAllowance = true;
        isDashing = false;
    }
}