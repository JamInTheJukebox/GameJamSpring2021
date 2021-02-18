using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonCamera : Bolt.EntityBehaviour<IMasterPlayerState>
{
    public float RotationSpeed = 1f;
    public Transform Target, Player;
    float MouseX, MouseY;
    public  Cinemachine.CinemachineFreeLook CameraSetting;        // invert x and y axis here.

    public float MinAngleY = -20, MaxAngleY = 60;


    public override void Attached()
    {
        if (entity.IsOwner)
        {
            Invoke("SetCamera", 1f);
        }
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void SetCamera()
    {
        FindObjectOfType<GameUI>().CameraSettings = CameraSetting;      
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = (Cursor.lockState == CursorLockMode.Locked) ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = !Cursor.visible;
        }

    }

    private void LateUpdate()
    {
        //CamControl();
    }

    void CamControl()       // UNUSED
    {
        MouseX += Input.GetAxis(MouseAxis.MOUSE_X) * RotationSpeed;
        MouseY -= Input.GetAxis(MouseAxis.MOUSE_Y) * RotationSpeed;

        MouseY = Mathf.Clamp(MouseY, MinAngleY, MaxAngleY);
        transform.LookAt(Target);
        Target.rotation = Quaternion.Euler(MouseY, MouseX, 0);
        Player.rotation = Quaternion.Euler(0, MouseX, 0);
    }
}
