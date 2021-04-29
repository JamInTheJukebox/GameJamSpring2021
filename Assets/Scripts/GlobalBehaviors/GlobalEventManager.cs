using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Bolt;
using Bolt.Matchmaking;
using UdpKit;
using System.Linq;

[BoltGlobalBehaviour(BoltNetworkModes.Server)]
public class GlobalEventManager : Bolt.GlobalEventListener
{
    static int MAX_PLAYERS = 10;

    public override void OnEvent(GetPlayerPersonalizationEvent evnt)        // HANDLE ALL COSMETICS HERE!!
    {
        var playermat = Player_Colors.PlayerMaterials;
        if (playermat.Count == 0)     // no materials assigned.        Get a  list of all the possible colors. Remove them as you go.
        {
            playermat.Add(Color_Tags.BLUE); playermat.Add(Color_Tags.GREEN);
            playermat.Add(Color_Tags.GREY); playermat.Add(Color_Tags.LIGHTBLUE);
            playermat.Add(Color_Tags.LIGHTGREEN); playermat.Add(Color_Tags.ORANGE);
            playermat.Add(Color_Tags.PURPLE); playermat.Add(Color_Tags.RED);
            playermat.Add(Color_Tags.WHITE); playermat.Add(Color_Tags.YELLOW);
        }
        int chosenIndex = Random.Range(0, playermat.Count);
        string chosenColor = playermat[chosenIndex];        // if its a problem, use the delete function in player_materials.
        playermat.RemoveAt(chosenIndex);        // index has been chosen.

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

    public override void ConnectRequest(UdpEndPoint endpoint, IProtocolToken token)         // reject the player if there are more than 10 players and if the game has started.
    {
        var connections = BoltNetwork.Connections.ToList();
        // https://doc.photonengine.com/en-us/bolt/current/connection-and-authentication/accept-refuse-connection
        if (connections.Count > MAX_PLAYERS | GameManager.instance.Game_Counter_Started)     // 10 players max; // reject if the game has started
        {
            BoltNetwork.Refuse(endpoint);
            /*
            BoltMatchmaking.UpdateSession(new PhotonRoomProperties()
            {
                IsOpen = false,
                IsVisible = false,
            });*/
            return;
        }

        BoltNetwork.Accept(endpoint);
    }

    public override void SessionConnected(UdpSession session, IProtocolToken token)
    {
        BoltLog.Warn("Connected to Game!");
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
    public const string BLUE = "Blue";
    public const string GREEN = "Green";
    public const string GREY = "Grey";
    public const string LIGHTBLUE = "LightBlue";
    public const string LIGHTGREEN = "LightGreen";
    public const string ORANGE = "Orange";
    public const string PURPLE = "Purple";
    public const string RED = "Red";
    public const string WHITE = "White";
    public const string YELLOW = "Yellow";

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

    public static List<string> PlayerMaterials = new List<string>();

    public static Dictionary<string, float> Color_Intensity_Multiplier = new Dictionary<string, float>()
    {
        [Color_Tags.BLUE] = 3.5f,
        [Color_Tags.GREEN] = 4.5197f,
        [Color_Tags.GREY] = 5.294f,
        [Color_Tags.LIGHTBLUE] = 3.79f,
        [Color_Tags.LIGHTGREEN] = 3.271f,
        [Color_Tags.ORANGE] = 3.5f,
        [Color_Tags.PURPLE] = 3.5f,
        [Color_Tags.RED] = 3.5f,
        [Color_Tags.WHITE] = 3.0227f,
        [Color_Tags.YELLOW] = 3.5f
    };

    public static void RemoveMaterial(string color)
    {
        if (PlayerMaterials.Contains(color))
            PlayerMaterials.Remove(color);
    }

    public static void AddMaterial(string color)
    {
        if (!PlayerMaterials.Contains(color))
            PlayerMaterials.Add(color);
    }
    public static float GetColorIntensity(string color)
    {
        if (Color_Intensity_Multiplier.ContainsKey(color))
        {
            return Color_Intensity_Multiplier[color];
        }
        return 3.5f;         // if no color is found, return the default value of 3.5
    }
}
