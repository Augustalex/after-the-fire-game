using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bridge : MonoBehaviour
{
    public GameObject bridgeToggle;
    
    void Start()
    {
        bridgeToggle.SetActive(false);
    }

    public void Toggle()
    {
        bridgeToggle.SetActive(true);
    }
}
