using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimation : Bolt.EntityBehaviour<IMasterPlayerState>
{
    [Header("SFX")]
    public AudioSFX Walk;
    public AudioSFX Jump;
    public AudioSFX Bat;
    private Animator charAnim;
    Dictionary<string, int> AnimClips = new Dictionary<string, int>();
    [Header("Gameobjects")]
    public WeaponManager WepManager;
    private int m_currentClip;
    public int currentClip
    {
        get { return m_currentClip; }
        set
        {
            if (value != m_currentClip)
            {
                m_currentClip = value;
                charAnim.Play(m_currentClip);
            }
        }
    }
    public override void Attached()
    {
        charAnim = GetComponent<Animator>();
        #region clips
        AnimClips.Add(AnimationTags.IDLE, Animator.StringToHash(AnimationTags.IDLE));
        AnimClips.Add(AnimationTags.MAJORDAMAGE, Animator.StringToHash(AnimationTags.MAJORDAMAGE));
        AnimClips.Add(AnimationTags.MINORDAMAGE, Animator.StringToHash(AnimationTags.MINORDAMAGE));
        AnimClips.Add(AnimationTags.PUNCH, Animator.StringToHash(AnimationTags.PUNCH));
        AnimClips.Add(AnimationTags.TRIP, Animator.StringToHash(AnimationTags.TRIP));
        AnimClips.Add(AnimationTags.SWING, Animator.StringToHash(AnimationTags.SWING));
        AnimClips.Add(AnimationTags.CHICKENSLAP, Animator.StringToHash(AnimationTags.CHICKENSLAP));
        AnimClips.Add(AnimationTags.FALL, Animator.StringToHash(AnimationTags.FALL));
        AnimClips.Add(AnimationTags.WALK, Animator.StringToHash(AnimationTags.WALK));
        AnimClips.Add(AnimationTags.JUMP, Animator.StringToHash(AnimationTags.JUMP));
        #endregion
    }

    public void ChangeAnimation(string clipName)
    {
        currentClip = AnimClips[clipName];
    }

    public void PlayWalkSFX()
    {
        Walk.PlayRandomSFX();
    }

    public void PlayJumpSFX()
    {
        Jump.PlayRandomSFX();
    }

    public void ContinueFall()      // used to skip the beginning of the fall animation.
    {
        charAnim.Play(AnimationTags.FALL, 0, 0.24f);
    }

    public void ResetAttack()
    {
        WepManager.IsAttacking = false;
        WepManager.StartCoroutine(WepManager.DelayNextAttack());
    }
}
