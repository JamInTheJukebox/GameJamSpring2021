using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[BoltGlobalBehaviour(BoltNetworkModes.Server)]
public class GlobalEventManager : Bolt.GlobalEventListener
{
    List<string> PlayerMaterials = new List<string>();

    public override void OnEvent(GetPlayerPersonalizationEvent evnt)        // HANDLE ALL COSMETICS HERE!!
    {
        if (PlayerMaterials.Count == 0)     // no materials assigned.        Get a  list of all the possible colors. Remove them as you go.
        {
            PlayerMaterials.Add(Color_Tags.BLACK); PlayerMaterials.Add(Color_Tags.BLUE);
            PlayerMaterials.Add(Color_Tags.DARKGREEN); PlayerMaterials.Add(Color_Tags.DARKRED);
            PlayerMaterials.Add(Color_Tags.GREEN); PlayerMaterials.Add(Color_Tags.LIGHTBLUE);
            PlayerMaterials.Add(Color_Tags.ORANGE); PlayerMaterials.Add(Color_Tags.PINK);
            PlayerMaterials.Add(Color_Tags.PURPLE); PlayerMaterials.Add(Color_Tags.RED);
        }
        int chosenIndex = Random.Range(0, PlayerMaterials.Count);
        string chosenColor = PlayerMaterials[chosenIndex];
        PlayerMaterials.RemoveAt(chosenIndex);        // index has been chosen.

        var Set_Evnt = SetPlayerPersonalizationEvent.Create();
        Set_Evnt.PlayerEntity = evnt.PlayerEntity;
        Set_Evnt.PlayerColor = chosenColor;
        Set_Evnt.Send();
    }

    public override void OnEvent(SetPlayerPersonalizationEvent evnt)        // assign the state of each user color to the chosen player color.
    {
        if (evnt.PlayerEntity.IsOwner)
        {
            Debug.LogWarning(evnt.PlayerColor);
            evnt.PlayerEntity.GetState<IMasterPlayerState>().UserColor = evnt.PlayerColor;      // changes the state only on the owner's screen
        }
        //evnt.PlayerEntity.transform.Find("PlayerGFX").GetComponent<MeshRenderer>().material = Player_Colors.GetColor(evnt.PlayerColor);
    }
}
[BoltGlobalBehaviour(BoltNetworkModes.Client)]
public class GlobalEventManager_Everyone : Bolt.GlobalEventListener
{
    public override void OnEvent(SetPlayerPersonalizationEvent evnt)        // assign the state of each user color to the chosen player color.
    {
        if (evnt.PlayerEntity.IsOwner)
        {
            Debug.LogWarning(evnt.PlayerColor);
            evnt.PlayerEntity.GetState<IMasterPlayerState>().UserColor = evnt.PlayerColor;      // changes the state only on the owner's screen
        }
        //evnt.PlayerEntity.transform.Find("PlayerGFX").GetComponent<MeshRenderer>().material = Player_Colors.GetColor(evnt.PlayerColor);
    }
}
public static class Color_Tags
{
    public const string Directory = "Materials/PlayerColors/";
    public const string BLACK = "Black";
    public const string BLUE = "Blue";
    public const string DARKGREEN = "DarkGreen";
    public const string DARKRED = "DarkRed";
    public const string GREEN = "Green";
    public const string LIGHTBLUE = "LightBlue";
    public const string ORANGE = "Orange";
    public const string PINK = "Pink";
    public const string PURPLE = "Purple";
    public const string RED = "Red";
}

public static class Player_Colors
{
    public static Material GetColor(string Color)
    {
        var mat = Resources.Load(Color_Tags.Directory + Color, typeof(Material)) as Material;
        if(mat == null)
        {
            var Default_Mat = Resources.Load(Color_Tags.Directory + Color_Tags.BLUE, typeof(Material)) as Material;         // get a material from the resources folder.
        }
        return mat;
    }

}
