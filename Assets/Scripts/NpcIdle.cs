using System;
using UnityEngine;
using UnityEngine.Serialization;

public class NpcIdle : MonoBehaviour
{
    [SerializeField] private string quest1Instructions = "We need # beetles to finish our stew! Maybe check with our neighbours?";
    [SerializeField] private string quest1Success = "Look! Everyone came to our party to thank you for helping them!";
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
            var text = quest1Instructions.Replace("#", (GameManager.Instance.quest1BeetlesToCollect - GameManager.Instance.Quest1Progress).ToString());
            UIManager.Instance.SetSubtitle(GameManager.Instance.Completed ? quest1Success : text);
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
            var text = quest1Instructions.Replace("#", (GameManager.Instance.quest1BeetlesToCollect - GameManager.Instance.Quest1Progress).ToString());
            UIManager.Instance.SetSubtitle(GameManager.Instance.Completed ? quest1Success : text);
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
