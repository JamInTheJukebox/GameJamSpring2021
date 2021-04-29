using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class SelectRandomPremise : MonoBehaviour
{
    public Image SpriteHolder;
    public List<Sprite> RandomPremises = new List<Sprite>();
    private Animator premiseAnimator;

    private void Start()
    {
        premiseAnimator = GetComponent<Animator>();
        SpriteHolder.sprite = RandomPremises[Random.Range(0,RandomPremises.Count)];
        Invoke("PlayPremise", 4f);
    }

    private void PlayPremise()
    {
        premiseAnimator.enabled = true;
    }
}
