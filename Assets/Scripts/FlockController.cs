using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FlockController : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UpdateEchoRadius()
    {
        Slider echoRadiusSlider = GetComponentInChildren<Slider>();

        // get all enemies that have the FlockingLogic script
        var enemies = FindObjectsOfType<FlockingLogic>();
        
        foreach (FlockingLogic enemy in enemies)
        {
            enemy.SetEchoRadius(echoRadiusSlider.value);
        }
    }
}
