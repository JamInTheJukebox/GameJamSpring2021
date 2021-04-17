using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerPersonalization : Bolt.EntityBehaviour<IMasterPlayerState>
{
    // handles any outfits/colors/ names for the player
    public TextMeshProUGUI NameTextbox;
    public Transform Target;            // used by the camera to look at the characters UI.
    public GameObject PlayerIcon;
    public Renderer PlayerGraphics;
    public Renderer[] PlayerLimbs = new Renderer[10];
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
        Invoke("SearchForTarget", 0.01f);        // search for target in 0.1f seconds
    }

    void SearchForTarget()
    {
        var Cam = FindObjectOfType<Camera>();
        if(Cam == null)
        {
            Invoke("SearchForTarget", 0.1f);        // search for target in 0.1f seconds
        }
        else
        {
            SetCameraTarget(Cam);
        }
    }
    private void MaterialCallBack()
    {
        var mat = Player_Colors.GetColor(state.UserColor);        // player colors gets any color material based off a string
        foreach(var limb in PlayerLimbs)
        {
            limb.material = mat;
        }
        if (entity.IsOwner)
        {
            FindObjectOfType<Inventory>().InitializeInventory(PlayerGraphics.material.color);
        }
        LobbyTiles.AddPlayer(gameObject, PlayerGraphics.material.color);
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

    public string GetName()
    {
        return state.Username;
    }

    public void SetCameraTarget(Camera target)
    {
        Target = target.transform;
    }

    private void OnTriggerEnter(Collider other)
    {
        print(gameObject.name);
    }
}
