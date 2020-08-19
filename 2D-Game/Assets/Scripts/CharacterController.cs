﻿using System;
using System.Collections;
using System.Data.OleDb;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

public class CharacterController : MonoBehaviour
{

    [SerializeField] private float m_Jumpforce = 5f;                             //Amount of force added when a player jumps
    [Range(0, 1f)] [SerializeField] private float m_maxJumpTime = 0.3f;          //Amount of time a player can jump for 
    [Range(0, 1f)] [SerializeField] private float m_maxClingTime = 1f;           //Amount of time a player can cling to a wall
    [Range(0, 1f)] [SerializeField] private float m_CrouchSpeed = 0.36f;         //Amount of maxSpeed applied to crouch movment. 1 = 100%
    [Range(0, 100f)] [SerializeField] private float m_DashSpeed = 50f;           //Speed when dashing
    [Range(0, 10f)] [SerializeField] private float m_fallSpeed = 1f;             //Speed when falling
    [Range(0, 0.3f)] [SerializeField] private float m_MovementSmoothing = 0.05f; //How much to smooth out movement
    [SerializeField] private float m_dashRate = 2f;                              //Number of times a player can dash per second
    [SerializeField] private float m_dashDelay = 0.3f;                           //Length of dash animation

    [SerializeField] private bool m_AirControl = false;                          //Whether or not a player can steer while jumping
    [SerializeField] private LayerMask m_WhatIsGround;                           //A mask determining what is ground to the character
    [SerializeField] private LayerMask m_WhatIsWall;                             //A mask determining what is wall to the character
    [SerializeField] private Transform m_GroundCheck;                            //A position marking where to check if the player is grounded
    [SerializeField] private Transform m_CeilingCheck;                           //A position marking where to check for ceilings
    [SerializeField] private Transform m_WallCheck;                              //A position marking where to check for walls
    [SerializeField] private Collider2D m_CrouchDisableCollider;                 //A collider that will be disabled when crouching

    private float m_nextDashTime = 0f;                                           //Next time that a player can dash
    private float m_JumpTimer = 0f;                                              //Amount of time the jump has lasted  
    private float m_ClingTimer = 0f;
    const float k_GroundedRadius = 0.2f;                                         //Radius of the overlap circle to determine if grounded
    const float k_CeilingRadius = 0.2f;                                          //Radius of the overlap circle to determine if the player can stand
    const float k_WalledRadius = 0.2f;                                           //Radius of the overlap circle to determine if walled
    private bool m_Grounded;                                                     //Whether or not the player is on the ground
    private bool m_isJumping;                                                    //Whether or not the player is currently jumping
    private bool m_canJump;                                                      //Whether or not the player can jump
    private bool m_canDash;                                                      //Whether or not the player can dash
    private bool m_Walled;                                                       //Whether or not the player is against a wall
    private int m_dashDirection;                                                 //Multiplier to change dash direction

    public bool m_FacingRight = true;                                           //For determining which way the player is currently facing

    private Rigidbody2D m_Rigidbody2D;
    private Vector3 m_Velocity = Vector3.zero;

    [Header("Events")]
    [Space]

    public UnityEvent OnLandEvent;
    public UnityEvent OnClingEvent;

    [System.Serializable]
    public class BoolEvent : UnityEvent<bool>{ }

    public BoolEvent OnCrouchEvent;
    private bool m_wasCrouching = false;

    private void Awake()
    {
        m_Rigidbody2D = GetComponent<Rigidbody2D>();

        if (OnLandEvent == null)
            OnLandEvent = new UnityEvent();

        if (OnCrouchEvent == null)
            OnCrouchEvent = new BoolEvent();

        if (OnClingEvent == null)
            OnClingEvent = new UnityEvent();
    }

    private void FixedUpdate()
    {
        bool wasGrounded = m_Grounded;
        m_Grounded = false;

        //The player is grounded if a circle cast to the groundcheck position hits anything designated as ground
        //This can be done using layers instead but Sample Assets will not overwrite you projects settings.
        Collider2D[] groundColliders = Physics2D.OverlapCircleAll(m_GroundCheck.position, k_GroundedRadius, m_WhatIsGround);
        for (int i = 0; i < groundColliders.Length; i++)
        {
            if (groundColliders[i].gameObject != gameObject)
            {
                m_Grounded = true;
                m_canJump = true;
                m_canDash = true;

                if (!wasGrounded)
                    OnLandEvent.Invoke();
            }
        }

        bool wasWalled = m_Walled;
        m_Walled = false;

        //The player is walled if a circle cast to the wallcheck position hits anything designated as wall
        Collider2D[] wallColliders = Physics2D.OverlapCircleAll(m_WallCheck.position, k_WalledRadius, m_WhatIsWall);
        for (int i = 0; i < wallColliders.Length; i++)
        {
            if (wallColliders[i].gameObject != gameObject)
            {
                m_Walled = true;
                m_canJump = true;
                m_canDash = true;

                if (!wasWalled)
                    OnClingEvent.Invoke();
            }
        }
    }

