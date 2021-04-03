using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test1 : MonoBehaviour
{
    private Animator ButtonAnim;
    private int AnimHashID;
    public Renderer ButtonRender;
    public Material OwnedMaterial;      // material to assign when Player owns the material.
    private void Awake()
    {
        ButtonAnim = GetComponent<Animator>();
        AnimHashID = Animator.StringToHash("Pushed");
    }
    public void ClaimTile()
    {
        print("I am now claimed!");
        ButtonAnim.Play(AnimHashID);
        ButtonRender.material = OwnedMaterial;
    }


}
