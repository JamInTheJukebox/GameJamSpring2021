using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_LookAtPlayer : MonoBehaviour
{
    Transform Target;
    private void Awake()
    {
        Invoke("SearchForTarget", 0.01f);        // search for target in 0.1f seconds
    }

    void SearchForTarget()
    {
        var Cam = FindObjectOfType<Camera>();
        if (Cam == null)
        {
            Invoke("SearchForTarget", 0.1f);        // search for target in 0.1f seconds
        }
        else
        {
            Target = Cam.transform;
        }
    }

    private void Update()
    {
        if (Target != null)
        {
            Quaternion TargetPosition = Quaternion.LookRotation(transform.position - Target.transform.position);
            transform.rotation = TargetPosition;
        }
    }
}
