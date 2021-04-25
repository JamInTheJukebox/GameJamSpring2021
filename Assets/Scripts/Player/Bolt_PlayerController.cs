using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bolt_PlayerController : Bolt.EntityBehaviour<IMasterPlayerState>
{
    public float speed = 3f;
    private float OriginalSpeed;
    // Update is called once per frame
    [Header("Basic Movement")]
    private CharacterController char_Controller;
    [SerializeField] Transform Cam;
    [SerializeField] float TurnSmoothTime = 0.1f;
    float turnSmoothVelocity;                   // temporary variable.
    [Header("Jumping")]
    [SerializeField] float Gravity = -9.81f;
    [SerializeField] float Jump_Velocity = 4;
    Vector3 Current_Y_Velocity;
    [SerializeField] Transform GroundCheck;
    [SerializeField] float GroundCheckRadius = 5;
    [SerializeField] LayerMask groundMask = 8;
    [Tooltip("Layer of platform that is falling.")]
    [SerializeField] LayerMask FallingMask = 10;
    [SerializeField] CapsuleCollider TemporaryCollider;     // temporary collider to attach when falling to the black hole.
    [Header("Dependencies")]
    [SerializeField] PlayerAnimation playerAnim;
    [SerializeField] WeaponManager WepManager;
    [Header("Camera Shake")]
    public ScreenShakeController CameraScreenShake;
    public float WeakIntensity = 4;
    public float StrongIntensity = 7.5f;
    public float WeakShakeTimer = 0.2f;
    public float StrongShakeTimer = 0.4f;
    bool isGrounded;
    bool isParented = false;        // called if you are on a falling platform
    Vector3 SpawnPosition = new Vector3(0,10,0);
    bool teleporting;
    private bool m_stunned = false;
    public bool stunned {
        get { return m_stunned; }
        set
        {
            if(value != m_stunned)
            {
                m_stunned = value;
                // start i frame count here.
                if (!m_stunned)
                {
                    Invoke("Reset_I_Frames",0.3f);
                }
            }
        }
    }

    private void Reset_I_Frames()
    {
        print("No more I frames!");
        i_frames_Active = false;
    }
    [HideInInspector] public bool i_frames_Active;      // when you are hit, there is a frame of time at which you CANNOT and SHALL NOT be hit.

    [Header("Debug Tools")]
    public bool DrawGroundCheck;

    
    public override void Attached() // start.
    {
        char_Controller = GetComponent<CharacterController>();
        state.SetTransforms(state.PlayerTransform, transform);
    }

    public Transform GetGroundCheckTransform()
    {
        return GroundCheck;
    }

    // void update on owner's computer.
    public override void SimulateOwner()
    {
        if (GameUI.UserInterface == null) { return; }       // when clients join the game, userinterface is sometimes not observed.
        if (!entity.IsOwner) { return; }
        if (teleporting) { return; }    // do not move the player if they are stunned.
        // if is grounded and you are not attacking, do moveplayer. Do not attack if you are NOT on the ground.
        MovePlayer();
        HandleYAxis();      // includes gravity and Jumping;
        if (Input.GetKeyDown(KeyCode.B))
        {
            MinorDamage();
        }
        if (Input.GetKeyDown(KeyCode.N))
        {
            MajorDamage();
        }
    }

    private void HandleYAxis()
    {
        isGrounded = Physics.CheckSphere(GroundCheck.position, GroundCheckRadius, groundMask);

        Current_Y_Velocity.y += Gravity * BoltNetwork.FrameDeltaTime;   // acceleration is always pointing down.

        if (isGrounded && Current_Y_Velocity.y <= 0) {
            Current_Y_Velocity.y = -2f;             // if you are grounded, keep moving down at a velocity of 2.

            if (Input.GetButtonDown("Jump") && !WepManager.IsAttacking && !stunned)        // if you are grounded and you press jump, JUMP!
            {
                if (GameUI.UserInterface.Paused) { return; }            // do not move if paused.
                Current_Y_Velocity.y = Mathf.Sqrt(Jump_Velocity * -2f * Gravity);
            }
        }
        if (!isGrounded && !WepManager.IsAttacking && !stunned)
        {
            if (Current_Y_Velocity.y > 0)
            {
                playerAnim.ChangeAnimation(AnimationTags.JUMP);
            }
            else if(Current_Y_Velocity.y < 0)
            {
                playerAnim.ChangeAnimation(AnimationTags.FALL);
            }
        }


        Current_Y_Velocity.y = Mathf.Clamp(Current_Y_Velocity.y, -20f, 20f);
        char_Controller.Move(Current_Y_Velocity * BoltNetwork.FrameDeltaTime);
        
    }

    public bool CheckGrounded()
    {
        return isGrounded;
    }
    public bool CheckStunned()
    {
        return stunned;
    }

    void MovePlayer()
    {
        if (GameUI.UserInterface.Paused | stunned) { return; }            // do not move if paused.
        float hor = Input.GetAxisRaw(Axis.HORIZONTAL);
        float vert = Input.GetAxisRaw(Axis.VERTICAL);
        Vector3 playerMovement = new Vector3(hor, 0f, vert).normalized;
        if(playerMovement.sqrMagnitude >= 0.01f)
        {
            float targetAngle = Mathf.Atan2(playerMovement.x, playerMovement.z) * Mathf.Rad2Deg + Cam.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle,ref turnSmoothVelocity, TurnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0);
            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            moveDir = moveDir.normalized;
            moveDir *= speed * BoltNetwork.FrameDeltaTime;
            if (WepManager.IsAttacking)
            {
                moveDir *= WepManager.WeaponSpeedMultiplier;        // move slightly slower when attacking.
            }

            char_Controller.Move(moveDir);
            if(isGrounded && !WepManager.IsAttacking)
                    playerAnim.ChangeAnimation(AnimationTags.WALK);
            }
        else if (!WepManager.IsAttacking && playerMovement.sqrMagnitude <= 0.01f && char_Controller.velocity.sqrMagnitude <= 0.01f)     // if there is no input and we are moving slightly slow, change to the idle animation. Do not change until you are no longer attacking
        {
            playerAnim.ChangeAnimation(AnimationTags.IDLE);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (DrawGroundCheck)
        {
            Gizmos.color = (isGrounded) ? Color.green : Color.red;
            Gizmos.DrawSphere(GroundCheck.position, GroundCheckRadius);
 
        }
    }
    #region StunningPlayer

    public void MinorDamage()           // called only when punched.
    {
        stunned = true;
        CameraScreenShake.Shake(WeakIntensity, WeakShakeTimer);
        playerAnim.ChangeAnimation(AnimationTags.MINORDAMAGE);
    }
    public void MajorDamage()           // called when hit with weapon or trap.
    {
        stunned = true;
        CameraScreenShake.Shake(StrongIntensity, StrongShakeTimer);
        playerAnim.ChangeAnimation(AnimationTags.MAJORDAMAGE);
    }
    public void UndoStun()
    {
        stunned = false;
    }

    #endregion
    public void MoveToGameRoom()
    {
        Vector3 spawnPos = SpawnPositionManager.instance.GameSpawnPosition.position;
        spawnPos += new Vector3(UnityEngine.Random.Range(-8, 8), 1, UnityEngine.Random.Range(-8, 8));
        transform.position = spawnPos;
    }               // teleport to game room when the game starts

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == Tags.GRAVITYFIELD_TAG && entity.IsOwner)
        {
            print("GETTING SUCCED!");
            LoseMovementPattern();
        }

        else if(other.tag == Tags.BUTTON_TAG)      // if you enter a button tag object and are moving down, push the button down and claim ownership.
        {
            other.GetComponentInParent<GuardedTilePlacement>().ClaimTile(entity);
        }
    }           // guard tile

    private void OnTriggerExit(Collider other)
    {
        if (!entity.IsOwner) { return; }
        if (other.tag == Tags.GRAVITYFIELD_TAG)
        {
            print("FREED! YOU ALSO LOST THE GAME!");
            ReturnMovementPattern();
        }
    }

    public void LoseMovementPattern()           // when entering the black hole,  players will  not be able to move.
    {
        char_Controller.enabled = false;
        if(!GetComponent<Rigidbody>())
            gameObject.AddComponent<Rigidbody>();
        TemporaryCollider.enabled = true;
    }

    public void ReturnMovementPattern()
    {
        char_Controller.enabled = true;
        var vel = GetComponent<Rigidbody>();
        if (vel)
        {
            vel.velocity = new Vector3(0, -1, 0);
            Destroy(GetComponent<Rigidbody>());
        }
        TemporaryCollider.enabled = false;
    }

    public void Teleport(Vector3 newPosition)
    {
        if(!entity.IsOwner) { return; }
        char_Controller.enabled = false;
        transform.position = newPosition;
        char_Controller.enabled = true;
    }
}
