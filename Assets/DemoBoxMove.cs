using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemoBoxMove : MonoBehaviour
{
    [SerializeField]
    private float boxSize = 1.0f;
    [SerializeField]
    private BlockColor blockColor = BlockColor.Blue;
    [SerializeField]
    private float blockSpeed = 0.1f;
    [SerializeField]
    private float pushDelay = 0.5f;
    [SerializeField]
    private float errorMargin = 0.2f;

    private bool isHeld;
    private bool isMoving;
    private float pushTimer;            // How long since we were last pushed?
    private Vector3 startMovePosition;  // Where was the box before being pushed?
    private Vector3 endMovePosition;    // Where is the box going to end up?

    private Coroutine blockMove;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private Vector3 GetTarget(Vector3 hitVector)
    {
        Vector3 v = Vector3.zero;
        Quaternion hitAngle;
        hitAngle = Quaternion.LookRotation(hitVector, Vector3.up);
        return v;
    }

    IEnumerator MoveToTarget(Vector3 target)
    {
        isMoving = true;
        Debug.Log("Coroutine invoked!");
        while (Vector3.Distance(transform.position, target) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, target, blockSpeed * Time.deltaTime);
            yield return null;
        }

        transform.position = target;

        isMoving = false;
        yield return new WaitForSeconds(0.5f);
    }

    public void Align()

    {
        if (isHeld)
        {
            transform.parent = null;
            isHeld = false;
        }
        if (blockMove != null) StopCoroutine(blockMove);
        blockMove = StartCoroutine("MoveToTarget", GameManager.VectorToGrid(transform.position));
    }

    public void Grabbed(Transform parent)
    {
        isHeld = true;
        transform.parent = parent;
    }

    private void OnTriggerEnter(Collider other)
    {
        switch(other.tag)
        {
            case "Trapdoor":
                break;
            default:
                return;
        }
    }
}
