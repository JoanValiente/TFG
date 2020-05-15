using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Paintable : MonoBehaviour
{

    public GameObject brush = null;
    public float brushSize = 0.1f;
    public float offset = 0.1f;
    public RenderTexture RTexture;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if(Physics.Raycast(ray, out hit))
            {
                if (brush != null)
                {
                    GameObject go = Instantiate(brush, hit.point + Vector3.up * offset, Quaternion.identity, transform);
                    //go.transform.rotation.ToAngleAxis(x,);                
                    go.transform.localScale = Vector3.one * brushSize;
                    //go.transform.rotation = Quaternion.identity;
                    //go.transform.localEulerAngles.Set(0.0f, 0.0f, 0.0f);
                    go.transform.rotation.SetLookRotation(hit.normal);
                    go.transform.localRotation.Set(0.0f, 0.0f, 0.0f, 0.0f);
                }
            }
        }      
    }

    public void Save()
    {
        StartCoroutine(CoSave());
    }

    private IEnumerator CoSave()
    {
        yield return new WaitForEndOfFrame();
        Debug.Log(Application.dataPath + "/savedImage.png");

        RenderTexture.active = RTexture;

        var texture2D = new Texture2D(RTexture.width, RTexture.height);
        texture2D.ReadPixels(new Rect(0, 0, RTexture.width, RTexture.height), 0, 0);
        texture2D.Apply();

        texture2D.Apply();

        var data = texture2D.EncodeToPNG();

        File.WriteAllBytes(Application.dataPath + "/savedImage.png", data);
    }
}
