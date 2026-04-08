using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FlockController : MonoBehaviour
{
    [SerializeField]
    [Range(0.5f, 15.0f)]
    private float currentEchoRadius = 15.0f;
    [SerializeField]
    [Range(0.0f, 1.0f)]
    private float separationRange = 0.5f;
    [SerializeField]
    [Range(0.0f, 10.0f)]
    private float cohesionWeight = 1.0f;
    [SerializeField]
    [Range(0.0f, 10.0f)]
    private float separationWeight = 1.0f;
    [SerializeField]
    [Range(0.0f, 10.0f)]
    private float alignmentWeight = 1.0f;
    [SerializeField]
    [Range(0.0f, 10.0f)]
    private float tetherWeight = 1.0f;
    //[SerializeField]
    //[Range(0.0f, 100.0f)]
    //private float wanderStrength = 0.0f;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        var enemies = FindObjectsOfType<FlockingLogic>();

        foreach (FlockingLogic enemy in enemies)
        {
            enemy.SetEchoRadius(currentEchoRadius);
            enemy.SetSeparationRadius(separationRange * currentEchoRadius);
        }
    }

    public void UpdateEchoRadius()
    {
        Slider echoRadiusSlider = GetComponentInChildren<Slider>();

        // get all enemies that have the FlockingLogic script
        var enemies = FindObjectsOfType<FlockingLogic>();

        currentEchoRadius = echoRadiusSlider.value;
    }

    public float GetCohesionWeight()
    {
        return cohesionWeight;
    }
    public float GetSeparationWeight()
    {
        return separationWeight;
    }
    public float GetAlignmentWeight()
    {
        return alignmentWeight;
    }
    public float GetTetherWeight()
    {
        return tetherWeight;
    }
    //public float GetWanderStrength()
    //{
    //    return wanderStrength;
    //}
}
