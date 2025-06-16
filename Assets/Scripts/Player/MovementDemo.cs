using System.Collections;
using System.Collections.Generic;
using UnityEngine;

enum LockDirection
{
    X = 0,
    Y = 1,
}

public class MovementDemo : MonoBehaviour
{
    private const float ForceSpeedLimit = 5.0f;
    private const float CORNER_GRAB = -9000f;

    [SerializeField]
    private bool canPull = false;
    [SerializeField]
    private float MovementSpeed = 1.0f;
    [SerializeField]
    private float Acceleration = 0.2f;
    [SerializeField]
    private float TurnSpeed = 0.1f;
    [SerializeField]
    private float PlayerGravity = 3.0f;
    [SerializeField]
    private Transform MainCamera;
    private float speed;

    private int rotateDirection;
    private float currentSpeed = 0;
    private float smoothVelocity = 0;

    // Lock movement to one axis during push/pull
    private Vector3 lockedDirection;
    private Vector3 lastMoveVector;
    private Vector3 ForcedMovement = Vector3.zero;

    private bool snapRotation = false;
    private Quaternion targetRotation;
    private Vector3 targetPosition;
    private LockDirection facing = LockDirection.Y;

    private CharacterController control;
    private Animator anim;
    private bool isPushing;
    Transform pushedBlock;

    // Start is called before the first frame update
    void Start()
    {
        control = GetComponent<CharacterController>();
        anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        CheckControls();
        AnimatePlayer();
    }

