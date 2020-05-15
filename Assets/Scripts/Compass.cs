using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Compass : MonoBehaviour
{
    public Vector3 northDirection;
    public Transform player;

    public RectTransform northLayer;


    void Update()
    {
        ChangeNorthDir();
    }

    public void ChangeNorthDir()
    {
        northDirection.z = player.eulerAngles.y;
        northLayer.localEulerAngles = northDirection;
    }
}
