using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Anchor : MonoBehaviour
{
    public TextMeshProUGUI labelText;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void UpdateLabel(string text)
    {
        labelText.text = text;
    }
}
