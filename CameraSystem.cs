using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class CameraSystem : MonoBehaviour
{
    [SerializeField] Camera cam;
    Transform camTransform;

    // Start is called before the first frame update
    void Start()
    {
        cam = GameObject.Find("MainCamera").GetComponent<Camera>();
        camTransform = cam.GetComponent<Transform>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.A))
        {
            Debug.Log("A");
            PanLeft();
        }
        if (Input.GetKey(KeyCode.D))
        {
            Debug.Log("D");
            PanRight();
        }
        if (Input.GetKey(KeyCode.W))
        {
            Debug.Log("W");
            PanUp();
        }
        if (Input.GetKey(KeyCode.S))
        {
            Debug.Log("S");
            PanDown();
        }
    }

    public void ZoomOut()
    {
        if (cam.orthographicSize > 4.0)
        {
            cam.orthographicSize--;
        }
    }

    public void ZoomIn()
    {
        if (cam.orthographicSize < 15.0)
        {
            cam.orthographicSize++;
        }
    }

    public void PanLeft()
    {
        camTransform.Translate(new Vector3(-0.05f, 0, 0), Space.World);
    }

    public void PanRight()
    {
        camTransform.Translate(new Vector3(0.05f, 0, 0), Space.World);
    }
    public void PanUp()
    {
        camTransform.Translate(new Vector3(0, 0.05f, 0), Space.World);
    }
    public void PanDown()
    {
        camTransform.Translate(new Vector3(0, -0.05f, 0), Space.World);
    }
}
