using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UtilityAI;

public class HIveMindSignalSlider : MonoBehaviour
{
    private float signalStrength = 0;

    // Start is called before the first frame update
    void Start()
    {
        var hiveMind = FindObjectOfType<HiveMind>();

        GetComponent<Slider>().value = hiveMind.globalSignalStrength * 100.0f;

        signalStrength = hiveMind.globalSignalStrength * 100.0f;
    }

    // Update is called once per frame
    void Update()
    {
        GetComponentInChildren<TextMeshProUGUI>().text = "Hive Mind Signal Strength: " + (int)signalStrength + "%";
    }

    public void SignalStrengthChanged()
    {
        float newSignalStrengthPercent = GetComponent<Slider>().value;

        float newSignalStrengthDecimal = newSignalStrengthPercent / 100.0f;


        var hiveMind = FindObjectOfType<HiveMind>();
        
        hiveMind.SetSignalStrength(newSignalStrengthDecimal);

        signalStrength = newSignalStrengthPercent;
    }

}
