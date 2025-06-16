using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {

    public int MaxPushWeight = 1;
    public int MaxPushDistance = 1;

    public float runSpeed = 2000.0f;
    public float MaxRunSpeed = 4f;
    public float turnSpeed = 15.0f;
    public float jumpSpeed = 512.0f;
    public float ConveyorSpeed = 4f;

    public GameObject playerPivot;

    public LevelFlags MovementTrigger;
    public LevelFlags TriggerEventOnJump;
    public LevelFlags TriggerEventOnFallout;
    public LevelFlags TriggerEventOnReset;

    private bool TouchingDown = false;
    private bool onGround = true;
    private bool Jumping = false;

    private Animator anim;
    private Rigidbody rbody;
    private CapsuleCollider coll;

    [SerializeField]
    private bool CanWalk;
    private Vector3 ForcedMovement;

    private LevelManager lm;

    // Use this for initialization
    void Start() {
        lm = GameObject.FindObjectOfType<LevelManager>();
        GameManager.StartPos = transform.position;
        GameManager.StartRot = transform.rotation;
        playerPivot.GetComponent<PlayerRotation>().SetInitialState(GameManager.StartRot);
        anim = GetComponent<Animator>();
        rbody = GetComponent<Rigidbody>();
        coll = GetComponent<CapsuleCollider>();

        if (MovementTrigger != LevelFlags.None)
        {
            CanWalk = GameManager.CheckFlag(MovementTrigger);
        }
        else
        {
            CanWalk = true;
        }

        ForcedMovement = Vector3.zero;

        if (MaxPushDistance < 1) MaxPushDistance = 1;
        if (MaxPushWeight < 1) MaxPushWeight = 1;
    }

    void FixedUpdate() {

        if (transform.position.y < GameManager.WorldBottom)
        {
            ResetPosition();
        }

        if (!anim) anim = GetComponent<Animator>();

//      if (Physics.CheckSphere(coll.transform.position - groundDistance, 0.1f, ~(int)PhysicsLayer.Player))
        if (CheckGrounded())
        {
            onGround = true;
            anim.SetBool("Grounded", true);
        }
        else
        {
            onGround = false;
            ForcedMovement = Vector3.zero;
            anim.SetBool("Grounded", false);
        }

        if(!CanWalk && !TouchingDown)
        {
            if(MovementTrigger != LevelFlags.None)
            {
                CanWalk = GameManager.CheckFlag(MovementTrigger);
            }
            else
            {
                if (!AnimationPlaying("Dead")) CanWalk = true;
            }
        }

        CheckControls();
    }

    bool CheckGrounded()
    {
        int collisionCount = 0;
//        Vector3 groundDistance = new Vector3(0, coll.bounds.extents.y, 0);
        Vector3 checkDistance = new Vector3(0, coll.bounds.extents.y, 0) + new Vector3(0, -0.1f, 0);
        Vector3 posA = new Vector3(transform.position.x - coll.bounds.extents.x, transform.position.y, transform.position.z);
        Vector3 posB = new Vector3(transform.position.x + coll.bounds.extents.x, transform.position.y, transform.position.z);
        Vector3 posC = new Vector3(transform.position.x, transform.position.y, transform.position.z - coll.bounds.extents.z);
        Vector3 posD = new Vector3(transform.position.x, transform.position.y, transform.position.z + coll.bounds.extents.z);


        if (Physics.CheckSphere(posA, 0.05f, ~(int)PhysicsLayer.Player)) collisionCount++;
        if (Physics.CheckSphere(posB, 0.05f, ~(int)PhysicsLayer.Player)) collisionCount++;
        if (Physics.CheckSphere(posC, 0.05f, ~(int)PhysicsLayer.Player)) collisionCount++;
        if (Physics.CheckSphere(posD, 0.05f, ~(int)PhysicsLayer.Player)) collisionCount++;

        Debug.Log("Collision Count: " + collisionCount);
        if (collisionCount > 0) return true;

        return false;

    }

    void CheckControls()
    {
        float Dhorizontal = 0;
        float Dvertical = 0;

        Dhorizontal = Input.GetAxis("LeftHoriz");
        Dvertical = Input.GetAxis("LeftVert");

        if (Input.GetKeyDown(KeyCode.Joystick1Button0))
        {
            if (onGround)
            {
                anim.SetBool("Jump", true);
            }
        }

        if (Input.GetKey(KeyCode.A))
        {
            Dhorizontal = -1.0f;
        }
        if (Input.GetKey(KeyCode.D))
        {
            Dhorizontal = 1.0f;
        }

        if (Input.GetKey(KeyCode.W))
        {
            Dvertical = 1.0f;
        }
        if (Input.GetKey(KeyCode.S))
        {
            Dvertical = -1.0f;
        }
        
        // SHIFT + B to toggle Debug mode!
        if (Input.GetKeyDown(KeyCode.B))
        {
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            {
                GameManager.DebugMode = !GameManager.DebugMode;
            }
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (onGround)
            {
                anim.SetBool("Jump", true);
                Jumping = true;
            }
        }
        if (Input.GetKeyDown(KeyCode.Joystick1Button3))
        {
            if (!isJumping())
            {
                anim.Play("Emote");
            }
        }

        if (GameManager.DebugMode)
        {
            Debug.Log("Player movement detected: " + Dhorizontal + ", " + Dvertical);
        }

        if (CanWalk)
        {
            anim.SetFloat("JoyY", Dvertical);
            anim.SetFloat("JoyX", Dhorizontal);
        }

        float Xmovement = Dhorizontal * runSpeed * Time.deltaTime;
        float Ymovement = Dvertical * runSpeed * Time.deltaTime;

        if (CanWalk && !AnimationPlaying("Emote"))
        {
            Vector3 xVector = GameManager.MainCamera.transform.right * Xmovement;
            Vector3 yVector = GameManager.MainCamera.transform.forward * Ymovement;
            xVector.y = 0;
            yVector.y = 0;

            Vector3 newVector = (xVector + yVector) + ForcedMovement;

            if (ForcedMovement != Vector3.zero) Debug.Log("New Move Vector: " + newVector);

            if (newVector.magnitude > MaxRunSpeed)
            {
                newVector = newVector.normalized * MaxRunSpeed;
            }

            newVector.y = rbody.velocity.y;

            rbody.velocity = newVector;

            transform.rotation = Quaternion.RotateTowards(transform.rotation, playerPivot.gameObject.transform.rotation, turnSpeed);
        }
    }

    void JumpUp()
    {
        anim.SetBool("Jump", false);
        rbody.AddForce(Vector3.up * jumpSpeed, ForceMode.VelocityChange);

        if(TriggerEventOnJump != LevelFlags.None)
        {
            GameManager.AddFlag(TriggerEventOnJump);
        }
    }

    void TouchDown()
    {
        rbody.velocity = ForcedMovement;
        TouchingDown = true;
        CanWalk = false;
        Jumping = false;
    }

    void StoodUp()
    {
        TouchingDown = true;
        if (MovementTrigger != LevelFlags.None)
        {
            CanWalk = GameManager.CheckFlag(MovementTrigger);
        }
        else
        {
            CanWalk = true;
        }
    }

    void OnCallChangeFace()
    {

    }

    bool isJumping()
    {
        if (AnimationPlaying("Start Jump"))
        {
            return true;
        }
        if (AnimationPlaying("Mid Jump"))
        {
            return true;
        }
        if (AnimationPlaying("Landing"))
        {
            return true;
        }
        return false;
    }

    bool AnimationPlaying(string animName)
    {
        return (anim.GetCurrentAnimatorStateInfo(0).IsName(animName));
    }

    public void ResetPosition()
    {
        transform.position = GameManager.StartPos;
        transform.rotation = GameManager.StartRot;
        CanWalk = true;
        rbody.velocity = Vector3.zero;
        anim.Play("Blend Tree");
    }

    private void OnTriggerEnter(Collider other)
    {
        // if (other.gameObject.tag != "PlayerDetector") return;

        GameObject obj = other.transform.root.gameObject;

        switch (obj.tag)
        {
            case "Teleporter":
                obj = other.transform.parent.gameObject;
                Transform destination = other.transform.parent.GetComponent<Teleporter>().destination;
                if (!destination) { Debug.LogError("No teleporter object found on " + obj); return; }
                if (!Physics.CheckBox(destination.position, new Vector3(0.49f, 0.49f, 0.49f)))
                {
                    transform.position = destination.position;
                }
                obj.GetComponent<Teleporter>().Trigger();
                break;
            case "Lava":
                Magma m = obj.transform.root.GetComponent<Magma>();
                if (!m) { Debug.LogError("No magma object found on " + obj); return; }
                if (!m.IsSolid)
                {
                    CanWalk = false;
                    anim.SetTrigger("Dead");
                }
                break;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.tag != "PlayerDetector") return;

        GameObject obj = other.gameObject.transform.parent.gameObject;

        switch (obj.tag)
        {
            case "Conveyor":
                Debug.Log("Adding conveyor movement");
                ForcedMovement = (other.gameObject.transform.forward * MaxRunSpeed);
                // if (onGround) rbody.AddForce((other.gameObject.transform.forward * ConveyorSpeed) * Time.deltaTime, ForceMode.Acceleration);

                // if(onGround && !transform.parent) transform.parent = Instantiate((GameObject)Resources.Load("MovementSpark"), transform.position, other.gameObject.transform.rotation).transform;\
                break;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag != "PlayerDetector") return;

        GameObject obj = other.gameObject.transform.parent.gameObject;

        switch (obj.tag)
        {
            case "Conveyor":
                Debug.Log("Resetting Forced Movement to zero in OnTriggerExit");
                ForcedMovement = Vector3.zero;
                break;
        }
    }

    private void DeathAnimEnd()
    {
        LevelManager lm = GameObject.FindObjectOfType<LevelManager>();
        anim.ResetTrigger("Dead");
        lm.RestartLevel();
    }

    public bool CanPush(GameObject obj)
    {
        RaycastHit ray;
        bool canPush = false;
        Vector3 midPoint = transform.position + (transform.up * 0.5f);
        LayerMask layer = LayerMask.GetMask("Pushable");

        // Use player pivot for line cast to ensure we only push in the direction we're intending to face
        if (Physics.Linecast(midPoint, midPoint + playerPivot.transform.forward, out ray, layer))
        {
            if (ray.collider.gameObject == obj)
            {
                if (CanWalk)
                {
                    if (!Jumping)
                    {
                        canPush = true;
                    }
                }
            }
        }

        Debug.Log("Player can push: " + canPush);
        return canPush;
    }

    public bool DebugGrounded
    {
        get { return onGround; }
    }

    public void Freeze(bool frozen)
    {
        if(frozen)
        {
            CanWalk = false;
        }
        else
        {
            CanWalk = true;
        }
    }

    private void OnDrawGizmos()
    {
        Collider cl = GetComponent<CapsuleCollider>();
        Gizmos.color = Color.blue;
        Vector3 midPoint = transform.position + (transform.up * 0.5f);
//        Gizmos.DrawLine(midPoint, midPoint + transform.forward);

        Vector3 checkDistance = new Vector3(0, cl.bounds.extents.y, 0) + new Vector3(0, -0.1f, 0);
        Vector3 posA = new Vector3(transform.position.x - cl.bounds.extents.x, transform.position.y, transform.position.z);
        Vector3 posB = new Vector3(transform.position.x + cl.bounds.extents.x, transform.position.y, transform.position.z);
        Vector3 posC = new Vector3(transform.position.x, transform.position.y, transform.position.z - cl.bounds.extents.z);
        Vector3 posD = new Vector3(transform.position.x, transform.position.y, transform.position.z + cl.bounds.extents.z);

        Gizmos.color = Color.green;
        Debug.Log("Player bounds: " + cl.bounds.extents);
        Gizmos.DrawSphere(transform.position - checkDistance, 0.1f);

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(posA, 0.05f);
        Gizmos.DrawSphere(posB, 0.05f);
        Gizmos.DrawSphere(posC, 0.05f);
        Gizmos.DrawSphere(posD, 0.05f);

    }
}
