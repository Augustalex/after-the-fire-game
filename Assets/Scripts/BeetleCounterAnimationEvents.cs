using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeetleCounterAnimationEvents : MonoBehaviour
{
    public void AnimIncreaseBugWithOne()
    {
        GameManager.Instance.playerInventory.IncreaseBugWithOne();
        this.gameObject.SetActive(false);
    }
}
