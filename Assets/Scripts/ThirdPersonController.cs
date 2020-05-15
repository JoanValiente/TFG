using UnityEngine;
using UnityEngine.UI;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

public class ThirdPersonController : MonoBehaviour
{
    float hor;
    float ver;
    Vector3 moveDirection;
    float moveAmount;
    Vector3 camYforward;

    Transform camHolder;

    Rigidbody rb;
    Collider col;
    Animator anim;
    FreeClimb freeClimb;

    public float moveSpeed = 4;
    public float rotSpeed = 9;
    public float jumpSpeed = 15;

    public float timer = 0.0f;

    bool onGround;
    bool keepOffGround = false;
    float savedTime;
    bool climbOff;
    float climbTimer;

    public bool canClimb = false;
    public bool isClimbing;

    public bool isMapOpened = false;
    CameraFollow camFollow;
    public GameObject map;
    public bool canMap = false;
    public GameObject compassObj;
    public GameObject note1Obj;
    public GameObject note1Toggle;
    public GameObject note2Obj;
    public GameObject note2Toggle;

    public GameObject paperNote2;
    public bool note2Taken = false;

    public GameObject mainCamera;
    public GameObject mapCamera;

    public int gemCounter = 0;
    public Text gemCounterTex;

    public GameObject flagPrefab;

    public GameObject mapNotification;
    public GameObject winNotification;
    public GameObject treasureNotification;
    public GameObject climbNotification;

    public float notificationTimer = 0.0f;
    public bool isNotification = false;

    public bool isSprint = false;
    public float sprintTimer = 0.0f;
    public float sprintRest = 0.0f;

    //DrawManager drawManager;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.angularDrag = 999;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;

        col = GetComponent<Collider>();
        anim = GetComponentInChildren<Animator>();

        camHolder = CameraFollow.singleton.transform;
        freeClimb = GetComponent<FreeClimb>();
        camFollow = CameraFollow.singleton;
        //drawManager = DrawManager.singleton;

        gemCounterTex.text = gemCounter.ToString() + " / 25";

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void FixedUpdate()
    {
        if (isClimbing)
            return;

        onGround = OnGround();
        if (!isMapOpened)
        {
            Movement();
        }
    }


    void Movement()
    {
        hor = Input.GetAxis("Horizontal");
        ver = Input.GetAxis("Vertical");

        camYforward = camHolder.forward;

        Vector3 v = ver * camHolder.forward;
        Vector3 h = hor * camHolder.right;

        moveDirection = (v + h).normalized;
        moveAmount = Mathf.Clamp01((Mathf.Abs(hor) + Mathf.Abs(ver)));

        Vector3 targetDir = moveDirection;
        targetDir.y = 0;

        Quaternion targetRot = new Quaternion();

        if (targetDir == Vector3.zero)
            targetDir = transform.forward;

        if (isClimbing)
        {
            targetRot = Quaternion.LookRotation(targetDir);
        }
        else
        {
            Quaternion lookDir = Quaternion.LookRotation(targetDir);
            targetRot = Quaternion.Slerp(transform.rotation, lookDir, Time.deltaTime * rotSpeed);
            
        }
        transform.rotation = targetRot;


        Vector3 dir = transform.forward * (moveSpeed * moveAmount);
        dir.y = rb.velocity.y;
        rb.velocity = dir;
    }

    void Update()
    {
        if (isClimbing)
        {
            if(Input.GetKeyDown("space"))
            {
                freeClimb.isClimbing = false;
                EnableController();
                freeClimb.a_hook.enabled = false;
            }
            freeClimb.Tick(Time.deltaTime);
            return;
        }

        onGround = OnGround();
        if(keepOffGround)
        {
            if(Time.realtimeSinceStartup - savedTime > 0.5f)
            {
                keepOffGround = false;
            }
        }

        Jump();

        if(!onGround && !keepOffGround)
        {
            if (timer > 1.0f)
            {
                if (!climbOff && canClimb)
                {
                    isClimbing = freeClimb.CheckForClimb();
                }
                
            }
            if(isClimbing)
            {
                DisableController();
            }

            if (climbOff)
            {
                if(Time.realtimeSinceStartup - climbTimer > 1)
                {
                    climbOff = false;
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.M) && canMap)
        {
            OpenMap();
        }

        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            GameObject go = Instantiate(flagPrefab, transform.position, Quaternion.identity);
            //Vector3 rot = Vector3(-90.0f, 0.0f, 0.0f);
            go.transform.Rotate(-90.0f, 0.0f, 0.0f);
        }

        if (isNotification)
        {
            notificationTimer += Time.deltaTime;
        }

        if(notificationTimer > 8.0f)
        {
            isNotification = false;
            mapNotification.SetActive(false);
            winNotification.SetActive(false);
            climbNotification.SetActive(false);
            treasureNotification.SetActive(false);
            notificationTimer = 0.0f;
        }


        if(sprintTimer > 5.0f)
        {
            isSprint = false;
            sprintTimer = 0.0f;
            moveSpeed = 4.0f;
        }
        if (Input.GetKey(KeyCode.LeftShift) && sprintRest > 5.0f)
        {
            isSprint = true;
            sprintTimer += Time.deltaTime;
            moveSpeed = 6.0f;
        }
        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            isSprint = false;
            sprintTimer = 0.0f;
            moveSpeed = 4.0f;
        }
        if (isSprint)
        {
            anim.SetFloat("move", 3);
        }
        else
        {
            anim.SetFloat("move", moveAmount);
            sprintRest += Time.deltaTime;
        }

