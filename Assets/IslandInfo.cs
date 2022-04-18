using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IslandInfo : MonoBehaviour
{
    public string islandName = "Unknown Island";

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    public string GetIslandNameFormatted()
    {
        return islandName.Replace("<br>", "\n");
    }
}