    public void Move(float move, bool crouch, bool jump, bool dash)
    {
        //If crouching, check to see if the character can stand up
        if (!crouch)
        {
            //If the character has a ceiling preventing them from standing up, keep them crouching
            if (Physics2D.OverlapCircle(m_CeilingCheck.position, k_CeilingRadius, m_WhatIsGround))
            {
                crouch = true;
            }
        }

        //Only control the player if grounded or AirControl is turned on
        if (m_Grounded || m_AirControl)
        {
            //If crouching
            if (crouch)
            {
                if (!m_wasCrouching)
                {
                    m_wasCrouching = true;
                    OnCrouchEvent.Invoke(true);
                }

                //Reduce the speed by the crouchSpeed multiplier
                move *= m_CrouchSpeed;

                //Disable one of the colliders when crouching
                if (m_CrouchDisableCollider != null)
                    m_CrouchDisableCollider.enabled = false;
            }
            else
            {
                //Enable the collider when not crouching
                if (m_CrouchDisableCollider != null)
                    m_CrouchDisableCollider.enabled = true;

                if (m_wasCrouching)
                {
                    m_wasCrouching = false;
                    OnCrouchEvent.Invoke(false);
                }
            }

            //Move the character by finding the target velocity
            Vector3 targetVelocity = new Vector2(move * 10f, m_Rigidbody2D.velocity.y);
            //And then smoothing it out and applying it to the character
            m_Rigidbody2D.velocity = Vector3.SmoothDamp(m_Rigidbody2D.velocity, targetVelocity, ref m_Velocity, m_MovementSmoothing);

            //If the input is moving the player right and the player is facing left...
            if (move > 0 && !m_FacingRight)
            {
                //... flip the player
                Flip();
            }
            //Otherwise if the input is moving the player left and the player is facing right...
            else if (move < 0 && m_FacingRight)
            {
                //... flip the player.
                Flip();
            }
        }

        if ((m_Walled || m_Grounded) && m_canJump && jump)
        {
            m_canJump = false;
            m_Grounded = false;
            m_isJumping = true;
            m_JumpTimer = 0f;
            m_ClingTimer = 0f;
            m_Rigidbody2D.velocity = Vector2.up * m_Jumpforce;
        }

        if (Input.GetButton("Jump") && m_isJumping)
        {
            if (m_JumpTimer <= m_maxJumpTime)
            {
                m_Rigidbody2D.velocity = Vector2.up * m_Jumpforce;
                m_JumpTimer += Time.deltaTime;
            } else
            {
                m_isJumping = false;
            }
        }

        if (m_isJumping && Input.GetButtonUp("Jump"))
        {
            m_isJumping = false;
        }

        if (m_Walled && !m_Grounded)
        {
            m_canJump = true;

            if ((m_FacingRight && Math.Sign(move) > 0) || (!m_FacingRight && Math.Sign(move) < 0))
            {
                m_Rigidbody2D.velocity = Vector2.zero;
                m_ClingTimer += Time.deltaTime;
            }

            if (m_ClingTimer >= m_maxClingTime)
            {
                m_Rigidbody2D.velocity = Vector2.down * m_fallSpeed;
            }
        }

        if (dash && m_canDash)
        {
            m_dashDirection = m_FacingRight ? 1 : -1;

            StartCoroutine(Dash(m_dashDirection));
        }
    }

    private void Flip()
    {
        //Switch the way the player is labelled as facing.
        m_FacingRight = !m_FacingRight;

        transform.Rotate(0f, 180f, 0f);
    }

    IEnumerator Dash(int m_dashDirection)
    {
        if (Time.time >= m_nextDashTime)
        {
            Vector2 velocity;
            
            if (Input.GetButton("Vertical"))
            {
                float verticalDir = Math.Sign(Input.GetAxisRaw("Vertical"));

                velocity = new Vector2(m_dashDirection * m_DashSpeed * 0.2f, verticalDir * m_DashSpeed * 0.2f);
            } else
            {
                velocity = new Vector2(m_dashDirection * m_DashSpeed, 0f);
            }

            m_Rigidbody2D.velocity = velocity;

            m_nextDashTime = Time.time + 1f / m_dashRate;

            yield return new WaitForSeconds(0.3f);

            m_Rigidbody2D.velocity = Vector2.zero;
            m_canDash = false;
        }
    }

    private void OnDrawGizmos()
    {
        if (m_GroundCheck == null)
            return;

        Gizmos.DrawWireSphere(m_GroundCheck.position, k_GroundedRadius);
        Gizmos.DrawWireSphere(m_CeilingCheck.position, k_CeilingRadius);
        Gizmos.DrawWireSphere(m_WallCheck.position, k_WalledRadius);
    }
}
