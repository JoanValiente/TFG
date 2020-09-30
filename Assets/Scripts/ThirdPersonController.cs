using System;
using System.Collections.Generic;
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

    [Header("Movement")]
    public float moveSpeed = 4;
    public float rotSpeed = 9;
    public float jumpSpeed = 15;
    public bool canLevitate = false;
    public bool isLevitating = false;

    public float timer = 0.0f;

    bool onGround;
    bool keepOffGround = false;
    float savedTime;
    bool climbOff;
    float climbTimer;

    [Header("First Zone")]
    public bool canClimb = false;
    public bool isClimbing;

    public bool isMapOpened = false;
    public bool canMap = false;
    public GameObject compassObj;
    public GameObject note1Obj;
    public GameObject note1Toggle;
    public GameObject note2Obj;
    public GameObject note2Toggle;
    public GameObject valleyMapBtn;

    [Header("Maps")]
    public bool canDesertMap = false;
    public GameObject desertMapBtn;

    public bool canForestMap = false;
    public GameObject forestMapBtn;

    public bool canDungeonMap = false;
    public GameObject dungeonMapBtn;

    public bool canSkycityMap = false;
    public GameObject skycityMapBtn;

    public GameObject paperNote2;
    public bool note2Taken = false;

    [Header("Cameras")]
    public GameObject mainCamera;
    public GameObject mapCamera;
    CameraFollow camFollow;

    [Header("Gems")]
    public int gemCounter = 0;
    public Text gemCounterTex;
    public int gemDesertCounter = 0;
    public Text gemDesertCounterTex;
    public int gemForestCounter = 0;
    public Text gemForestCounterTex;
    public int gemDungeonCounter = 0;
    public Text gemDungeonCounterTex;
    public int gemSkycityCounter = 0;
    public Text gemSkycityCounterTex;

    [Header("Notifications")]
    public GameObject mapNotification;
    public GameObject winNotification;
    public GameObject treasureNotification;
    public GameObject climbNotification;
    public GameObject desertTreasureNotification;
    public GameObject desertToolNotification;
    public GameObject desertMapNotification;
    public GameObject desertKeyNotification;
    public GameObject desertCristalNotification;
    public GameObject forestTreasureNotification;
    public GameObject forestToolNotification;
    public GameObject forestMapNotification;
    public GameObject forestKeyNotification;
    public GameObject forestCristalNotification;
    public GameObject dungeonMapNotification;
    public GameObject dungeonTreasureNotification;
    public GameObject dungeonCristalNotification;
    public GameObject allLeversNotification;
    public GameObject skycityMapNotification;
    public GameObject skycityTreasureNotification;
    public GameObject skycityCristalNotification;
    public GameObject skycityToolNotification;
    public float notificationTimer = 0.0f;
    public bool isNotification = false;

    [Header("Sprint")]
    public bool isSprint = false;
    public float sprintTimer = 0.0f;
    public float sprintRest = 0.0f;
    public float sprintSpeed = 0.0f;

    [Header("Desert")]
    public bool canVibrationTool;
    public List<GameObject> desertPoints;
    public List<Vector3> desertPointsVectors;
    public GameObject vibrationSign;
    public float vibrationMinDistance = 35.0f;
    public float vibrationSignTimer = 0.0f; 
    public bool isVibrationSignOn = false;    
    public bool desertKey = false;

    [Header("Forest")]
    public bool canSoundTool;
    public AudioSource mushroomsAudio1;
    public AudioSource mushroomsAudio2;
    public AudioSource mushroomsAudio3;
    public AudioSource mushroomsAudio4;
    public AudioSource mushroomsAudio5;
    private float soundTimer = 0.0f;
    private bool isSoundReady = false;
    public bool forestKey = false;
    private AudioSource soundToolAudio;

    [Header("Dungeon")]
    public GameObject dungeonPuzzle;
    public List<Toggle> puzzleBtns;
    public bool puzzleSuccess = false;
    public bool isDungeonPuzzle = false;
    public GameObject lever1;
    public bool isLever1Active = false;
    private bool isLever1Rotated = false;
    public GameObject lever2;
    public bool isLever2Active = false;
    private bool isLever2Rotated = false;
    public GameObject lever3;
    public bool isLever3Active = false;
    private bool isLever3Rotated = false;    
    public GameObject leftDoor;
    public GameObject rightDoor;
    public GameObject elevator;
    public Vector3[] elevatorPoints;
    public int elevatorPointNum = 0;
    private Vector3 currentTarget;
    public float tolerance;
    public float elevatorSpeed;
    public float delayTime;
    private float delayStart;
    public bool automatic;
    public GameObject dungeonAccessDoors;

    [Header("Others")]
    public GameObject flagPrefab;
    public bool isDesertCristalTaken = false;
    public GameObject desertCristalCenter;
    public bool isForestCristalTaken = false;
    public GameObject forestCristalCenter;
    public bool isDungeonCristalTaken = false;
    public GameObject dungeonCristalCenter;
    public bool isSkycityCristalTaken = false;
    public GameObject skycityCristalCenter;

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

        soundToolAudio = GetComponent<AudioSource>();

        if(elevatorPoints.Length > 0)
        {
            currentTarget = elevatorPoints[0];
        }

        tolerance = 0.1f;

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
            if (Input.GetKeyDown("space"))
            {
                freeClimb.isClimbing = false;
                EnableController();
                freeClimb.a_hook.enabled = false;
            }
            freeClimb.Tick(Time.deltaTime);
            return;
        }

        onGround = OnGround();
        if (keepOffGround)
        {
            if (Time.realtimeSinceStartup - savedTime > 0.5f)
            {
                keepOffGround = false;
            }
        }

        Jump();

        if (!onGround && !keepOffGround)
        {
            if (timer > 1.0f)
            {
                if (!climbOff && canClimb)
                {
                    isClimbing = freeClimb.CheckForClimb();
                }

            }
            if (isClimbing)
            {
                DisableController();
            }

            if (climbOff)
            {
                if (Time.realtimeSinceStartup - climbTimer > 1)
                {
                    climbOff = false;
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.M) && canMap)
        {
            OpenMap();
        }

        if (Input.GetKeyDown(KeyCode.R) && canVibrationTool)
        {
            if (!isVibrationSignOn)
            {
                CheckNearVibration();
                isVibrationSignOn = true;
            }
        }

        if (Input.GetKeyDown(KeyCode.T) && canSoundTool)
        {
            soundToolAudio.Play();
            isSoundReady = true;
        }

        if (isSoundReady)
        {
            soundTimer += Time.deltaTime;
        }

        if(soundTimer >= 1.0f)
        {
            PlayMushrooms();
            isSoundReady = false;
            soundTimer = 0.0f;
        }

        if (isVibrationSignOn)
        {
            vibrationSignTimer += Time.deltaTime;
        }
        if (vibrationSignTimer >= 5.0f)
        {
            isVibrationSignOn = false;
            vibrationSignTimer = 0.0f;
            vibrationSign.SetActive(false);
        }

        if (Input.GetKeyDown(KeyCode.F) && canLevitate)
        {
            if (isLevitating)
            {
                rb.useGravity = true;
                isLevitating = false;
            }
            else
            {
                rb.useGravity = false;
                isLevitating = true;
            }
        }

        if (isDungeonPuzzle)
        {
            CheckDungeonPuzzle();
        }

        if (puzzleSuccess)
        {
            if (elevator.transform.position != currentTarget)
            {
                MoveElevator();
            }
            else
            {
                UpdateElevatorTarget();
            }
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

        if (notificationTimer > 8.0f)
        {
            isNotification = false;
            mapNotification.SetActive(false);
            winNotification.SetActive(false);
            climbNotification.SetActive(false);
            treasureNotification.SetActive(false);
            desertCristalNotification.SetActive(false);
            desertKeyNotification.SetActive(false);
            desertTreasureNotification.SetActive(false);
            desertMapNotification.SetActive(false);
            desertToolNotification.SetActive(false);
            forestCristalNotification.SetActive(false);
            forestKeyNotification.SetActive(false);
            forestTreasureNotification.SetActive(false);
            forestMapNotification.SetActive(false);
            forestToolNotification.SetActive(false);
            dungeonMapNotification.SetActive(false);
            dungeonTreasureNotification.SetActive(false);
            dungeonCristalNotification.SetActive(false);
            allLeversNotification.SetActive(false);
            skycityMapNotification.SetActive(false);
            skycityTreasureNotification.SetActive(false);
            skycityCristalNotification.SetActive(false);
            skycityToolNotification.SetActive(false);
            notificationTimer = 0.0f;
        }


        if (sprintTimer > 5.0f)
        {
            isSprint = false;
            sprintTimer = 0.0f;
            moveSpeed = 4.0f;
        }
        if (Input.GetKey(KeyCode.LeftShift) && sprintRest > 5.0f)
        {
            isSprint = true;
            sprintTimer += Time.deltaTime;
            moveSpeed = sprintSpeed; ;
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
        if (Physics.Raycast(origin, direction, out hit, 0.41f))
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
        if (collision.gameObject.name == "GoldenChest")
        {
            canClimb = true;
            Destroy(collision.gameObject);
            climbNotification.SetActive(true);
            isNotification = true;
        }
        else if (collision.gameObject.name == "SilverChest")
        {
            canMap = true;
            //currentMap = maps[0];
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
        
        else if (collision.gameObject.name == "note2")
        {
            note2Taken = true;
            Destroy(collision.gameObject);
        }

        else if (collision.gameObject.name == "DesertMapChest")
        {
            desertMapNotification.SetActive(true);
            canDesertMap = true;
            Destroy(collision.gameObject);
            isNotification = true;
        }

        else if (collision.gameObject.name == "SismicToolChest")
        {
            desertToolNotification.SetActive(true);
            canVibrationTool = true;
            Destroy(collision.gameObject);
            isNotification = true;
        }

        else if (collision.gameObject.name == "DesertTreasureChest")
        {
            desertTreasureNotification.SetActive(true);
            Destroy(collision.gameObject);
            isNotification = true;
        }

        else if (collision.gameObject.name == "DesertDungeonAccessChest")
        {
            desertKeyNotification.SetActive(true);
            Destroy(collision.gameObject);
            isNotification = true;
            desertKey = true;
            CheckDungeonAccess();
        }

        else if (collision.gameObject.name == "DesertCristal")
        {
            desertCristalNotification.SetActive(true);
            Destroy(collision.gameObject);
            isNotification = true;
            isDesertCristalTaken = true;
        }

        else if (collision.gameObject.name == "ForestMapChest")
        {
            forestMapNotification.SetActive(true);
            canForestMap = true;
            Destroy(collision.gameObject);
            isNotification = true;
        }

        else if (collision.gameObject.name == "SoundToolChest")
        {
            forestToolNotification.SetActive(true);
            canSoundTool = true;
            Destroy(collision.gameObject);
            isNotification = true;
        }

        else if (collision.gameObject.name == "ForestTreasureChest")
        {
            forestTreasureNotification.SetActive(true);
            Destroy(collision.gameObject);
            isNotification = true;
        }

        else if (collision.gameObject.name == "ForestDungeonAccessChest")
        {
            forestKeyNotification.SetActive(true);
            Destroy(collision.gameObject);
            isNotification = true;
            forestKey = true;
            CheckDungeonAccess();
        }

        else if (collision.gameObject.name == "ForestCristal")
        {
            forestCristalNotification.SetActive(true);
            Destroy(collision.gameObject);
            isNotification = true;
            isForestCristalTaken = true;
        }

        else if (collision.gameObject.name == "DungeonMapChest")
        {
            dungeonMapNotification.SetActive(true);
            canDungeonMap = true;
            Destroy(collision.gameObject);
            isNotification = true;
        }

        else if (collision.gameObject.name == "DungeonTreasureChest")
        {
            dungeonTreasureNotification.SetActive(true);
            Destroy(collision.gameObject);
            isNotification = true;
        }

        else if (collision.gameObject.name == "DungeonCristal")
        {
            isDungeonCristalTaken = true;
            dungeonCristalNotification.SetActive(true);
            Destroy(collision.gameObject);
            isNotification = true;
        }

        else if (collision.gameObject.name == "SkycityMapChest")
        {
            skycityMapNotification.SetActive(true);
            canSkycityMap = true;
            Destroy(collision.gameObject);
            isNotification = true;
        }

        else if (collision.gameObject.name == "SkycityTreasureChest")
        {
            skycityTreasureNotification.SetActive(true);
            Destroy(collision.gameObject);
            isNotification = true;
        }

        else if (collision.gameObject.name == "SkycityCristal")
        {
            isSkycityCristalTaken = true;
            skycityCristalNotification.SetActive(true);
            Destroy(collision.gameObject);
            isNotification = true;
        }

        else if (collision.gameObject.name == "SkycityToolChest")
        {
            skycityToolNotification.SetActive(true);
            Destroy(collision.gameObject);
            isNotification = true;
            canLevitate = true;
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

        else if (other.gameObject.layer == 12)
        {
            gemDesertCounter += 1;
            gemDesertCounterTex.text = gemDesertCounter.ToString() + " / 25";
            Destroy(other.gameObject);
        }

        else if (other.gameObject.layer == 13)
        {
            gemForestCounter += 1;
            gemForestCounterTex.text = gemForestCounter.ToString() + " / 20";
            Destroy(other.gameObject);
        }

        else if (other.gameObject.layer == 14)
        {
            gemDungeonCounter += 1;
            gemDungeonCounterTex.text = gemDungeonCounter.ToString() + " / 25";
            Destroy(other.gameObject);
        }

        else if (other.gameObject.layer == 15)
        {
            gemSkycityCounter += 1;
            gemSkycityCounterTex.text = gemSkycityCounter.ToString() + " / 30";
            Destroy(other.gameObject);
        }

        else if (other.gameObject.name == "OpenPanelTrigger" && !puzzleSuccess)
        {
            isDungeonPuzzle = true;
            dungeonPuzzle.SetActive(true);
            camFollow.isActive = false;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        else if (other.gameObject.name == "DesertCristalTrigger")
        {
            if (isDesertCristalTaken)
            {
                desertCristalCenter.SetActive(true);
            }
        }
        else if (other.gameObject.name == "ForestCristalTrigger")
        {
            if (isForestCristalTaken)
            {
                forestCristalCenter.SetActive(true);
            }
        }
        else if (other.gameObject.name == "DungeonCristalTrigger")
        {
            if (isDungeonCristalTaken)
            {
                dungeonCristalCenter.SetActive(true);
            }
        }
        else if (other.gameObject.name == "SkycityCristalTrigger")
        {
            if (isSkycityCristalTaken)
            {
                skycityCristalCenter.SetActive(true);
            }
        }

        else if (other.gameObject == elevator)
        {
            transform.parent = elevator.transform;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject == elevator)
        {
            transform.parent = null;
        }
    }

    private void OnCollisionStay(Collision collisionInfo)
    {
        if (collisionInfo.gameObject.name == "Lever1Trigger")
        {           
            if (!isLever1Rotated)
            {
                lever1.transform.Rotate(90, 0, 0);
                isLever1Rotated = true;
            }
            isLever1Active = true;
            CheckDungeonLevers();
        }

        else if (collisionInfo.gameObject.name == "Lever2Trigger")
        {
            if (!isLever2Rotated)
            {
                lever2.transform.Rotate(90, 0, 0);
                isLever2Rotated = true;
            }
            isLever2Active = true;
            CheckDungeonLevers();
        }

        else if (collisionInfo.gameObject.name == "Lever3Trigger")
        {
            if (!isLever3Rotated)
            {
                lever3.transform.Rotate(90, 0, 0);
                isLever3Rotated = true;
            }
            isLever3Active = true;
            CheckDungeonLevers();
        }
    }


    public void OpenMap()
    {
        if (isMapOpened)
        {
            camFollow.isActive = true;
            //currentMap.SetActive(false);
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

            valleyMapBtn.SetActive(false);
            desertMapBtn.SetActive(false);
            forestMapBtn.SetActive(false);
            dungeonMapBtn.SetActive(false);
            skycityMapBtn.SetActive(false);

            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;

            //drawManager.drawingAllowed = false;
        }
        else
        {
            camFollow.isActive = false;
            //currentMap.SetActive(true);
            isMapOpened = true;
            Cursor.visible = true;
            mainCamera.tag = "Untagged";
            mapCamera.SetActive(true);
            mapCamera.tag = "MainCamera";
            mainCamera.SetActive(false);
            compassObj.SetActive(true);
            note1Toggle.SetActive(true);
            valleyMapBtn.SetActive(true);

            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

            if (note2Taken)
            {
                note2Toggle.SetActive(true);
            }
            if (canDesertMap)
            {
                desertMapBtn.SetActive(true);
            }
            if (canForestMap)
            {
                forestMapBtn.SetActive(true);
            }
            if (canDungeonMap)
            {
                dungeonMapBtn.SetActive(true);
            }
            if (canSkycityMap)
            {
                skycityMapBtn.SetActive(true);
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
    public void OpenValleyMap()
    {
        //currentMap = maps[0];
        mapCamera.transform.GetChild(0).gameObject.SetActive(true);
        mapCamera.transform.GetChild(3).gameObject.SetActive(false);
        mapCamera.transform.GetChild(2).gameObject.SetActive(false);
        mapCamera.transform.GetChild(1).gameObject.SetActive(false);
        note1Toggle.SetActive(true);
        if (note2Taken)
        {
            note2Toggle.SetActive(true);
        }
    }
    public void OpenDesertMap()
    {
        //currentMap.SetActive(false);
        //currentMap = maps[1];
        mapCamera.transform.GetChild(0).gameObject.SetActive(false);
        mapCamera.transform.GetChild(3).gameObject.SetActive(false);
        mapCamera.transform.GetChild(2).gameObject.SetActive(false);
        mapCamera.transform.GetChild(1).gameObject.SetActive(true);

        note1Obj.SetActive(false);
        note1Toggle.SetActive(false);
        note2Obj.SetActive(false);
        note2Toggle.SetActive(false);
        //currentMap.SetActive(true);
    }

    public void OpenForestMap()
    {
        //currentMap.SetActive(false);
        //currentMap = maps[1];
        mapCamera.transform.GetChild(0).gameObject.SetActive(false);
        mapCamera.transform.GetChild(3).gameObject.SetActive(false);
        mapCamera.transform.GetChild(2).gameObject.SetActive(true);
        mapCamera.transform.GetChild(1).gameObject.SetActive(false);

        note1Obj.SetActive(false);
        note1Toggle.SetActive(false);
        note2Obj.SetActive(false);
        note2Toggle.SetActive(false);
        //currentMap.SetActive(true);
    }

    public void OpenDungeonMap()
    {
        //currentMap.SetActive(false);
        //currentMap = maps[1];   
        mapCamera.transform.GetChild(0).gameObject.SetActive(false);
        mapCamera.transform.GetChild(3).gameObject.SetActive(true);
        mapCamera.transform.GetChild(2).gameObject.SetActive(false);
        mapCamera.transform.GetChild(1).gameObject.SetActive(false);

        note1Obj.SetActive(false);
        note1Toggle.SetActive(false);
        note2Obj.SetActive(false);
        note2Toggle.SetActive(false);
        //currentMap.SetActive(true);
    }

    private void CheckNearVibration()
    {
        bool foundVibration = false;
        foreach (GameObject go in desertPoints)
        {
            float dis = Vector3.Distance(go.transform.position, transform.position);
            Vector3 vec = go.transform.position - transform.position;

            if (vec.magnitude <= vibrationMinDistance)
            {
                foundVibration = true;
                desertPointsVectors.Add(vec);               
                Debug.Log(vec.magnitude);
            }
        }

        if (foundVibration)
        {
            vibrationSign.SetActive(true);
            vibrationSign.transform.forward = CheckClosestVibration();
            foundVibration = false;
            desertPointsVectors.Clear();
        }
    }

    private Vector3 CheckClosestVibration()
    {
        Vector3 finalVec;
        finalVec = Vector3.zero;

        int i = 0;

        List<float> dis = new List<float>();

        finalVec = desertPointsVectors[0];

        foreach (Vector3 vec2 in desertPointsVectors)
        {
            if (vec2.magnitude <= finalVec.magnitude)
            {
                finalVec = vec2;
            }
            /*
            dis.Add(vec2.magnitude);

            foreach (float dis2 in dis)
            {
                if (finalVec.magnitude <= dis2)
                {
                    finalVec = vec2;
                }
                i++;
            }
            */
        }            

        return finalVec;
    }

    public void ExitPuzzlePanel()
    {
        isDungeonPuzzle = false;
        dungeonPuzzle.SetActive(false);
        camFollow.isActive = true;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void CheckDungeonPuzzle()
    {
        if (!puzzleBtns[0].isOn && !puzzleBtns[1].isOn && puzzleBtns[2].isOn && !puzzleBtns[3].isOn && puzzleBtns[4].isOn && !puzzleBtns[5].isOn && puzzleBtns[6].isOn && !puzzleBtns[7].isOn)
        {
            puzzleSuccess = true;
            ExitPuzzlePanel();
        }
    }

    private void CheckDungeonLevers()
    {
        if (isLever1Active && isLever2Active && isLever3Active)
        {
            allLeversNotification.SetActive(true);
            isNotification = true;
            Destroy(leftDoor);
            Destroy(rightDoor);
        }
    }

    private void PlayMushrooms()
    {
        mushroomsAudio1.Play();
        mushroomsAudio2.Play();
        mushroomsAudio3.Play();
        mushroomsAudio4.Play();
        mushroomsAudio5.Play();
    }

    public void MoveElevator()
    {
        Vector3 heading = currentTarget - elevator.transform.position;
        elevator.transform.position += (heading / heading.magnitude) * elevatorSpeed * Time.deltaTime;
        if(heading.magnitude < tolerance)
        {
            elevator.transform.position = currentTarget;
            delayStart = Time.deltaTime;
        }
    }
    public void UpdateElevatorTarget()
    {
        if(automatic)
        {
            if(Time.time - delayStart > delayTime)
            {
                NextPlatform();
            }
        }
    }

    public void NextPlatform()
    {
        elevatorPointNum++;
        if(elevatorPointNum >= elevatorPoints.Length)
        {
            elevatorPointNum = 0;
        }
        currentTarget = elevatorPoints[elevatorPointNum];
    }

    public void CheckDungeonAccess()
    {
        if (desertKey && forestKey)
        {
            dungeonAccessDoors.SetActive(false);
        }
    }
}