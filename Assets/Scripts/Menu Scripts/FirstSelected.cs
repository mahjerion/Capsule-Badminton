using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirstSelected : MonoBehaviour
{
    public GameObject FirstObject;

    void OnEnable()
    {
        if (SinglePlayerCamera.isMobile == false)
        {
            UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(FirstObject, null);
        }
    }

    private void Update()
    {
        if (SinglePlayerCamera.isMobile == false && UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject == null)
        {
            Debug.Log("reselecting first input");
            UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(FirstObject, null);
        }
    }
}
