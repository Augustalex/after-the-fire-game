using UnityEngine;

public class DeadBlock : MonoBehaviour
{
    void Awake()
    {
        Destroy(gameObject);
    }
}