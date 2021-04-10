using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    [Header("SFX")]
    public AudioSFX Walk;
    public AudioSFX Jump;
    public AudioSFX Bat;
    public float resetTime = 0.16f;
    bool resetWaik;
    public void TestWalk()      // trigger on animation in the future.
    {
        if (resetWaik) { return; }
        resetWaik = true;
        Invoke("ResetTheWalk", resetTime);
        Walk.PlayRandomSFX();
    }
    public void TestJump()
    {
        if (resetWaik) { return; }
        resetWaik = true;
        Invoke("ResetTheWalk", 3f);
        Jump.PlayRandomSFX();
    }

    private void ResetTheWalk()
    {
        resetWaik = false;
    }
}
