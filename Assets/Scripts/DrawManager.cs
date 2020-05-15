using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawManager : MonoBehaviour
{

    public GameObject drawPrefab;
    public GameObject drawingHolder;
    GameObject trail;
    Plane plane;
    Vector3 startPos;

    public bool drawingAllowed = false;

    public static DrawManager singleton;
    void Awake()
    {
        singleton = this;
    }

    void Start()
    {
        plane = new Plane(Camera.main.transform.forward * -1, this.transform.position);
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && drawingAllowed)
        {
            trail = (GameObject)Instantiate(drawPrefab, this.transform.position, Quaternion.identity, drawingHolder.transform);

            Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            float _dis;
            if(plane.Raycast(mouseRay, out _dis))
            {
                startPos = mouseRay.GetPoint(_dis);
            }
        }
        else if (Input.GetMouseButton(0) && drawingAllowed)
        {
            Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            float _dis;
            if (plane.Raycast(mouseRay, out _dis))
            {
                trail.transform.position = mouseRay.GetPoint(_dis);
            }
        }
    }
}
