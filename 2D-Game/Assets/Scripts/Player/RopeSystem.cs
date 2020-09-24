using System.Collections;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using System.Linq;
using UnityEngine;

public class RopeSystem : MonoBehaviour
{
    
    [SerializeField] private float maxRopeLength = 10f;
    [SerializeField] private float climbSpeed = 3f;

    public GameObject ropeAnchorPoint;
    public DistanceJoint2D ropeJoint;
    public CharacterController characterController;
    public SpriteRenderer ropeAnchorSprite;
    public LineRenderer ropeRenderer;

    private Vector2 playerPosition;
    private Rigidbody2D ropeAnchorRB;
    private Rigidbody2D playerRB;
    private List<Vector2> ropePositions = new List<Vector2>();
    private RaycastHit2D hit;

    private int layerMask = 1 << 8;                                 // Layer 8 is solely occupied by the player
    private bool ropeTethered = false;
    private bool ropeAttached = false;
    private bool distanceSet;
    private bool isColliding;

    private void Awake()
    {
        ropeJoint.enabled = false;
        playerPosition = transform.position;
        playerRB = transform.GetComponent<Rigidbody2D>();
        ropeAnchorRB = ropeAnchorPoint.GetComponent<Rigidbody2D>();
        ropeAnchorSprite = ropeAnchorSprite.GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        playerPosition = transform.position;

        HandleInput();

        UpdateRopePositions();

        HandleRopeLength();
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        isColliding = true;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        isColliding = false;
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
            hit = Physics2D.Raycast(playerPosition, dirRope, maxRopeLength, ~layerMask);

            if (hit.collider != null)
            {
                Collider2D hitCollider = hit.collider;

                switch (hitCollider.tag)
                {
                    case "Enemy":

                        if (!ropePositions.Contains(hit.point))
                        {
                            ropePositions.Add(hit.point);
                            ropeAttached = true;
                            ropeTethered = true;
                            ropeAnchorSprite.enabled = true;
                            characterController.isTethered = true;
                            characterController.ropeHook = hit.point;
                        }

                        Debug.Log("Pull!");

                        break;

                    case "Hinge":

                        Debug.Log("Hit!");

                        if (!ropePositions.Contains(hit.point))
                        {
                            playerRB.AddForce(Vector2.up * 10f, ForceMode2D.Impulse);

                            ropeAttached = true;
                            ropePositions.Add(hit.point);
                            ropeJoint.distance = Vector2.Distance(playerPosition, hit.point);
                            ropeJoint.enabled = true;
                            ropeAnchorSprite.enabled = true;
                            characterController.isSwinging = true;
                            characterController.ropeHook = hit.point;
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



    public void ResetRope()
    {
        ropeJoint.enabled = false;
        ropeAttached = false;
        ropeTethered = false;
        ropeRenderer.positionCount = 2;
        ropeRenderer.SetPosition(0, transform.position);
        ropeRenderer.SetPosition(1, transform.position);
        ropePositions.Clear();
        ropeAnchorSprite.enabled = false;
        characterController.isSwinging = false;
        characterController.isTethered = false;
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

    private void HandleRopeLength()
    {
        if (Input.GetAxisRaw("Vertical") >= 1f && ropeAttached && !ropeTethered && !isColliding)
        {
            ropeJoint.distance -= Time.deltaTime * climbSpeed;
        } else if (Input.GetAxisRaw("Vertical") < 0f && ropeAttached && !ropeTethered && !isColliding)
        {
            ropeJoint.distance += Time.deltaTime * climbSpeed;
        }

        if (ropeJoint.distance > maxRopeLength)
        {
            ropeJoint.distance = maxRopeLength;
        }
    }
}
