using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeetleToggle : MonoBehaviour
{
    public GameObject undone;
    public GameObject done;

    public void IsDone()
    {
        undone.SetActive(false);
        done.SetActive(true);
    }

    public void IsNotDone()
    {
        undone.SetActive(true);
        done.SetActive(false);
    }
}