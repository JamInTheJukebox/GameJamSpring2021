using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class ScreenShakeController : Bolt.EntityBehaviour<IMasterPlayerState>
{
    private Cinemachine.CinemachineFreeLook cam;
    Cinemachine.CinemachineBasicMultiChannelPerlin rig0;
    Cinemachine.CinemachineBasicMultiChannelPerlin rig1;
    Cinemachine.CinemachineBasicMultiChannelPerlin rig2;
    private float ShakeTimer;

    public override void Attached()
    {
        if(entity.IsOwner)
        {
            cam = GetComponent<Cinemachine.CinemachineFreeLook>();
            rig0 = cam.GetRig(0).GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
            rig1 = cam.GetRig(1).GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
            rig2 = cam.GetRig(2).GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();

        }
    }

    public void Shake(float intensity, float time)
    {
        rig0.m_AmplitudeGain = intensity;
        rig1.m_AmplitudeGain = intensity;
        rig2.m_AmplitudeGain = intensity;
        rig0.m_FrequencyGain = time;
        rig1.m_FrequencyGain = time;
        rig2.m_FrequencyGain = time;
        ShakeTimer = time;
    }

    public override void SimulateOwner()        // shake camera only on owners screen
    {
        if(cam == null) { return; }
        if(ShakeTimer > 0)
        {
            ShakeTimer -= BoltNetwork.FrameDeltaTime;
            if(ShakeTimer <= 0)                 // stop the screen shake
            {
                rig0.m_FrequencyGain = 0;
                rig1.m_FrequencyGain = 0;
                rig2.m_FrequencyGain = 0;
            }
        }
    }
}
