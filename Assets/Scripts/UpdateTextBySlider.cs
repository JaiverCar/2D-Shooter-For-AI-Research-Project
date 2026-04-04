using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpdateTextBySlider : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UpdateText(string text)
    {
        Slider echoRadiusSlider = GetComponentInChildren<Slider>();

        TextMeshProUGUI textComponent = GetComponent<TextMeshProUGUI>();

        textComponent.text = text + echoRadiusSlider.value.ToString("F2");
    }
}
