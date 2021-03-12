using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerPersonalization : Bolt.EntityBehaviour<IMasterPlayerState>
{
    // handles any outfits/colors/ names for the player
    public TextMeshProUGUI NameTextbox;
    public Transform Target;
    public GameObject PlayerIcon;
    public MeshRenderer PlayerGraphics;

    Cinemachine.CinemachineFreeLook PlayerCamera;
    /// <summary>
    /// SYSTEM ERROR:
    /// When playing the game on the same system, unity will update the name if someone else has changed it. So if you try to join a game and someone else changes
    /// the name on the other instance, your name will now be the same as theirs.
    /// </summary>
    public override void Attached()
    {
        if(state.UserColor != null)       // if it is empty, the player just joined
        {
            MaterialCallBack();
        }
        state.AddCallback("UserColor", MaterialCallBack);            // we changed a state. When the state is changed, the server will call the callback on everyone's computer.

    }

    private void MaterialCallBack()
    {
        PlayerGraphics.material = Player_Colors.GetColor(state.UserColor);        // player colors gets any color material based off a string
    }
    private void Update()
    {
        if(Target != null)
        {
            Quaternion TargetPosition = Quaternion.LookRotation(transform.position - Target.transform.position);
            NameTextbox.transform.rotation = TargetPosition;
            PlayerIcon.transform.rotation = TargetPosition;
        }
        NameTextbox.text = state.Username;
    }

    public void SetName()
    {
        state.Username = PlayerPrefs.GetString("username");
        PlayerIcon.SetActive(true);
    }

    public void SetCameraTarget(Camera target)
    {
        Target = target.transform;
    }


}
