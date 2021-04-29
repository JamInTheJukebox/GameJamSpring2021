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

    public static int NumberOfHats = 10;
    public static int NumberOfEyewear = 2;

    [Header("Hats")]
    public GameObject[] Hats = new GameObject[NumberOfHats];

    public GameObject[] Eyes = new GameObject[NumberOfEyewear];
    public GameObject[] SmallEyes = new GameObject[NumberOfEyewear];

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
        state.AddCallback("HatID", HatCallback);
        state.AddCallback("EyeID", EyeCallback);

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
            limb.material = mat;                    // paint the characters
        }
        if (entity.IsOwner)
        {
            //FindObjectOfType<Inventory>().InitializeInventory(PlayerGraphics.material.color);
            state.UserColorData = mat.color;
        }
        LobbyTiles.AddPlayer(gameObject, PlayerGraphics.material.color,state.UserColor);            // remove this key
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
        //PlayerIcon.SetActive(true);       no longer used.
    }

    public string GetName()
    {
        return state.Username;
    }

    public string GetColor()
    {
        return state.UserColor;
    }

    public Color GetColorData()
    {
        return state.UserColorData;
    }

    public void SetHat_Eye()
    {
        state.HatID = PlayerPrefs.GetInt("HatID");
        state.EyeID = PlayerPrefs.GetInt("EyeID");
    }

    public void HatCallback()
    {
        if (state.HatID == 0)        // do nothing
        {
            return;
        }
        var obj = Hats[state.HatID - 1];
        obj.SetActive(true);
        // move the ui text up or the hat will cover it.
        Vector3 NewPos = NameTextbox.transform.parent.localPosition;
        NewPos.y = obj.GetComponent<TransformObject>().GetNewYPosition();
        NameTextbox.transform.parent.localPosition = NewPos;
    }

    public void EyeCallback()
    {
        if (state.HatID == 0)        // do nothing
        {
            return;
        }
        Eyes[state.EyeID - 1].SetActive(true);          
        if(state.HatID == 1)                // enable small eyewear for small popo head.
        {
            SmallEyes[state.EyeID - 1].SetActive(true);
        }
    }

    public void SetCameraTarget(Camera target)
    {
        Target = target.transform;
    }

    private void OnTriggerEnter(Collider other)
    {
        print(gameObject.name);
    }

    public override void Detached()         // when a user is detached, restore their color back to the list of all colors.
    {
        RestoreColorData();
    }

    private void RestoreColorData()
    {
        if (BoltNetwork.IsServer)
        {
            BoltLog.Info("adding " + GetColor());
            Player_Colors.AddMaterial(GetColor());
        }
    }
}
