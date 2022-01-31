using System;
using UnityEngine;

public class NpcIdle : MonoBehaviour
{
    private string text = "We need 4 beetles to finish our stew! Maybe check with our neighbours?";
    private string text2 = "Look! Everyone came to our party to thank you for helping them!";
    private bool _firstTime = true;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && _firstTime)
        {
            _firstTime = false;
            UIManager.Instance.SetSubtitle("~ Press E to talk ~");
        }
        
        if (other.CompareTag("PlayerHog"))
        {
            UIManager.Instance.SetSubtitle(GameManager.Instance.completed ? text2 : text);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("PlayerHog"))
        {
            other.GetComponentInParent<PlayerModeController>().CloseToNpc();
        }

        if (other.CompareTag("PlayerHog"))
        {
            UIManager.Instance.SetSubtitle(GameManager.Instance.completed ? text2 : text);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("PlayerHog"))
        {
            UIManager.Instance.ClearSubtitle();
        }
    }
}
