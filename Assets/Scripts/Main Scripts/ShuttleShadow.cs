using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShuttleShadow : MonoBehaviour
{
    public GameObject shuttleGameObject;
    public GameObject shadowGameObject;
    public Transform shuttleTransform;
    public Transform shadowTransform;

    public Transform redGlow;
    public Transform blueGlow;
    public Transform yellowGlow;
    public Transform greenGlow;

    // Start is called before the first frame update
    void Start()
    {
        shadowGameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        redGlow.position = shuttleTransform.position;
        blueGlow.position = shuttleTransform.position;
        yellowGlow.position = shuttleTransform.position;
        greenGlow.position = shuttleTransform.position;

        // Checks to see if the shuttle is active. If it is, follows shuttle. Otherwise it becomes inactive.
        if (shuttleGameObject.activeSelf)
        {
            shadowGameObject.SetActive(true);
            shadowTransform.position = (new Vector3(shuttleTransform.position.x, 0, shuttleTransform.position.z));
        }
        else
        {
            shadowGameObject.SetActive(false);
        }
    }
}
