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
    public Bolt_PlayerController PlayerController;
    public StarAnimator StarFX;

    public ParticleSystem HitFX;

    private int m_currentClip;
    public int currentClip
    {
        get { return m_currentClip; }
        set
        {
            if (value != m_currentClip)
            {
                /*
                if(m_currentClip == AnimClips[AnimationTags.CHICKENSLAP] | m_currentClip == AnimClips[AnimationTags.PUNCH] | m_currentClip == AnimClips[AnimationTags.SWING])
                {
                    WepManager.DelayNextAttack();
                }*/
                m_currentClip = value;
                if(entity.IsOwner)
                    state.AnimClip = m_currentClip;
            }
        }
    }


    public override void Attached()
    {
        charAnim = GetComponent<Animator>();
        state.SetAnimator(charAnim);
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
        AnimClips.Add(AnimationTags.TRAPSET, Animator.StringToHash(AnimationTags.TRAPSET));

        #endregion
    }

    public void ChangeAnimation(string clipName)
    {
        currentClip = AnimClips[clipName];
    }
    public void InitiateFist()
    {
        WepManager.EnablePushCollider();
    }
    public void InitiateWeapon()
    {
        WepManager.EnableGenericCollider();
    }
    public void ResetAttack()
    {
        //currentClip = AnimClips[AnimationTags.IDLE];
        WepManager.IsAttacking = true;                  // make sure this variable is true.
        WepManager.DelayNextAttack();
    }

    public void ResetAttackTraps()
    {

    }
    #region Stun
    public void PlayStars()
    {
        PlayHitVFX();
        StarFX.EnableStars();
    }

    public void PlayHitVFX()
    {
        HitFX.Play();
    }

    public void ResetStun()
    {
        PlayerController.UndoStun();
        ResetAttack();      // call this incase the player's attack function was interrupted.
    }
    #endregion
    private void Update()
    {
        charAnim.Play(state.AnimClip);
    }

    #region  SFX

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
    #endregion
}
