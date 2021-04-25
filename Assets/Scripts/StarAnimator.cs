using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StarAnimator : MonoBehaviour
{
    public void EnableStars()
    {
        gameObject.SetActive(true);
        Invoke("DisableStars", 1.2f);
    }

    public void DisableStars()
    {
        gameObject.SetActive(false);
    }
}
