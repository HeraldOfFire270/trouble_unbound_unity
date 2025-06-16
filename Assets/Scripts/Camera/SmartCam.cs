using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmartCam : MonoBehaviour {

    public GameObject mainPlayer;
    public float ZoomSpeed = 0.01f;
    public float ZoomMaximum = 5.0f;
    public float ZoomMinimum = 1.0f;

    public float turnSpeed = 128;
    private float zoom = 0;

    private Transform camPos;
    private int rotationTarget;
    private Quaternion targetRotation;
    private Vector3 playerVector;
    private Vector3 targetPos = Vector3.zero;
    private float currentZoom;
    private float forcedZoom;

    // Use this for initialization
    void Start () {
        camPos = transform.GetChild(0);

        GameManager.MainCamera = camPos.gameObject.GetComponent<Camera>();
        if (!GameManager.MainCamera)
        {
            Debug.Log("No camera!");
            Destroy(this);
        }
        if (!mainPlayer)
        {
            mainPlayer = GameObject.FindGameObjectWithTag("Player");
            if (!mainPlayer)
            {
                Debug.LogError("No Player!");
                Destroy(this);
            }
        }

        transform.position = mainPlayer.transform.position;
        playerVector = Vector3.zero - camPos.localPosition;
        currentZoom = playerVector.magnitude;

        int currentFacing = (int)transform.eulerAngles.y;

        switch (currentFacing)
        {
            case 0:     // North
                rotationTarget = 0;
                break;
            case 90:
                rotationTarget = 1;
                break;
            case 180:
                rotationTarget = 2;
                break;
            case 270:
                rotationTarget = 3;
                break;
            case -90:
                rotationTarget = 3;
                break;
            case -180:
                rotationTarget = 2;
                break;
            case -270:
                rotationTarget = 1;
                break;
            default:
                transform.rotation = Quaternion.identity;
                rotationTarget = 0;
                break;
        }
    }

    void Update()
    {
        float Rtrig;
        float Ltrig;

        float actualZoom;
        float distance;

        playerVector = Vector3.zero - camPos.localPosition;
        actualZoom = playerVector.magnitude;

        Rtrig = Input.GetAxis("Right Trigger");
        Ltrig = Input.GetAxis("Left Trigger");

        if (Input.GetButtonDown("LeftBumper") || Input.GetKeyDown(KeyCode.Comma))
        {
            rotationTarget++;
            if (rotationTarget > 3)
            {
                rotationTarget = 0;
            }
        }

        if (Input.GetButtonDown("RightBumper") || Input.GetKeyDown(KeyCode.Period))
        {
            rotationTarget--;
            if (rotationTarget < 0)
            {
                rotationTarget = 3;
            }
        }

        if (Rtrig >= 1 || Input.GetKey(KeyCode.KeypadPlus))
        {
            currentZoom += ZoomSpeed;
        }
        if (Ltrig >= 1 || Input.GetKey(KeyCode.KeypadMinus))
        {
            currentZoom -= ZoomSpeed;
        }

        if (currentZoom > ZoomMaximum)
        {
            currentZoom = ZoomMaximum;
        }
        if (currentZoom < ZoomMinimum)
        {
            currentZoom = ZoomMinimum;
        }

        if (currentZoom != actualZoom)
        {
            targetPos = playerVector.normalized * currentZoom;
        }

        switch(rotationTarget)
        {
            case 0:     // North
                targetRotation = Quaternion.Euler(new Vector3(0, 0, 0));
                break;
            case 1:     // East
                targetRotation = Quaternion.Euler(new Vector3(0, 90, 0));
                break;
            case 2:     // South
                targetRotation = Quaternion.Euler(new Vector3(0, 180, 0));
                break;
            case 3:     // West
                targetRotation = Quaternion.Euler(new Vector3(0, 270, 0));
                break;
            default:    // Something went wrong! Default to North
                targetRotation = Quaternion.Euler(Vector3.forward * 90);
                rotationTarget = 0;
                break;
        }
        transform.position = mainPlayer.transform.position;

        forcedZoom = 0.0f;
        CheckVisibility();

        if (forcedZoom != 0.0f)
        {
            if (actualZoom != forcedZoom) targetPos = playerVector.normalized * -forcedZoom;
        }
        else
        {
            if (actualZoom != currentZoom) targetPos = playerVector.normalized * -currentZoom;
        }

        distance = Vector3.Distance(camPos.localPosition, targetPos);

        if (distance < ZoomSpeed)
        {
            camPos.localPosition = Vector3.Lerp(camPos.localPosition, targetPos, 1.0f);
        }
        else
        {
            camPos.localPosition = Vector3.Lerp(camPos.localPosition, targetPos, ZoomSpeed);
        }
    }

    void LateUpdate()
    {
        float distance;
        float step = turnSpeed * Time.deltaTime;
        float speedModifier = 1.0f;

        distance = Vector3.Distance(camPos.localPosition, targetPos);
        if (distance >= ZoomSpeed * 10)
        {
            speedModifier = 50.0f;
        }
        else if (distance >= ZoomSpeed * 5)
        {
            speedModifier = 25.0f;
        }

        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, step);
        camPos.LookAt(mainPlayer.transform.position);
    }

    void UpdateRotation()
    {
        float step = turnSpeed * Time.deltaTime;
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, step);
    }

    void CheckVisibility()
    {
        string Tag = "No object";
        Bounds playerBounds = mainPlayer.GetComponent<CapsuleCollider>().bounds;
        Renderer r;
        BlockVisibility b;
        RaycastHit midHit;
        RaycastHit reverse;

        bool hit;
        bool reverseHit;

        Vector3 _playerMid = mainPlayer.transform.position + ((Vector3.up * playerBounds.size.y) / 2);

        hit = Physics.Linecast(camPos.position, _playerMid, out midHit, (int)PhysicsLayer.AllVisible);

        if (hit)
        {
            Tag = midHit.collider.tag;
            if (Tag != "Player")
            {
                if (Tag == "Pushables")
                {
                    Pushables p = midHit.collider.gameObject.GetComponentInChildren<Pushables>();
                    BlockColor clr = p.blockColor;

                    if (clr == BlockColor.Ice) return;

                    r = midHit.collider.gameObject.GetComponentInChildren<Renderer>();

                    if (r != null)
                    {
                        b = r.gameObject.GetComponent<BlockVisibility>();
                        if (b == null)
                        {
                            b = r.gameObject.AddComponent<BlockVisibility>();
                        }

                        b.Translucency();
                    }
                    else
                    {
                        Debug.LogError("No Renderer found on object: " + r.gameObject);
                    }
                }
                else
                {
                    reverseHit = Physics.Linecast(mainPlayer.transform.position, camPos.position, out reverse, (int)PhysicsLayer.AllVisible);
                    if (reverseHit)
                    {
                        forcedZoom = (camPos.position - reverse.point).magnitude;
                        camPos.position = reverse.point;
                    }
                }
            }
        }

    }

    bool PlayerVisible()
    {
        int intLayerMask = (int)PhysicsLayer.AllVisible;
        float fHeight = mainPlayer.GetComponent<Renderer>().bounds.size.y;
        int rayHits = 0;

        Vector3 _playerTop = mainPlayer.transform.position + (Vector3.up * fHeight);
        Vector3 _playerBot = mainPlayer.transform.position - (Vector3.up * fHeight);

        // Three point check system: Top, Mid, Bottom. If any one is false, we can see the player

        if (Physics.Linecast(GameManager.MainCamera.transform.position, _playerTop, intLayerMask))
        {
            rayHits++;
        }

        if (Physics.Linecast(GameManager.MainCamera.transform.position, mainPlayer.transform.position, intLayerMask))
        {
            rayHits++;
        }

        if (Physics.Linecast(GameManager.MainCamera.transform.position, _playerBot, intLayerMask))
        {
            rayHits++;
        }

        if(rayHits >= 2)
        {
            return false;
        }

        return true;
    }

    void OnDrawGizmos()
    {
        if (mainPlayer != null)
        {
            Bounds playerBounds = mainPlayer.GetComponent<CapsuleCollider>().bounds;
            Vector3 _playerTop = mainPlayer.transform.position + (Vector3.up * playerBounds.size.y);
            Vector3 _playerMid = mainPlayer.transform.position + ((Vector3.up * playerBounds.size.y) / 2);
            Vector3 _playerBot = mainPlayer.transform.position;

            if (!camPos) camPos = transform.GetChild(0);
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(camPos.position, _playerMid);
            Gizmos.color = Color.green;
            Gizmos.DrawLine(camPos.position, _playerBot);
            Gizmos.DrawLine(camPos.position, _playerTop);
        }
    }
}
