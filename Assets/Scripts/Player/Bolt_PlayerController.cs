﻿using System;
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

    bool isGrounded;
    bool isParented = false;        // called if you are on a falling platform
    Vector3 SpawnPosition = new Vector3(0,10,0);
    bool teleporting;
    private bool stunned = false;
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
        if (stunned | teleporting) { return; }    // do not move the player if they are stunned.
        MovePlayer();
        HandleYAxis();      // includes gravity and Jumping;
    }

    private void HandleYAxis()
    {
        isGrounded = Physics.CheckSphere(GroundCheck.position, GroundCheckRadius, groundMask);

        Current_Y_Velocity.y += Gravity * BoltNetwork.FrameDeltaTime;   // acceleration is always pointing down.

        if (isGrounded && Current_Y_Velocity.y <= 0) {
            Current_Y_Velocity.y = -2f;             // if you are grounded, keep moving down at a velocity of 2.

            if (Input.GetButtonDown("Jump"))        // if you are grounded and you press jump, JUMP!
            {
                if (GameUI.UserInterface.Paused) { return; }            // do not move if paused.
                //GetComponent<PlayerAnimation>().TestJump();
                Current_Y_Velocity.y = Mathf.Sqrt(Jump_Velocity * -2f * Gravity);
            }
        }
      
        Current_Y_Velocity.y = Mathf.Clamp(Current_Y_Velocity.y, -20f, 20f);
        char_Controller.Move(Current_Y_Velocity * BoltNetwork.FrameDeltaTime);
        
    }

    private void HandleFallingPlatforms()// if we are on a falling platform, we want the player to be able to jump
    {
        var Platform = Physics.OverlapSphere(GroundCheck.position, GroundCheckRadius, FallingMask);
        if(Platform.Length == 0) { return; }
        //transform.SetParent(Platform[0].transform);
    }

    void MovePlayer()
    {
        if (GameUI.UserInterface.Paused) { return; }            // do not move if paused.
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
            char_Controller.Move(moveDir);
            //if(isGrounded)
                //GetComponent<PlayerAnimation>().TestWalk();
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

    public void PlayerStunned()     // getting pushed back or setting off a trap. Make this an animation.
    {
        stunned = true;
        Invoke("UndoStun", 2f);
    }

    private void UndoStun()
    {
        stunned = false;
    }

    public void MoveToGameRoom()
    {
        Vector3 spawnPos = SpawnPositionManager.instance.GameSpawnPosition.position;
        spawnPos += new Vector3(UnityEngine.Random.Range(-8, 8), 1, UnityEngine.Random.Range(-8, 8));
        transform.position = spawnPos;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!entity.IsOwner) { return; }
        if (other.tag == Tags.GRAVITYFIELD_TAG)
        {
            print("GETTING SUCCED!");
            LoseMovementPattern();
        }

        else if(other.tag == Tags.BUTTON_TAG)      // if you enter a button tag object and are moving down, push the button down and claim ownership.
        {
            other.GetComponentInParent<GuardedTilePlacement>().ClaimTile(entity);
        }
    }

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
