using System.Collections;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using System.Linq;
using UnityEngine;

public class RopeSystem : MonoBehaviour
{
    
    [SerializeField] private float maxRopeLength = 20f;
    [SerializeField] private float pullForce = 5f;

    public GameObject ropeAnchorPoint;
    public DistanceJoint2D ropeJoint;
    public CharacterController characterController;
    public SpriteRenderer ropeAnchorSprite;
    public LineRenderer ropeRenderer;

    private Vector2 playerPosition;
    private Rigidbody2D ropeAnchorRB;
    private Rigidbody2D playerRB;
    private List<Vector2> ropePositions = new List<Vector2>();

    private int layerMask = 1 << 8;                                 // Layer 8 is solely occupied by the player
    private bool ropeAttached = false;
    private bool distanceSet;

    private void Awake()
    {
        ropeJoint.enabled = false;
        playerPosition = transform.position;
        playerRB = transform.GetComponent<Rigidbody2D>();
        ropeAnchorRB = ropeAnchorPoint.GetComponent<Rigidbody2D>();
        ropeAnchorSprite = ropeAnchorSprite.GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        playerPosition = transform.position;

        HandleInput();

        if (ropeAttached)
        {

        } else
        {

        }

        UpdateRopePositions();
    }

    private void HandleInput()
    {
        if (Input.GetButtonDown("Fire2"))
        {
            if (ropeAttached)
                return;

            ropeRenderer.enabled = true;

            int ropeHorizDir = characterController.m_FacingRight ? 1 : -1;

            Vector2 dirRope;

            if (Input.GetAxisRaw("Vertical") > 0)
            {
                dirRope = new Vector2(ropeHorizDir, 1);
            } else if (Input.GetAxisRaw("Vertical") < 0)
            {
                dirRope = new Vector2(ropeHorizDir, -1);
            } else
            {
                dirRope = new Vector2(ropeHorizDir, 0);
            }

            // ~Layermask allows the RayCast to interact with everything except the player layer
            RaycastHit2D hit = Physics2D.Raycast(playerPosition, dirRope, maxRopeLength, ~layerMask);

            if (hit.collider != null)
            {
                Collider2D hitCollider = hit.collider;
                Rigidbody2D hitColliderRB = hitCollider.GetComponent<Rigidbody2D>();

                switch (hitCollider.tag)
                {
                    case "Enemy":

                        if (!ropePositions.Contains(hit.point))
                        {
                            Vector2 dirToPlayer = (playerPosition - hitColliderRB.position).normalized;
                            hitColliderRB.AddForce(dirToPlayer * pullForce, ForceMode2D.Impulse);
                            playerRB.AddForce(-dirToPlayer * pullForce * 2.0f, ForceMode2D.Impulse);

                            ropePositions.Add(hit.point);
                            ropeJoint.distance = Vector2.Distance(playerPosition, hit.point);

                            ropeAttached = true;
                            ropeAnchorSprite.enabled = true;
                        }

                        Debug.Log("Pull!");

                        break;

                    case "Hinge":

                        Debug.Log("Hit!");

                        if (!ropePositions.Contains(hit.point))
                        {
                            playerRB.AddForce(Vector2.up * pullForce, ForceMode2D.Impulse);

                            ropeAttached = true;
                            ropePositions.Add(hit.point);
                            ropeJoint.distance = Vector2.Distance(playerPosition, hit.point);
                            ropeJoint.enabled = true;
                            ropeAnchorSprite.enabled = true;
                        }
                        break;
                    default:
                        break;
                }
            } else
            {
                ropeRenderer.enabled = false;
                ropeAttached = false;
                ropeJoint.enabled = false;
            }
        }

        if (Input.GetButtonUp("Fire2"))
        {
            ResetRope();
        }
    }

    private void ResetRope()
    {
        ropeJoint.enabled = false;
        ropeAttached = false;
        ropeRenderer.positionCount = 2;
        ropeRenderer.SetPosition(0, transform.position);
        ropeRenderer.SetPosition(1, transform.position);
        ropePositions.Clear();
        ropeAnchorSprite.enabled = false;
    }

    private void UpdateRopePositions()
    {
        if (!ropeAttached)
            return;

        ropeRenderer.positionCount = ropePositions.Count + 1;

        for (int i = ropeRenderer.positionCount - 1; i >= 0; i--)
        {
            if (i != ropeRenderer.positionCount - 1)
            {
                ropeRenderer.SetPosition(i, ropePositions[i]);

                if (i == ropePositions.Count - 1 || ropePositions.Count == 1)
                {
                    Vector2 ropePosition = ropePositions[ropePositions.Count - 1];

                    if (ropePositions.Count == 1)
                    {
                        ropeAnchorRB.transform.position = ropePosition;

                        if (!distanceSet)
                        {
                            ropeJoint.distance = Vector2.Distance(transform.position, ropePosition);
                            distanceSet = true;
                        }
                    }
                    else
                    {
                        ropeAnchorRB.transform.position = ropePosition;

                        if (!distanceSet)
                        {
                            ropeJoint.distance = Vector2.Distance(transform.position, ropePosition);
                            distanceSet = true;
                        }
                    }
                }
                else if (i - 1 == ropePositions.IndexOf(ropePositions.Last()))
                {
                    Vector2 ropePosition = ropePositions.Last();
                    ropeAnchorRB.transform.position = ropePosition;

                    if (!distanceSet)
                    {
                        ropeJoint.distance = Vector2.Distance(transform.position, ropePosition);
                        distanceSet = true;
                    }
                }
            } else
            {
                ropeRenderer.SetPosition(1, transform.position);
            }
        }
    }
}
