using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Toggles : MonoBehaviour
{
    public static Toggles Instance; // Singleton

    private bool drawAStar = false;
    private bool drawGrid = false;

    void Awake()
    {
        Instance = this;
    }

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

    public bool DrawAStarPaths()
    {
        return drawAStar;
    }

    public void ToggleDrawAStar()
    {
        drawAStar = !drawAStar;
    }

    public bool DrawGrid()
    {
        return drawGrid;
    }

    public void ToggleDrawGrid()
    {
        drawGrid = !drawGrid;
    }
}
