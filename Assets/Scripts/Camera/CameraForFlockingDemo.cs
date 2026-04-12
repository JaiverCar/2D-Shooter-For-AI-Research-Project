using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraForFlockingDemo : MonoBehaviour
{
    //Speed for when manually moving the camera
    private float CameraMoveSpeed = 5.0f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.UpArrow)) // move camera up
        {
            transform.position = new Vector3(transform.position.x, transform.position.y + (CameraMoveSpeed * Time.deltaTime), transform.position.z);
        }
        if (Input.GetKey(KeyCode.DownArrow)) // move camera down
        {
            transform.position = new Vector3(transform.position.x, transform.position.y - (CameraMoveSpeed * Time.deltaTime), transform.position.z);
        }
        if (Input.GetKey(KeyCode.RightArrow)) // move camera right
        {
            transform.position = new Vector3(transform.position.x + (CameraMoveSpeed * Time.deltaTime), transform.position.y, transform.position.z);
        }
        if (Input.GetKey(KeyCode.LeftArrow)) // move camera left
        {
            transform.position = new Vector3(transform.position.x - (CameraMoveSpeed * Time.deltaTime), transform.position.y, transform.position.z);
        }
    }
}
