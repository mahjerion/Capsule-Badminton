using UnityEngine;

public class FrameLimiter : MonoBehaviour
{
    void Awake()
    {
        Application.targetFrameRate = 60;
    }
}
