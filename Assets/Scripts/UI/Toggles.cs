using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Toggles : MonoBehaviour
{
    public static Toggles Instance; // Singleton

    void Awake()
    {
        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        var gridSystem = GameObject.Find("GridSystem");
        gridSystem.GetComponent<AStarGrid>().drawGrid = false;

        var enemies = FindObjectsOfType<EnemyLogic>();

        foreach (EnemyLogic enemy in enemies)
        {
            enemy.drawAStarPath = false;
        }
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

    public void ToggleDrawAStar()
    {
        var enemies = FindObjectsOfType<EnemyLogic>();

        foreach (EnemyLogic enemy in enemies)
        {
            enemy.drawAStarPath = !enemy.drawAStarPath;
        }
    }

    public void ToggleDrawGrid()
    {
        var gridSystem = GameObject.Find("GridSystem");
        gridSystem.GetComponent<AStarGrid>().drawGrid = !gridSystem.GetComponent<AStarGrid>().drawGrid;
    }
}
