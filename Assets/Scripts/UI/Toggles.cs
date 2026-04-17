using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Toggles : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ToggleSpectatePlayer()
    {
        var camera = GameObject.Find("Main Camera(Clone)");

        bool value = camera.GetComponent<CameraFollow>().GetCameraSpectatePlayer();

        camera.GetComponent<CameraFollow>().SetCameraSpectatePlayer(!value);
    }
}
