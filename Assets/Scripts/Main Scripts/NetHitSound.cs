using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetHitSound : MonoBehaviour
{
    public Rigidbody shuttle;
    public AudioSource netSound;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.CompareTag("Shuttle"))
        {
            netSound.Play();
            shuttle.velocity /= 2;
        }
    }
}