        timer += Time.deltaTime;
    }

    void Jump()
    {
        
        bool jump = Input.GetButtonUp("Jump");
        if (jump)
        {
            Vector3 v = rb.velocity;
            v.y = jumpSpeed;
            rb.velocity = v;
            savedTime = Time.realtimeSinceStartup;
            keepOffGround = true;
            anim.SetBool("is_in_air", true);
        }
        
    }

    bool OnGround()
    {
        if (keepOffGround)
            return false;
        Vector3 origin = transform.position;
        origin.y += 0.4f;
        Vector3 direction = -transform.up;
        RaycastHit hit;
        if (Physics.Raycast(origin, direction, out hit, 0.405f))
        {
            anim.SetBool("is_in_air", false);
            return true;
        }

        return false;
    }

    public void DisableController()
    {
        rb.isKinematic = true;
        col.enabled = false;
    }

    public void EnableController()
    {
        rb.isKinematic = false;
        col.enabled = true;
        anim.CrossFade("Jump", 0.2f);
        climbOff = true;
        climbTimer = Time.realtimeSinceStartup;
        isClimbing = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.name == "GoldenChest")
        {
            canClimb = true;
            Destroy(collision.gameObject);
            climbNotification.SetActive(true);
            isNotification = true;
        }
        else if (collision.gameObject.name == "SilverChest")
        {
            canMap = true;
            Destroy(collision.gameObject);
            mapNotification.SetActive(true);
            isNotification = true;
        }
        else if (collision.gameObject.name == "WoodenChest")
        {
            treasureNotification.SetActive(true);
            Destroy(collision.gameObject);
            isNotification = true;
        }
        else if (collision.gameObject.name == "WinFlag")
        {
            winNotification.SetActive(true);
            //Destroy(collision.gameObject);
            isNotification = true;
        }
        else if (collision.gameObject.name == "note2")
        {
            note2Taken = true;
            Destroy(collision.gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 11)
        {
            gemCounter += 1;
            gemCounterTex.text = gemCounter.ToString() + " / 25";
            Destroy(other.gameObject);
        }
    }



    public void OpenMap()
    {
        if (isMapOpened)
        {
            camFollow.isActive = true;
            map.SetActive(false);
            isMapOpened = false;
            mapCamera.tag = "Untagged";
            mainCamera.SetActive(true);
            mainCamera.tag = "MainCamera";
            mapCamera.SetActive(false);
            compassObj.SetActive(false);
            note1Obj.SetActive(false);
            note1Toggle.SetActive(false);
            note2Obj.SetActive(false);
            note2Toggle.SetActive(false);

            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;

            //drawManager.drawingAllowed = false;
        }
        else
        {
            camFollow.isActive = false;
            map.SetActive(true);
            isMapOpened = true;
            Cursor.visible = true;
            mainCamera.tag = "Untagged";
            mapCamera.SetActive(true);
            mapCamera.tag = "MainCamera";
            mainCamera.SetActive(false);
            compassObj.SetActive(true);
            note1Toggle.SetActive(true);

            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

            if (note2Taken)
            {
                note2Toggle.SetActive(true);
            }
            //drawManager.drawingAllowed = true;
        }

    }

    public void ActivateNote1()
    {
        note1Obj.SetActive(!note1Obj.activeSelf);
        note2Obj.SetActive(false);
    }

    public void ActivateNote2()
    {
        note2Obj.SetActive(!note2Obj.activeSelf);
        note1Obj.SetActive(false);
    }

}