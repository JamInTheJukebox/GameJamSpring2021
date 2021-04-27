using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum e_transformMode
{
    Rotate, PlayerUI
}
public class TransformObject : MonoBehaviour
{
    public e_transformMode TransformMode = e_transformMode.Rotate;
    [Header("Rotate")]
    public float xAngle = 0.1f;
    public float yAngle;
    public float zAngle;

    // For Player UI only
    [Header("PlayerUI")]
    public float newYPosition = 4.128f;
    void Update()
    {
        if(gameObject.activeSelf && TransformMode == e_transformMode.Rotate)           // if the gameobject is not active, do not do anything.
            transform.Rotate(xAngle, yAngle, zAngle, Space.World);
    }

    public float GetNewYPosition()          // moves UI above the hat.
    {
        return newYPosition;
    }
}
