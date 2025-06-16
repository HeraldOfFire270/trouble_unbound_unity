using System    .Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pushables : MonoBehaviour {

    public float MoveSpeed = 0.1f;         // How fast does this block move when pushed?
    public BlockColor blockColor = BlockColor.Blue;
    public float PushAngle = 45.0f;
    public float ScaleSpeed = 0.1f;

    public bool TestBlock = false;

    [SerializeField]
    private LevelFlags EventOnPush;
    [SerializeField]
    private LevelFlags EventOnDestroy;
    
    private float pushTimer;        // How long has the block been pushed in this direction?
    private bool isMoving;          // Do we have a move target?
    private bool IsTeleporting;     // Are we in the teleport animation?

    private Animator anim;

    private bool isLocked;
    private bool isMelting;
    private bool lastAirCheck;      // was the block airbourne last frame?
    private bool onPlatform;
    private Vector3 moveTarget;
    private Transform TelePoint;
    private Vector3 warpTarget;
    private GameObject blockDetector;
    private Rigidbody rBody;
    private TextMesh debugText;
    private int weight;
    private Trapdoor startDoor;

    private TrapDoorState currentState;

    private float blockScale = 1.0f;

    private Vector3 StartingPoint;
    private RigidbodyConstraints defaultConstraints;

    private HitDirection LastMoveDirection;

    private void Awake()
    {
        rBody = GetComponent<Rigidbody>();
        LastMoveDirection = HitDirection.None;
        IsTeleporting = false;
        isMoving = false;
        isLocked = false;
        isMelting = false;
        lastAirCheck = false;
        onPlatform = false;
        anim = GetComponentInChildren<Animator>();
        defaultConstraints = rBody.constraints;
        moveTarget = transform.position;
    }

    void Start () {

        // Push leniancy cannot exceed 80 degrees!
        if (PushAngle >= 80f) PushAngle = 80.0f;

        // No negative scales for scalespeed or we'd get unwanted results, so we flip it if one is entered
        if (ScaleSpeed < 0) ScaleSpeed = -ScaleSpeed;

        // We only need half for future calculations, so divide it in two.
        PushAngle = (PushAngle / 2);

        GameObject child;
        for (int i = 0; i < transform.childCount; i++)
        {
            child = transform.GetChild(i).gameObject;
            switch (child.tag)
            {
                case "BlockDetector":
                    if (!blockDetector) { blockDetector = child; } else { Debug.LogError("Multiple Block Detectors on object. Using first only!"); }
                    break;
            }

            StartingPoint = transform.position;
        }

        debugText = GetComponent<TextMesh>();
	}

    private void Update()
    {
        if (!isMoving)
        {
            if (transform.parent != null)
            {
                bool hit = false;
                Collider[] objects;
                objects = Physics.OverlapBox(transform.position, new Vector3(0.5f, 0.5f, 0.5f), Quaternion.identity);
                foreach (Collider c in objects)
                {
                    if (c.tag == "Platform") hit = true;
                }
                if (!hit)
                {
                    moveTarget = GameManager.VectorToGrid(transform.root.TransformPoint(moveTarget));
                    transform.parent = null;
                    onPlatform = false;
                    isMoving = true;
                    Debug.Log("Left platform");
                }
            }
        }

        if (debugText)
        {
            debugText.text = "Lock Status: " + isLocked + "\nTeleport Status: " + IsTeleporting + "\nMove Status: " + isMoving + "\nAirbourne: " + IsAirbourne();
        }
    }


    void FixedUpdate() {

        Vector3 variance;
        if (!isMelting)
        {
            weight = BlockWeight();
        }
        else
        {
            weight = 0;
        }

        // Check if we're at or below the world border
        if (transform.position.y < GameManager.BlockFallLimit)
        {
            Vector3 pos = new Vector3(transform.position.x, GameManager.BlockFallLimit, transform.position.z);
            transform.position = pos;
            rBody.velocity = Vector3.zero;
            rBody.constraints = RigidbodyConstraints.FreezeAll;
            Destroyed();
        }

        // Is the block teleporting?

        if (!IsTeleporting)
        {
            // We're not teleporting, so let's move!
            if (isMoving && !isLocked)
            {
                if (onPlatform)                  
                {
                    variance = transform.localPosition - moveTarget;
                }
                else
                {
                    variance = transform.position - moveTarget;
                }
                if (variance.y < 0.1f && variance.y > -0.1f)
                {
                    if (onPlatform)
                    {
                        transform.localPosition = Vector3.MoveTowards(transform.localPosition, moveTarget, (MoveSpeed * Time.deltaTime));
                        if (transform.localPosition == moveTarget)
                        {
                            //                    transform.position = tempPos;
                            if (blockColor == BlockColor.Ice)
                            {
                                if (!CheckAndMove(LastMoveDirection))
                                {
                                    isMoving = false;
                                }
                            }
                            else
                            {
                                isMoving = false;
                            }
                        }
                    }
                    else
                    {
                        transform.position = Vector3.MoveTowards(transform.position, moveTarget, (MoveSpeed * Time.deltaTime));
                        if (transform.position == moveTarget)
                        {
                            //                    transform.position = tempPos;
                            if (blockColor == BlockColor.Ice)
                            {
                                if (!CheckAndMove(LastMoveDirection))
                                {
                                    isMoving = false;
                                }
                            }
                            else
                            {
                                isMoving = false;
                            }
                        }
                    }
                }
            }
        }
	}

    private void OnCollisionEnter(Collision cl)
    {
        if (cl.gameObject.tag != "Player") return;
        if (!cl.gameObject.GetComponent<Player>().CanPush(gameObject)) return;
        
        if (isLocked || isMoving)
        {
            if (GameManager.DebugMode)
            {
                Debug.Log("Block is locked or currently in motion!");
            }
            return;
        }
        int distance = 1;
        int maxPushWeight = 1;
        int maxPushDistance = 1;
        Player pScript;

        Vector3 Strike = Vector3.zero;
        HitDirection SideHit;
        Vector3 vDir = Vector3.zero;

        ContactPoint hit = cl.contacts[0];

        bool canMove;
        
        if (cl.relativeVelocity.magnitude < 2.0f)
        {
            return;      // Ignore if only a light touch
        }

        /* Side Hit Reference
            Forward = Z 0.5
            Back = Z -0.5
            Left = X -0.5
            Right = X 0.5 */

        pScript = cl.gameObject.GetComponent<Player>();

        maxPushWeight = pScript.MaxPushWeight;
        maxPushDistance = pScript.MaxPushDistance;

        if (maxPushWeight < weight) { Debug.Log("Too heavy to push!\nCurrent Weight: " + weight + "\nMax Weight:" + maxPushWeight); return; }

        Debug.Log("Block Transform Y / Player Transform Y: " + transform.position.y + ", " + cl.gameObject.transform.position.y);
        float Ydiff = cl.gameObject.transform.position.y - (transform.position.y - 0.5f);

        Debug.Log("Height difference: " + Ydiff);
        // Ignore if coming from above or below -- we only care about side-on collisions for movement
        if (Ydiff > 0.02f || Ydiff < -0.02f)
        {
            if (GameManager.DebugMode && TestBlock) Debug.Log("Block not moving due to height difference:" + Ydiff);
            return;
        }

        /*if (hit.otherCollider.gameObject.transform.position.y > transform.position.y) return;
        if (hit.otherCollider.gameObject.transform.position.y < transform.position.y - 0.5f) return;*/

        Strike = hit.point - gameObject.transform.position;
        SideHit = ReturnDirection(GetCollisionAngle(hit.point));

        switch (SideHit)
        {
            case HitDirection.Back:
                vDir = Vector3.forward;
                break;
            case HitDirection.Forward:
                vDir = Vector3.back;
                break;
            case HitDirection.Left:
                vDir = Vector3.right;
                break;
            case HitDirection.Right:
                vDir = Vector3.left;
                break;
            default:
                return;
        }

        if (vDir != Vector3.zero) distance = PushWeight(vDir);

        if (maxPushDistance > 1)
        {
            if (weight <= 1)
            {
                canMove = CheckAndMove(SideHit, maxPushDistance, true);

                if (canMove)
                {
                    if (EventOnPush != LevelFlags.None)
                    {
                        GameManager.AddFlag(EventOnPush);
                    }
                }
                else
                {
                    Debug.Log("Block cannot move to new vector: " + gameObject);
                }
            }
            else
            {
                Debug.Log("Block too heavy to move: " + gameObject);
            }
        }
        else
        {
            if (CheckAndMove(SideHit, maxPushWeight))
            {
                if (EventOnPush != LevelFlags.None)
                {
                    GameManager.AddFlag(EventOnPush);
                }
            }
            else
            {
                Debug.Log("Block cannot move to new vector: " + gameObject);
            }
        }
    }

    public bool CheckAndMove(HitDirection dir, int maxWeight = 1, bool checkPush = false)
    {
        int maxPush = maxWeight - weight;
        bool isHit = false;
        bool blockPushed = false;

        if (IsTeleporting)
        {
            Debug.Log("Block currently teleporting.");
            return false;
        }
        if (isLocked)
        {
            Debug.Log("Exiting due to already locked!");
            return false;
        }
        if (dir == HitDirection.None)
        {
            Debug.Log("No hit direction");
            return false;
        }

        if (IsAirbourne())
        {
            if (transform.root.tag != "Platform")
            {
                Debug.Log("No ground found!");
                return false;
            }
        }

        if (weight > maxWeight)
        {
            Debug.Log("Weight exceeds limit!");
            return false;
        }

        int LayerMask = (1 << 8 | (1 << 9));
        Pushables p;
        RaycastHit ray = new RaycastHit();

        if(weight > 1 && maxWeight > 1)
        {
            Physics.Raycast(transform.position, transform.TransformDirection(Vector3.up), out ray, (1 << 9));
            p = ray.collider.gameObject.GetComponent<Pushables>();
            if (p)
            {
                p.CheckAndMove(dir, maxWeight - 1);
            }
        }

        if(checkPush && maxWeight > 1)
        {
            switch(dir)
            {
                case HitDirection.Forward:
                    isHit = Physics.Raycast(transform.position, transform.TransformDirection(Vector3.back), out ray, 1.0f, (1 << 9));
                    break;
                case HitDirection.Back:
                    isHit = Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out ray, 1.0f, (1 << 9));
                    break;
                case HitDirection.Left:
                    isHit = Physics.Raycast(transform.position, transform.TransformDirection(Vector3.right), out ray, 1.0f, (1 << 9));
                    break;
                case HitDirection.Right:
                    isHit = Physics.Raycast(transform.position, transform.TransformDirection(Vector3.left), out ray, 1.0f, (1 << 9));
                    break;
            }

            if (isHit)
            {
                p = ray.collider.gameObject.GetComponent<Pushables>();
                if (p)
                {
                    if (p.CheckAndMove(dir, maxWeight - 1, true))
                    {
                        blockPushed = true;
                    }
                }
            }
        }

        // If a block was pushed, collision checks won't be accurate, so we need to move with our partner block, hence a 'ForcedMove' call
        if(blockPushed)
        {
            Debug.Log("Using ForcedMove");
            return ForcedMove(dir);
        }

        switch (dir) { 
        
           case HitDirection.Forward:
                if (!Physics.Raycast(transform.position, -Vector3.forward, out ray, 1.2f, LayerMask))
                {
                    if (onPlatform)
                    {
                        moveTarget = GameManager.VectorToGrid(GameManager.VectorToGrid(transform.localPosition) - Vector3.forward);
                    }
                    else
                    {
                        moveTarget = GameManager.VectorToGrid(GameManager.VectorToGrid(transform.position) - Vector3.forward);
                    }
                    LastMoveDirection = dir;
                    isMoving = true;
                    return true;
                }
                else
                {
                        Debug.Log("Pushed Block obstructed!");
                }
                break;
            case HitDirection.Back:
                if (!Physics.Raycast(transform.position, Vector3.forward, out ray, 1.2f, LayerMask))
                {
                    if (onPlatform)
                    {
                        moveTarget = GameManager.VectorToGrid(GameManager.VectorToGrid(transform.localPosition) + Vector3.forward);
                    }
                    else
                    {
                        moveTarget = GameManager.VectorToGrid(GameManager.VectorToGrid(transform.position) + Vector3.forward);
                    }
                    LastMoveDirection = dir;
                    isMoving = true;
                    return true;
                }
                else
                {
                    Debug.Log("Pushed Block obstructed!");
                }
                break;
            case HitDirection.Left:
                if (!Physics.Raycast(transform.position, -Vector3.left, out ray, 1.2f, LayerMask))
                {
                    if (onPlatform)
                    {
                        moveTarget = GameManager.VectorToGrid(GameManager.VectorToGrid(transform.localPosition) - Vector3.left);
                    }
                    else
                    {
                        moveTarget = GameManager.VectorToGrid(GameManager.VectorToGrid(transform.position) - Vector3.left);
                    }
                    LastMoveDirection = dir;
                    isMoving = true;
                    return true;
                }
                else
                {
                    Debug.Log("Pushed Block obstructed!");
                }
                break;
            case HitDirection.Right:
                if (!Physics.Raycast(transform.position, Vector3.left, out ray, 1.2f, LayerMask))
                {
                    if (onPlatform)
                    {
                        moveTarget = GameManager.VectorToGrid(GameManager.VectorToGrid(transform.localPosition) + Vector3.left);
                    }
                    else
                    {
                        moveTarget = GameManager.VectorToGrid(GameManager.VectorToGrid(transform.position) + Vector3.left);
                    }
                    LastMoveDirection = dir;
                    isMoving = true;
                    return true;
                }
                else
                {
                    Debug.Log("Pushed Block obstructed!");
                }
                break;
            default:
                LastMoveDirection = HitDirection.None;
                Debug.Log("No Direction provided!");
                return false;
        }
        Debug.Log("Reached End of function! No movement!");
        return false;
    }

    public bool ForcedMove(HitDirection dir)
    {
        if (IsTeleporting)
        {
            return false;
        }

        // Even a Forced move can't move a locked block!
        if (isLocked)
        {
            Debug.Log("Exiting due to already locked!");
            return false;
        }

        // Can't move in a null direction!
        if (dir == HitDirection.None) return false;

        /// No flying blocks!
        if (IsAirbourne())
        {
            Debug.Log("No ground found!");
            return false;
        }

        switch (dir)
        {

            case HitDirection.Forward:
                moveTarget = GameManager.VectorToGrid(GameManager.VectorToGrid(transform.position) - Vector3.forward);
                LastMoveDirection = dir;
                isMoving = true;
                return true;
            case HitDirection.Back:
                moveTarget = GameManager.VectorToGrid(GameManager.VectorToGrid(transform.position) + Vector3.forward);
                LastMoveDirection = dir;
                isMoving = true;
                return true;
            case HitDirection.Left:
                moveTarget = GameManager.VectorToGrid(GameManager.VectorToGrid(transform.position) - Vector3.left);
                LastMoveDirection = dir;
                isMoving = true;
                return true;
            case HitDirection.Right:
                moveTarget = GameManager.VectorToGrid(GameManager.VectorToGrid(transform.position) + Vector3.left);
                LastMoveDirection = dir;
                isMoving = true;
                return true;
            default:
                LastMoveDirection = HitDirection.None;
                Debug.Log("Hit Direction invalid!");
                return false;
        }
    }

    public void CheckAndMove(Vector3 newPosition, bool NoAirCheck=false)
    {
        if (isLocked)
        {
            Debug.Log("Exiting due to already locked!");
            return;
        }
        if (IsTeleporting)
        {
            Debug.Log("Exiting due to currently mid-transit!");
            return;
        }

        if (!NoAirCheck)
        {
            if (IsAirbourne())
            {
                Debug.Log("No ground found!");
                return;
            }
        }
        Debug.Log("Moving to fixed position: " + newPosition);

        int LayerMask = (1 << 8 | (1 << 9));
        RaycastHit ray = new RaycastHit();

        if (!Physics.Raycast(transform.position, newPosition, out ray, 2.0f, LayerMask))
        {
            moveTarget = GameManager.VectorToGrid(newPosition);
            isMoving = true;
            LastMoveDirection = ReturnDirection(Vector3.Angle(moveTarget, transform.position));
            Debug.Log("Last Hit Direction: " + LastMoveDirection);
            return;
        }
        else
        {
            Debug.Log("Unable to move to vector!");
        }

    }

    private HitDirection GetConveyorDirection(Vector3 v)
    {
        if (v.x < 0) return HitDirection.Right;
        if (v.x > 0) return HitDirection.Left;
        if (v.z < 0) return HitDirection.Forward;
        if (v.z > 0) return HitDirection.Back;
        Debug.Log("No Direction found for conveyor!");
        return HitDirection.None;
    }

    private bool IsAirbourne()
    {
        bool raycastCheck;
        int LayerMask = (1 << 8 | (1 << 9));
        RaycastHit ray = new RaycastHit();
        raycastCheck = !Physics.Raycast(transform.position, transform.TransformDirection(Vector3.down), out ray, 0.6f, LayerMask);
        return raycastCheck;
    }

    public int BlockWeight()
    {
        if (isMelting) { Debug.Log("Block Weight called while melting!"); return 0; }
        BoxCollider[] colls = GetComponents<BoxCollider>();
        rBody.useGravity = false;
        foreach(BoxCollider cl in colls)
        {
            cl.enabled = false;
        }

        int wt = 1;
        int LayerMask = (1 << 9);
        RaycastHit ray = new RaycastHit();
        Collider c;
        if(Physics.Raycast(transform.position, transform.TransformDirection(Vector3.up), out ray, 0.5f, LayerMask))
        {
            c = ray.collider;
            switch(c.gameObject.tag)
            {
                case "Pushables":
                    wt += c.gameObject.GetComponent<Pushables>().GetWeight;
                    break;
            }
            
        }

        foreach (BoxCollider cl in colls)
        {
            cl.enabled = true;
        }
        rBody.useGravity = true;

        return wt;
    }

    public int PushWeight(Vector3 vDir)
    {
        int wt = 1;
        int layerMask = (1 << 9);
        RaycastHit ray = new RaycastHit();
        Collider c;
        if(Physics.Raycast(transform.position, transform.TransformDirection(vDir), out ray, 0.5f, layerMask))
        {
            c = ray.collider;
            switch(c.gameObject.tag)
            {
                case "Pushables":
                    wt += c.gameObject.GetComponent<Pushables>().PushWeight(vDir);
                    break;
            }
        }
        return wt;
    }

    public bool CanTeleport(Vector3 obj)
    {
        int layerMask = (1 << 8 | (1 << 9));
        return (!Physics.CheckBox(obj, new Vector3(0.49f, 0.49f, 0.49f), gameObject.transform.rotation, layerMask));
    }

    public void Warp(Transform obj)
    {
        if (!IsTeleporting)
        {
            if (CanTeleport(obj.position + obj.up))
            {
                warpTarget = (obj.position + obj.up);
                TelePoint = obj;
                IsTeleporting = true;
                gameObject.layer = 15;                  // ghost until warped
                rBody.useGravity = false;
                rBody.velocity = Vector3.zero;
            }
            else
            {
                Debug.Log("Cannot teleport to location: " + (obj.position + obj.up));
                Debug.Log("Position of Destination: " + (obj.position + obj.up));
            }
        }
        else
        {
            Debug.Log("Block is already teleporting!");
        }
    }

    public bool TrapdoorWarp(Trapdoor trapDoor)
    {
            if (CanTeleport(trapDoor.GetDestination))
            {
                startDoor = trapDoor;
                anim.Play("blockTrapdoorDrop");
                rBody.useGravity = false;
                IsTeleporting = true;
                moveTarget = GameManager.VectorToGrid(trapDoor.transform.position + trapDoor.transform.up);
                transform.position = moveTarget;
                return true;
            }
           return false;
    }

    public bool TrapdoorWarp(TrapDoorState State)
    {
        if(!startDoor) { return false; }

        Vector3 destination;
        Trapdoor paired = startDoor.PairedTrapdoor.GetComponent<Trapdoor>();
        HitDirection dir = startDoor.GetDirection();
        string animation = "";

        switch (State)
        {
            case TrapDoorState.BlockEntry:
                // Disable collisions
                rBody.useGravity = false;
                rBody.velocity = Vector3.zero;
                foreach (Collider c in GetComponentsInChildren<Collider>())
                {
                    c.enabled = false;
                }
                paired.Receive(gameObject);
                IsTeleporting = true;
                return true;
            case TrapDoorState.BlockExit:
                // Enable collisions
                foreach (Collider c in GetComponentsInChildren<Collider>())
                {
                    c.enabled = true;
                }
                rBody.useGravity = true;
                paired.Close();
                IsTeleporting = false;
                isLocked = false;
                isMoving = false;
                return true;
            case TrapDoorState.SourceOpen:
                anim.Play("blockTrapdoorDrop");
                return true;
            case TrapDoorState.SourceClosed:
                return true;
            case TrapDoorState.DestinationOpen:
                destination = startDoor.PairedTrapdoor.transform.position + startDoor.PairedTrapdoor.transform.up;
                Teleport(destination);
                switch(dir)
                {
                    case HitDirection.Top:
                        animation = "blockTrapdoorEmergeTop";
                        break;
                    case HitDirection.Bottom:
                        animation = "blockTrapdoorEmergeBottom";
                        break;
                    case HitDirection.Forward:
                        animation = "blockTrapdoorEmergeNorth";
                        break;
                    case HitDirection.Back:
                        animation = "blockTrapdoorEmergeSouth";
                        break;
                    case HitDirection.Left:
                        animation = "blockTrapdoorEmergeWest";
                        break;
                    case HitDirection.Right:
                        animation = "blockTrapdoorEmergeEast";
                        break;
                    default:
                        Debug.LogError("Trapdoor destination does not meet directional requirements.");
                        break;
                }
                if (animation != "")
                {
                    anim.Play(animation);
                }
                return true;
            default:
                return false;
        }
    }

    private bool Teleport(Vector3 vector)
    {
        transform.position = GameManager.VectorToGrid(vector);
        moveTarget = transform.position;
        return true;
    }

    private bool AnimationPlaying(string animName)
    {
        return (anim.GetCurrentAnimatorStateInfo(0).IsName(animName));
    }

    private HitDirection ReturnDirection(float angle)
    {
        HitDirection hitDirection = HitDirection.None;

        if(angle >= (360f - PushAngle) || angle <= PushAngle)
        {
            hitDirection = HitDirection.Forward;
        }
        if(angle >= (90 - PushAngle) && angle <= (90 + PushAngle))
        {
            hitDirection = HitDirection.Left;
        }
        if(angle >= (180 - PushAngle) && angle <= (180 + PushAngle))
        {
            hitDirection = HitDirection.Back;
        }
        if(angle >= (270 - PushAngle) && angle <= (270 + PushAngle))
        {
            hitDirection = HitDirection.Right;
        }

        return hitDirection;
    }

    public float GetCollisionAngle(Vector3 contactPoint)
    {

        Vector3 collidertWorldPosition = new Vector3(transform.position.x, transform.position.y, transform.position.z);
        Vector3 pointB = contactPoint - collidertWorldPosition;
 
        float theta = Mathf.Atan2(pointB.x, pointB.z);
        float angle = (360 - ((theta * 180) / Mathf.PI)) % 360;
        return angle;
    }

    public bool LockStatus
    {
        get { return isLocked; }
        set { isLocked = value; }
    }

    public bool MoveStatus
    {
        get { return isMoving; }
        set { isMoving = value; }
    }

    public bool Teleporting
    {
        get { return IsTeleporting; }
    }

    public void Melt()
    {
        isMelting = true;
        rBody.useGravity = false ;
        rBody.velocity = Vector3.zero;
        isLocked = true;
        foreach (Collider c in GetComponentsInChildren<Collider>())
        {
            c.enabled = false;
        }
        if(blockColor == BlockColor.Ice)
        {
            anim.Play("blockIceMelt");
        }
        else
        {
            anim.Play("blockLavaMelt");
        }
    }

    public void Destroyed()
    {
        if (EventOnDestroy != LevelFlags.None)
        {
            GameManager.AddFlag(EventOnDestroy);
        }
    }

    public void Transporting(Vector3 snapPoint)
    {
        onPlatform = true;
        Vector3 beforeChange = moveTarget;
        moveTarget = GameManager.VectorToGrid(transform.root.InverseTransformPoint(moveTarget));
        Debug.Log(string.Format("Move Target - Before: {0} After: {1}", beforeChange, moveTarget));
    }

    public void ResetBlock()
    {
        transform.position = StartingPoint;
        transform.parent = null;
        isLocked = false;
        isMoving = false;
        isMelting = false;
        onPlatform = false;
        lastAirCheck = false;
        anim.Play("blockStatic");
        moveTarget = transform.position;
        rBody.useGravity = true;
        rBody.velocity = Vector3.zero;
        foreach (Collider c in GetComponentsInChildren<Collider>())
        {
            c.enabled = true;
        }

        rBody.constraints = defaultConstraints;
    }

    public int GetWeight
    {
        get { return weight; }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, (transform.position + transform.up));

        Gizmos.color = Color.magenta;
        if (moveTarget != null)
        {
            if (onPlatform)
            {
                Gizmos.DrawWireCube(moveTarget + transform.root.position, new Vector3(0.5f, 0.5f, 0.5f));
            }
            else
            {
                Gizmos.DrawWireCube(moveTarget, new Vector3(0.5f, 0.5f, 0.5f));
            }
        }
        else
        {
            Gizmos.DrawWireCube(transform.position, new Vector3(0.5f, 0.5f, 0.5f));
        }
    }
}
