using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Pathfinding;

public class EnemyController : MonoBehaviour
{
    [SerializeField] private float speed = 200f;
    [SerializeField] private float nextWayPointDistance = 0.5f;
    [SerializeField] private float sightRange = 50f;
    [SerializeField] private float jumpForce = 8f;
    [SerializeField] private float maxJumpTime = 0.3f;
    [SerializeField] private float movementSmoothing = 0.05f;

    [SerializeField] private LayerMask whatIsGround;

    public Transform[] defaultPathNodes;
    public Transform player;
    public Transform enemyGFX;
    public Transform groundCheck;

    readonly int layerMask = 1 << 10;

    private int defaultPathPoint = 0;
    private bool seesPlayer = false;
    private bool grounded = true;
    private float jumpTimer = 0f;

    private const float groundedRadius = 0.2f;

    private static Vector2 zeroVelocity = Vector2.zero;

    Path path;
    Seeker seeker;
    Rigidbody2D rb;
    Vector2 velocity = Vector2.zero;

    int currentWayPoint = 0;

    // Start is called before the first frame update
    void Start()
    {
        seeker = GetComponent<Seeker>();
        rb = GetComponent<Rigidbody2D>();

        InvokeRepeating(nameof(UpdatePath), 0f, 1f);
    }

    void FixedUpdate()
    {
        if (path == null)
            return;

        Collider2D[] groundColliders = Physics2D.OverlapCircleAll(groundCheck.position, groundedRadius, whatIsGround);
        for (int i = 0; i < groundColliders.Length; i++)
        {
            if (groundColliders[i].gameObject != gameObject)
            {
                grounded = true;
                jumpTimer = 0f;
            } else
            {
                grounded = false;
            }
        }

        LookForPlayer();

        if (currentWayPoint >= path.vectorPath.Count)
        {
            OnTargetReached();
            return;
        }

        Vector2 direction = ((Vector2) path.vectorPath[currentWayPoint] - rb.position).normalized;

        if (direction.y > 0.5f && grounded)
        {
            jumpTimer += Time.fixedDeltaTime;

            if (jumpTimer <= maxJumpTime)
            {
                velocity = Vector2.up * jumpForce;
            }

        } else
        {
            Vector2 targetVelocity = new Vector2(direction.x * speed * Time.fixedDeltaTime, rb.velocity.y);
            velocity = Vector2.SmoothDamp(rb.velocity, targetVelocity, ref zeroVelocity, movementSmoothing);
        }

        rb.velocity = velocity;

        float distance = Vector2.Distance(rb.position, path.vectorPath[currentWayPoint]);

        if (distance < nextWayPointDistance)
            currentWayPoint++;

        if (velocity.x >= 0.01f)
        {
            enemyGFX.localScale = new Vector3(-1f, 1f, 1f);
        } else if (velocity.x <= -0.01f)
        {
            enemyGFX.localScale = new Vector3(1f, 1f, 1f);
        }
    }

    void LookForPlayer()
    {
        Vector2 dirToPlayer = ((Vector2)player.position - rb.position).normalized;

        RaycastHit2D hit = Physics2D.Raycast(rb.position, dirToPlayer, sightRange, ~layerMask);

        if (hit.collider != null && hit.collider.CompareTag("Player"))
        {
            seesPlayer = true;
        }
        else
        {
            seesPlayer = false;
        }

        Debug.DrawRay(rb.position, dirToPlayer * sightRange, Color.white);
    }

    void UpdatePath()
    {
        Transform target;

        if (seesPlayer)
        {
            target = player;
        }
        else
        {
            target = defaultPathNodes[defaultPathPoint];
        }

        if (seeker.IsDone())
            seeker.StartPath(rb.position, target.position, OnPathComplete);
    }

    void OnPathComplete(Path p)
    {
        if (!p.error)
        {
            path = p;
            currentWayPoint = 0;
        }
    } 

    public virtual void OnTargetReached()
    {
        if (!seesPlayer)
        {
            defaultPathPoint++;

            if (defaultPathPoint >= defaultPathNodes.Length)
            {
                defaultPathPoint = 0;
            }
        } //TODO: Logic for if near player

        UpdatePath();
    }
}
