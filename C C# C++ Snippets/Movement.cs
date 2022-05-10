/*
 * Movement.cs
 * Author(s): Albert Njubi
 * Based on code from Mix & Jam/André Cardoso https://github.com/mixandjam/Celeste-Movement
 * & TaroDev https://github.com/Matthew-J-Spencer/Ultimate-2D-Controller
 * Date Created: 7/1/19
 */
using System.Collections;
using UnityEngine;
using DG.Tweening;
using UnityEngine.InputSystem;


/// <summary>
/// This class handles the GameObject player movement, 
/// including its animations and stats.
/// </summary>
public class Movement : MonoBehaviour/*, EnemyHandler.IEnemyTargetable*/
{
    #region public variables
    public static Movement instance;
    [HideInInspector] public Rigidbody2D rb;

    [Space]
    public int side = 1;

    #region Public Floats
    [Space]
    [Header("Stats")]
    public float speed = 10;
    public float jumpForce = 50;
    public float slideSpeed = 5;
    public float wallJumpLerp = 10;
    public float dashSpeed = 20;
    private float jumpTimer;
    public float groundedTimer = 0;
    #endregion

    #region Public Bools
    [Space]
    [Header("Booleans")]
    public bool canMove;
    public bool wallGrab;
    public bool wallJumped;
    public bool wallSlide;
    public bool isDashing;
    #endregion

    #region Public ParticleSystem
    [Space]
    [Header("Polish")]
    public ParticleSystem dashParticle;
    public ParticleSystem jumpParticle;
    public ParticleSystem wallJumpParticle;
    public ParticleSystem slideParticle;
    public ParticleSystem dustParticle;
    public ParticleSystem PowerUpParticle;
    #endregion

    #endregion
    #region private variables
    private Collision coll;
    [HideInInspector] private AnimationScript anim;

    [Space]
    private bool hasDashed;
    #endregion

    #region private functions
    /// <summary>
    /// The start method retrives the player components, 
    /// being the Collider, Rigidbody2D and 
    /// AnimationScript Class.
    /// </summary>
    void Start()
    {
        coll = GetComponent<Collision>();
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponentInChildren<AnimationScript>();
    }

    /// <summary>
    /// This Update method handles the input, and boolen
    /// modifiers which determine how the player moves.
    /// BetterJumping class is referenced. 
    /// </summary>
    void Update()
    {
        float x = Input.GetAxis("Horizontal");
        float y = Input.GetAxis("Vertical");
        float xRaw = Input.GetAxisRaw("Horizontal");
        float yRaw = Input.GetAxisRaw("Vertical");
        Vector2 dir = new Vector2(x, y);
        dustParticle.Stop();
        

        Walk(dir);
        anim.SetHorizontalMovement(x, y, rb.velocity.y);

        jumpTimer -= Time.deltaTime;
        groundedTimer -= Time.deltaTime;

        var isGrounded = false;
        isGrounded |= coll.onGround;
        groundTouch = isGrounded;

        if (coll.onWall && Keyboard.current.shiftKey.wasPressedThisFrame && canMove)
        {
            if(side != coll.wallSide)
                anim.Flip(side*-1);
            wallGrab = true;
            wallSlide = false;
        }

        if (Keyboard.current.shiftKey.wasPressedThisFrame || !coll.onWall || !canMove)
        {
            wallGrab = false;
            wallSlide = false;
        }

        if (coll.onGround && !isDashing)
        {
            wallJumped = false;
            GetComponent<BetterJumping>().enabled = true;
        }
        
        if (wallGrab && !isDashing)
        {
            rb.gravityScale = 0;
            if(x > .2f || x < -.2f)
            rb.velocity = new Vector2(rb.velocity.x, 0);

            float speedModifier = y > 0 ? .5f : 1;

            rb.velocity = new Vector2(rb.velocity.x, y * (speed * speedModifier));
        }
        else
        {
            rb.gravityScale = 3;
        }

        if(coll.onWall && !coll.onGround)
        {
            if (x != 0 && !wallGrab)
            {
                wallSlide = true;
                WallSlide();
            }
        }

        if (!coll.onWall || coll.onGround)
            wallSlide = false;

        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            anim.SetTrigger("jump");
            jumpTimer = 0.2f;

            //Jump
            if (groundTouch && jumpTimer > 0)
                Jump(Vector2.up, false);
                jumpTimer = 0;
                groundedTimer = 0;
            
            if (coll.onWall && !coll.onGround)
                WallJump();
        }

        if (Keyboard.current.shiftKey.wasPressedThisFrame && !hasDashed)
        {
            if(xRaw != 0 || yRaw != 0)
                Dash(xRaw, yRaw);
        }

        if (coll.onGround && !groundTouch)
        {
            GroundTouch();
            groundTouch = true;
            anim.SetTrigger("land");
        }

        if(!coll.onGround && groundTouch)
        {
            groundTouch = false;
        }

        WallParticle(y);

        if (wallGrab || wallSlide || !canMove)
            return;

