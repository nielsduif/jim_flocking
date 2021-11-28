using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GetValueSlider : MonoBehaviour
{
    Slider s;
    Text t;

    private void Start()
    {
        s = GetComponentInParent<Slider>();
        t = GetComponent<Text>();
    }

    void Update()
    {
        t.text = s.value.ToString();
    }
}
