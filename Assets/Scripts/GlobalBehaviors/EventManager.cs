using UnityEngine;

public class EventManager : Bolt.GlobalEventListener
{
    public static EventManager Instance;
    [HideInInspector] public int Connections = 1;

    private void Awake()
    {
        if (BoltNetwork.IsServer)
        {
            Instance = this;
            BoltNetwork.Instantiate(BoltPrefabs.GameManager);
            var entity = BoltNetwork.Instantiate(BoltPrefabs.Hammer_ItemBlock, new Vector3(0, 0.2f, 0), Quaternion.Euler(5.293f, -92.402f, 65.55f));
        }
    }
    public override void OnEvent(ItemPickedUpEvent evnt)
    {
        if (BoltNetwork.IsServer)
        {
            BoltNetwork.Destroy(evnt.ItemEntity);
        }

        if (evnt.FromSelf)
        {
            evnt.PlayerEntity.GetComponentInChildren<WeaponManager>().SpawnWeapon();
            //BoltNetwork.Instantiate(BoltPrefabs.hammer_low, new Vector3(0,0.2f,0))
        }
    }

    public override void OnEvent(ChangeGameState evnt)
    {
        if(evnt.NewState == 1)
        {
            TileManager.instance.WarnPlayers(evnt.SafeIndices);
        }
        if (evnt.NewState == 2)
        {
            TileManager.instance.SpawnDanger();
        }
        if (evnt.NewState == 3)
        {
            TileManager.instance.SetTilesSafe();
        }
        if(evnt.NewState == 4)
        {
            TileManager.instance.DeleteTile(evnt.FallingIndices);          // delete the tile.
        }
    }

    public override void OnEvent(PlayerJoinedEvent evnt)
    {
        Debug.LogWarning(evnt.Message);
    }

    public override void OnEvent(GetWeaponEvent evnt)
    {
        print(evnt.PlayerEntity.gameObject.name);
        evnt.Weapon.transform.SetParent(evnt.PlayerEntity.transform.GetChild(1).Find("WeaponSlot"));
    }

    public override void OnEvent(StartLobbyCounter evnt)
    {
        BoltLog.Info(evnt.Message);
        var countdownObj = FindObjectOfType<Countdown>();
        countdownObj.GetComponent<CanvasGroup>().alpha = 1;
        countdownObj.StartingGame();
    }

    public override void OnEvent(StartGame evnt)            //teleport player to hub.
    {
        BoltLog.Info(evnt.Message);
        var player = FindObjectOfType<ThirdPersonCamera>().GetComponentInParent<Bolt_PlayerController>();
        player.MoveToGameRoom();
    }

    public override void Connected(BoltConnection connection)
    {
        if (BoltNetwork.IsServer)
            Connections++;
    }
    public override void Disconnected(BoltConnection connection)        // either the host or a player has left the game. If The host left, disconnect everyone
    {
        if (BoltNetwork.IsServer)
            Connections--;
        if(!BoltNetwork.IsServer)
            UnityEngine.SceneManagement.SceneManager.LoadScene(0);      // go to main menu;
    }


}
