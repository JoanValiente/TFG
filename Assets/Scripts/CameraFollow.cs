using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public float CameraMoveSpeed = 120.0f;
    public GameObject CameraFollowObj;
    public GameObject mapTransform;
    public Vector3 FollowPos;

    public float clampAngle = 80.0f;
    public float inputSensitivity = 150.0f;
    public GameObject CameraObj;
    public GameObject PlayerObj;
    public float canDinstanceXToPlayer;
    public float canDinstanceYToPlayer;
    public float canDinstanceZToPlayer;
    public float mouseX;
    public float mouseY;
    public float finalInputX;
    public float finalInputZ;
    public float smoothX;
    public float smoothY;
    public float rotX = 0.0f;
    public float rotY = 0.0f;

    public bool isActive = true;

    public static CameraFollow singleton;
    void Awake()
    {
        singleton = this;
    }


    void Start()
    {
        Vector3 rot = transform.localRotation.eulerAngles;
        rotY = rot.y;
        rotX = rot.x;
        //Cursor.lockState = CursorLockMode.Locked;
        //Cursor.visible = false;
    }

    // Update is called once per frame
    void Update()
    {
        float inputX = Input.GetAxis("RightStickHorizontal");
        float inputZ = Input.GetAxis("RightStickVertical");
        mouseX = Input.GetAxis("Mouse X");
        mouseY = Input.GetAxis("Mouse Y");

        finalInputX = inputX + mouseX;
        finalInputZ = inputZ + mouseY;

        rotY += finalInputX * inputSensitivity * Time.deltaTime;
        rotX -= finalInputZ * inputSensitivity * Time.deltaTime;

        rotX = Mathf.Clamp(rotX, -clampAngle, clampAngle);

        Quaternion localRotation = Quaternion.Euler(rotX, rotY, 0.0f);
        if (isActive)
        {
            transform.rotation = localRotation;
        }
        else
        {
            transform.rotation.eulerAngles.Set(-90.0f, 0.0f, 0.0f);
        }
        
    }

    void LateUpdate ()
    {
        if (isActive)
        {
            CameraUpdater();
        }
        else
        {
            transform.position = mapTransform.transform.position;
        }
    }

    void CameraUpdater()
    {
        Transform target = CameraFollowObj.transform;

        float step = CameraMoveSpeed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, target.position, step);
    }
}
