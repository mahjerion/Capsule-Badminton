using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]

public class Shot
{
    // yForce is now actually the angle
    public float yForce;
    // xForce is the angle on drop/net shots (temp)
    public float xForce;
}
public class ShotManager : MonoBehaviour
{
    public Shot clear;
    public Shot drive;
    public Shot drop;
    public Shot shortServe;
    public Shot longServe;
}
