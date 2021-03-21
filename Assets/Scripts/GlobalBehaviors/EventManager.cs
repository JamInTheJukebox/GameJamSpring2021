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
            var entity2 = BoltNetwork.Instantiate(BoltPrefabs.Hammer_ItemBlock, SpawnPositionManager.instance.LobbySpawnPosition.position + new Vector3(5, 0, 5), Quaternion.Euler(5.293f, -92.402f, 65.55f));
            var entity3 = BoltNetwork.Instantiate(BoltPrefabs.Shield_ItemBlock, SpawnPositionManager.instance.LobbySpawnPosition.position + new Vector3(-5, 0, -5), Quaternion.Euler(-90,0,0));
            var entity4 = BoltNetwork.Instantiate(BoltPrefabs.Shield_ItemBlock, SpawnPositionManager.instance.GameSpawnPosition.position + new Vector3(-5, 0, -5), Quaternion.Euler(-90, 0, 0));
            var entity5 = BoltNetwork.Instantiate(BoltPrefabs.Trap_ItemBox, SpawnPositionManager.instance.LobbySpawnPosition.position + new Vector3(10 , 0, 10), Quaternion.Euler(-90, 0, 0));
            BoltNetwork.Instantiate(BoltPrefabs.Trap_ItemBox, SpawnPositionManager.instance.GameSpawnPosition.position + new Vector3(10, 0, 10), Quaternion.Euler(-90, 0, 0));
        }
    }
    public override void OnEvent(ItemPickedUpEvent evnt)            // all clients and server must understand that the player has oicked up an item.
    {
        if (BoltNetwork.IsServer)
        {
            BoltNetwork.Destroy(evnt.ItemEntity);                   // only the server can spawn item entities
        }

        if (evnt.FromSelf)
        {
            if(evnt.ItemType == "" | evnt.ItemType == null) { return; }     // return if the item type is a shield or something that is not a weapon/trap.
            evnt.PlayerEntity.GetComponentInChildren<WeaponManager>().InitializeItem(evnt.ItemType);
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

    public override void OnEvent(LoseGameEvent evnt)
    {
        evnt.Player.transform.position = SpawnPositionManager.instance.LobbySpawnPosition.position;

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
        player.GetComponent<Health>().InitializeHealthUI();
        player.MoveToGameRoom();
        GameManager.instance.Game_Started = true;
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

    public override void OnEvent(ToggleAreaEffectorEvent evnt)      // either destroy the trap, or assign a player to own the guarded tile.
    {
        if(evnt.TrapEntity != null)
        {
            evnt.TrapEntity.GetComponent<TrapPlacement>().DestroyTrap();
        }
    }
}
