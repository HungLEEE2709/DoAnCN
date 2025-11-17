using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControl : MonoBehaviour
{
    public float moveSpeed = 5f;
    private Rigidbody2D myRigidbody2D;
    private Animator myAnimator;
    private bool facingRight = true;

    void Start()
    {
        myRigidbody2D = gameObject.GetComponent<Rigidbody2D>();
        myAnimator = gameObject.GetComponent<Animator>();
    }

    void Update()
    {
        float MoveInput = Input.GetAxis("Horizontal");

        // -------------------------------
        //  ATTACK (PHÍM 1)
        // -------------------------------
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            myAnimator.SetTrigger("Atk");
        }

        // -------------------------------
        //  MOVE + RUN ANIMATION
        // -------------------------------
        if (MoveInput != 0)
        {
            myRigidbody2D.velocity = new Vector2(MoveInput * moveSpeed, myRigidbody2D.velocity.y);
            myAnimator.SetBool("Run", true);

            if (MoveInput > 0 && !facingRight)
                Flip();
            else if (MoveInput < 0 && facingRight)
                Flip();
        }
        else
        {
            myAnimator.SetBool("Run", false);
        }
    }

    void Flip()
    {
        facingRight = !facingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }
}
