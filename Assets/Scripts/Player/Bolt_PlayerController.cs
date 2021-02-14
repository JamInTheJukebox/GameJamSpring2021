using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bolt_PlayerController : Bolt.EntityBehaviour<IMasterPlayerState>
{
    public float speed = 3f;
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
    bool isGrounded;

    [Header("Debug Tools")]
    public bool DrawGroundCheck;

    public override void Attached() // start.
    {
        char_Controller = GetComponent<CharacterController>();
        state.SetTransforms(state.PlayerTransform, transform);
    }

    // void update on owner's computer.
    public override void SimulateOwner()
    {
        if (!entity.IsOwner) { return; }
        MovePlayer();
        HandleYAxis();      // includes gravity and Jumping;
    }

    private void HandleYAxis()
    {
        isGrounded = Physics.CheckSphere(GroundCheck.position, GroundCheckRadius, groundMask);
        if (isGrounded) { Current_Y_Velocity.y = -2f; }
        Current_Y_Velocity.y += Gravity * BoltNetwork.FrameDeltaTime;
        if(Input.GetButtonDown("Jump") && isGrounded)
        {
            Current_Y_Velocity.y = Mathf.Sqrt(Jump_Velocity * -2f * Gravity);
        }
        char_Controller.Move(Current_Y_Velocity * BoltNetwork.FrameDeltaTime);
        
    }

    void MovePlayer()
    {
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
        }
    }
    private void OnDrawGizmosSelected()
    {
        if (DrawGroundCheck)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(GroundCheck.position, GroundCheckRadius);
        }
    }
}
