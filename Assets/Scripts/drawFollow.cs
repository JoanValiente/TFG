﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class drawFollow : MonoBehaviour
{

    // Update is called once per frame
    void Update()
    {
        Vector3 temp = Input.mousePosition;
        temp.z = 10.0f;
        this.transform.position = Camera.main.ScreenToWorldPoint(temp);
    }
}
