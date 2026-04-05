using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FlockController : MonoBehaviour
{
    [SerializeField]
    [Range(0.0f, 100.0f)]
    private float cohesionStrength = 0.0f;
    [SerializeField]
    [Range(0.0f, 100.0f)]
    private float separationStrength = 0.0f;
    [SerializeField]
    [Range(0.0f, 100.0f)]
    private float alignmentStrength = 0.0f;
    [SerializeField]
    [Range(0.0f, 100.0f)]
    private float wanderStrength = 0.0f;
    [SerializeField]
    [Range(0.0f, 100.0f)]
    private float tetherStrength = 1.0f;


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

    public float GetCohesionStrength()
    {
        return cohesionStrength;
    }
    public float GetSeparationStrength()
    {
        return separationStrength;
    }
    public float GetAlignmentStrength()
    {
        return alignmentStrength;
    }
    public float GetWanderStrength()
    {
        return wanderStrength;
    }
    public float GetTetherStrength()
    {
        return tetherStrength;
    }
}