        if(x > 0)
        {
            side = 1;
            anim.Flip(side);
            dustParticle.Play();
        }
        if (x < 0)
        {
            side = -1;
            anim.Flip(side);
            dustParticle.Play();
        }


    }

    /// <summary>
    /// Plays the jumpParticle if able to jump
    /// </summary>
    void GroundTouch()
    {
        hasDashed = false;
        isDashing = false;

        side = anim.sr.flipX ? -1 : 1;

        jumpParticle.Play();
    }

    /// <summary>
    /// Plays the RippleEffect and Dash animation,
    /// then calls coroutine until the next dash is ready. 
    /// </summary>
    private void Dash(float x, float y)
    {
        Camera.main.transform.DOComplete();
        Camera.main.transform.DOShakePosition(.2f, .5f, 14, 90, false, true);
        FindObjectOfType<RippleEffect>().Emit(Camera.main.WorldToViewportPoint(transform.position));

        hasDashed = true;

        anim.SetTrigger("dash");

        rb.velocity = Vector2.zero;
        Vector2 dir = new Vector2(x, y);

        rb.velocity += dir.normalized * dashSpeed;
        StartCoroutine(DashWait());
    }

    /// <summary>
    /// Flips the player sprite then starts a coroutine
    /// that disables movement for a brief moment until
    /// the player can jump again.
    /// </summary>
    private void WallJump()
    {
        if ((side == 1 && coll.onRightWall) || side == -1 && !coll.onRightWall)
        {
            side *= -1;
            anim.Flip(side);
        }

        StopCoroutine(DisableMovement(0));
        StartCoroutine(DisableMovement(.1f));

        Vector2 wallDir = coll.onRightWall ? Vector2.left : Vector2.right;

        Jump((Vector2.up / 1.5f + wallDir / 1.5f), true);

        wallJumped = true;
    }

    /// <summary>
    /// Flips the player sprite based on the side the wall is on,
    /// then returns if the player cannot move.
    /// If the players moving horizontally then the player is 
    /// pushing a wall
    /// </summary>
    private void WallSlide()
    {
        if (coll.wallSide != side)
            anim.Flip(side * -1);

        if (!canMove)
            return;

        bool pushingWall = false;
        if ((rb.velocity.x > 0 && coll.onRightWall) || (rb.velocity.x < 0 && coll.onLeftWall))
        {
            pushingWall = true;
        }
        float push = pushingWall ? 0 : rb.velocity.x;

        rb.velocity = new Vector2(push, -slideSpeed);
    }

    /// <summary>
    /// This function checks if the player is able to move
    /// and sets the rigidbody velocity.
    /// </summary>
    private void Walk(Vector2 dir)
    {
        if (!canMove)
            return;

        if (wallGrab)
            return;

        if (!wallJumped)
        {
            rb.velocity = new Vector2(dir.x * speed, rb.velocity.y);
            //dustParticle.Play();
        }
        else
        {
            rb.velocity = Vector2.Lerp(rb.velocity, (new Vector2(dir.x * speed, rb.velocity.y)), wallJumpLerp * Time.deltaTime);
            //dustParticle.Play();
        }
    }

    /// <summary>
    /// This function modifies the players verticle velocity.
    /// Then plays the jump particle effect.
    /// </summary>
    private void Jump(Vector2 dir, bool wall)
    {
        slideParticle.transform.parent.localScale = new Vector3(ParticleSide(), 1, 1);
        ParticleSystem particle = wall ? wallJumpParticle : jumpParticle;

        rb.velocity = new Vector2(rb.velocity.x, 0);
        rb.velocity += dir * jumpForce;

        particle.Play();
    }

    /// <summary>
    /// Gets the coefficient of the rigidbodys drag
    /// </summary>
    void RigidbodyDrag(float x)
    {
        rb.drag = x;
    }

    /// <summary>
    /// Plays the slide particle if the player is on a wall
    /// and sliding.
    /// </summary>
    void WallParticle(float vertical)
    {
        var main = slideParticle.main;

        if (wallSlide || (wallGrab && vertical < 0))
        {
            slideParticle.transform.parent.localScale = new Vector3(ParticleSide(), 1, 1);
            main.startColor = Color.white;
        }
        else
        {
            main.startColor = Color.clear;
        }
    }

    /// <summary>
    /// Returns which side the particle should be on.
    /// </summary>
    int ParticleSide()
    {
        int particleSide = coll.onRightWall ? 1 : -1;
        return particleSide;
    }
    #endregion

    #region public functions
    /// <summary>
    /// Sets a timer for how long the player has touched
    /// the ground and returns that variable.
    /// </summary
    public bool groundTouch
    {
        get
        {
            return groundedTimer > 0;
        }
        set
        {
            groundedTimer = value ? 10f : groundedTimer;
        }
    }
    #endregion

    #region coroutines

    /// <summary>
    /// This coroutine starts a timer after the player has dashed.
    /// During this time the players gravity is modified.
    /// </summary
    IEnumerator DashWait()
    {
        FindObjectOfType<GhostTrail>().ShowGhost();
        StartCoroutine(GroundDash());
        DOVirtual.Float(14, 0, .8f, RigidbodyDrag);

        dashParticle.Play();
        rb.gravityScale = 0;
        GetComponent<BetterJumping>().enabled = false;
        wallJumped = true;
        isDashing = true;

        yield return new WaitForSeconds(.3f);

        dashParticle.Stop();
        rb.gravityScale = 3;
        GetComponent<BetterJumping>().enabled = true;
        wallJumped = false;
        isDashing = false;
    }

    /// <summary>
    /// Returns a float for a waiting time until
    /// the player can ground dash again.
    /// </summary
    IEnumerator GroundDash()
    {
        yield return new WaitForSeconds(.15f);
        if (coll.onGround)
            hasDashed = false;
    }


    /// <summary>
    /// Disables player movement.
    /// </summary
    IEnumerator DisableMovement(float time)
    {
        canMove = false;
        yield return new WaitForSeconds(time);
        canMove = true;
    }

    #endregion

}