    void CheckControls()
    {
        // Check input from the left joystick thumbstick or keyboard
        bool rotateDir;
        Vector2 playerInput;
        Quaternion lookRotation;
        playerInput = new Vector2(Input.GetAxisRaw("LeftHoriz"), Input.GetAxisRaw("LeftVert"));

        Vector3 forward = MainCamera.forward;
        Vector3 right = MainCamera.right;

        bool completedRotation = false;
        bool completedPositioning = false;

        forward.y = 0;
        right.y = 0;

        Vector3 moveDirection = ((forward * playerInput.y) + (right * playerInput.x)).normalized;

        if (isPushing)
        {
            if (facing == LockDirection.X) moveDirection.z = 0;
            if (facing == LockDirection.Y) moveDirection.x = 0;
        }

        Vector3 gravity = Vector3.zero;

            if (!control.isGrounded)
        {
            gravity.y -= PlayerGravity;
        }

        if (!snapRotation)
        {
            if (moveDirection != Vector3.zero)
            {
                lookRotation = Quaternion.LookRotation(moveDirection);
                if (lookRotation != transform.rotation)
                {
                    rotateDir = GetRotateDirection(transform.rotation, lookRotation);
                    if (rotateDir) { rotateDirection = 2; } else { rotateDirection = 1; }
                }
                else
                {
                    rotateDirection = 0;
                }

                if (!isPushing)
                {
                    transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, TurnSpeed);
                }
            }

            float targetSpeed = MovementSpeed * playerInput.magnitude;
            currentSpeed = Mathf.SmoothDamp(currentSpeed, targetSpeed, ref smoothVelocity, Acceleration);

            if (isPushing)
            {
                if (CheckBlockCollision())
                {
                    isPushing = false;
                    pushedBlock.gameObject.GetComponent<DemoBoxMove>().Align();
                    pushedBlock = null;
                }
            }
            control.Move((moveDirection * currentSpeed) * Time.deltaTime);  // Move according to input
            control.Move(gravity * Time.deltaTime);                         // Apply gravity
            control.Move(ForcedMovement * Time.deltaTime);                  // Forced movements include conveyors etc

            lastMoveVector = (moveDirection * currentSpeed * Time.deltaTime);

            // After using any forced movement, return it to zero to prevent "stacking"
            ForcedMovement = Vector3.zero;

            if (Input.GetKeyDown(KeyCode.Joystick1Button2) || Input.GetKeyDown(KeyCode.LeftControl))
            {
                Debug.Log("Grab button pressed!");
                if (isPushing)
                {
                    isPushing = false;
                    pushedBlock.gameObject.GetComponent<DemoBoxMove>().Align();
                    pushedBlock = null;
                }
                else
                {
                    AttemptGrab();
                }
            }
        }

    }

    IEnumerator SnapRotation(Vector3 desiredPosition, Quaternion desiredDirection)
    {
        snapRotation = true;

        while(transform.rotation != desiredDirection)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, TurnSpeed);
            if (Quaternion.Angle(transform.rotation, targetRotation) < 5) transform.rotation = targetRotation;
            yield return null;
        }
        
        while(transform.position != desiredPosition)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, TurnSpeed);
            if ((transform.position - targetPosition).magnitude < 0.05f) transform.position = targetPosition; 
            yield return null;
        }

        if (pushedBlock != null)
        {
            pushedBlock.gameObject.GetComponent<DemoBoxMove>().Grabbed(transform);
            isPushing = true;
        }

        snapRotation = false;
    }

    void AttemptGrab()
    {
        Debug.Log("Attempting grab!");
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, 0.7f, ~(int)PhysicsLayer.Player))
        {
            if (hit.collider.tag == "Pushables")
            {
                pushedBlock = hit.collider.gameObject.transform;
                targetPosition = GetClosestPoint(GameManager.VectorToGrid(pushedBlock.position), hit.collider.bounds.size.x);
                if (targetPosition.y != CORNER_GRAB)
                {
                    // snapRotation = true;
                    targetRotation = ForwardSnapRotation();
                    StartCoroutine(SnapRotation(targetPosition, targetRotation));
                    Debug.Log("Rotating to target: " + targetRotation.eulerAngles);
                    Debug.Log("Current Rotation: " + transform.rotation.eulerAngles);
                }
                else
                {
                    targetPosition = transform.position;
                    pushedBlock = null;
                    Debug.Log("Cannot grab from corner!");
                }
            }
        }
        else
        {
            Debug.Log("No grab hit detected!");
        }
    }

    Quaternion ForwardSnapRotation()
    {
        Vector3 v = transform.forward;

        if (Mathf.Abs(v.x) < Mathf.Abs(v.y))
        {
            v.x = 0;
            if (Mathf.Abs(v.y) < Mathf.Abs(v.z))
                v.y = 0;
            else
                v.z = 0;
        }
        else
        {
            v.y = 0;
            if (Mathf.Abs(v.x) < Mathf.Abs(v.z))
                v.x = 0;
            else
                v.z = 0;
        }

        return (Quaternion.LookRotation(v, transform.up));
    }

    Vector3 GetClosestPoint(Vector3 b, float size)
    {
        Vector3 v = GameManager.VectorToGrid(transform.position);
        Vector3 newVector = transform.position;
        Debug.Log("Rounded Position: " + v);
        Debug.Log("Block Vector: " + b);
        if ((v.x != b.x) && (v.z != b.z)) newVector = new Vector3(0, CORNER_GRAB, 0);
        if ((v.x == b.x) && (v.z == b.z)) newVector = new Vector3(0, CORNER_GRAB, 0);

        if (v.x != b.x) { newVector = new Vector3(v.x, transform.position.y, b.z); facing = LockDirection.X; }
        if (v.z != b.z) { newVector = new Vector3(b.x, transform.position.y, v.z); facing = LockDirection.Y; }

        return newVector;
    }

    bool IsBetween(float f, float min, float max)
    {
        if (f > -min && f <= max) return true;
        return false;
    }

    void AnimatePlayer()
    {
        float speedValue = Mathf.Clamp(currentSpeed / MovementSpeed, 0, 1);

        switch(rotateDirection)
        {
            case 1:     // clockwise
                anim.SetFloat("Direction", 1.0f);
                break;
            case 2:     // anti-clockwise
                anim.SetFloat("Direction", -1.0f);
                break;
            default:    // default to forward
                anim.SetFloat("Direction", 0.0f);
                break;
        }

        if (currentSpeed < 0.1f)
        {
            anim.SetFloat("Speed", 0);
        }
        else
        {
            anim.SetFloat("Speed", currentSpeed);
        }
    }

    public void AddForcedMovement(Vector3 additiveVector, bool inverse = false)
    {
        if (inverse) { ForcedMovement -= additiveVector; }
        else { ForcedMovement += additiveVector; }

        if (ForcedMovement.magnitude > ForceSpeedLimit)
        {
            ForcedMovement = (ForcedMovement.normalized * ForceSpeedLimit);
        }
    }


    // True if rotating clockwise
    bool GetRotateDirection(Quaternion from, Quaternion to)
    {
        float fromY = from.eulerAngles.y;
        float toY = to.eulerAngles.y;
        float clockWise = 0f;
        float counterClockWise = 0f;

        if (fromY <= toY)
        {
            clockWise = toY - fromY;
            counterClockWise = fromY + (360 - toY);
        }
        else
        {
            clockWise = (360 - fromY) + toY;
            counterClockWise = fromY - toY;
        }
        return (clockWise <= counterClockWise);
    }

    bool CheckBlockCollision()
    {
        // RaycastHit hit;

        // Debug.Log("Distance to check: " + (pushedBlock.position + control.velocity.normalized));
        Debug.Log("Normalized velocity: " + lastMoveVector.normalized);

        if (Physics.Linecast(pushedBlock.position, pushedBlock.position + lastMoveVector.normalized))
        {
            Debug.Log("Hit Detected!");
            return true;
        }
        return false;
    }

    private void OnDrawGizmos()
    {
        if (pushedBlock != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(pushedBlock.position, 0.1f);
            Gizmos.DrawLine(pushedBlock.position, pushedBlock.position + lastMoveVector.normalized);
        }
    }
}
