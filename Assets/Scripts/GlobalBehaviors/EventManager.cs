﻿using UnityEngine;

public class EventManager : Bolt.GlobalEventListener
{
    public static EventManager Instance;
    [HideInInspector] public int Connections = 1;


    private void Awake()            // nothing special. Erase this if youd like!
    {
        if (BoltNetwork.IsServer)
        {
            Instance = this;
            BoltNetwork.Instantiate(BoltPrefabs.GameManager);
            
            var entity = BoltNetwork.Instantiate(BoltPrefabs.Hammer_ItemBlock_Final, new Vector3(0, 0.2f, 0), Quaternion.identity);
            var entity2 = BoltNetwork.Instantiate(BoltPrefabs.Hammer_ItemBlock_Final, SpawnPositionManager.instance.LobbySpawnPosition.position + new Vector3(5, 0, 5), Quaternion.identity);
            //var entity3 = BoltNetwork.Instantiate(BoltPrefabs.Shield_ItemBlock, SpawnPositionManager.instance.LobbySpawnPosition.position + new Vector3(-5, 0, -5), Quaternion.Euler(-90,0,0));
            //var entity4 = BoltNetwork.Instantiate(BoltPrefabs.Shield_ItemBlock, SpawnPositionManager.instance.GameSpawnPosition.position + new Vector3(-5, 0, -5), Quaternion.Euler(-90, 0, 0));
            //var entity5 = BoltNetwork.Instantiate(BoltPrefabs.Trap_ItemBox, SpawnPositionManager.instance.LobbySpawnPosition.position + new Vector3(10 , 0, 10), Quaternion.Euler(-90, 0, 0));
            //BoltNetwork.Instantiate(BoltPrefabs.Trap_ItemBox, SpawnPositionManager.instance.GameSpawnPosition.position + new Vector3(10, 0, 10), Quaternion.Euler(-90, 0, 0));

            BoltNetwork.Instantiate(BoltPrefabs.Chicken_ItemBlock, SpawnPositionManager.instance.LobbySpawnPosition.position + new Vector3(-15, 0, -15), Quaternion.identity);

        }
    }
    public override void OnEvent(ItemPickedUpEvent evnt)            // all clients and server must understand that the player has oicked up an item.
    {
        if (BoltNetwork.IsServer)
        {
            if(evnt.ItemEntity != null)
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
            if(BoltNetwork.IsServer)
                TileManager.instance.TryToSpawnGuardedTile();
        }
    }

    public override void OnEvent(PlayerJoinedEvent evnt)
    {
        Debug.LogWarning(evnt.Message);
    }

    public override void OnEvent(LoseGameEvent evnt)        // called whenever a player loses!
    {
        evnt.Player.GetComponent<Bolt_PlayerController>().Teleport(SpawnPositionManager.instance.LobbySpawnPosition.position);
        if (BoltNetwork.IsServer)
        {
            GameManager.instance.PlayerLost(evnt.Player);
        }
    }
    public override void OnEvent(SuccessfulAttackEvent evnt)        // for sender, get the attack and call the damage function in health.cs. For weapon owner, subtract 1 from their uses.
    {
        if(evnt.FromSelf)       // caller
        {
            FindObjectOfType<Camera>().GetComponentInParent<Health>().DamagedByWeapon(evnt.WeaponDamage);
        }
        else if (evnt.WeaponEntity)                                                                             // THIS CAUSES AN ERROR.
        {
            if(evnt.WeaponEntity.IsOwner)
                evnt.WeaponEntity.GetComponent<WeaponAttack>().UseAttacK();     // decrease 1 from their available weapon uses.
        }
    }
    public override void OnEvent(StartLobbyCounter evnt)
    {
        BoltLog.Info(evnt.Message);
        var countdownObj = FindObjectOfType<Countdown>();
        if (countdownObj)
        {
            countdownObj.GetComponent<CanvasGroup>().alpha = 1;
            countdownObj.StartingGame();
        }
    }

    public override void OnEvent(StartGame evnt)            //teleport player to hub.
    {
        BoltLog.Info(evnt.Message);
        var player = FindObjectOfType<ThirdPersonCamera>().GetComponentInParent<Bolt_PlayerController>();
        player.GetComponent<Health>().InitializeHealthUI();
        player.MoveToGameRoom();
        GameManager.instance.Game_Started = true;
        if (BoltNetwork.IsServer)
        {
            FindObjectOfType<GameUI>().DisableStartGameUI();
            GameManager.instance.InitializePlayerList();
        }
    }

    public override void Connected(BoltConnection connection)
    {
        if (BoltNetwork.IsServer)
            Connections++;
    }
    public override void Disconnected(BoltConnection connection)        // either the host or a player has left the game. If The host left, disconnect everyone
    {
        if (BoltNetwork.IsServer)       // restore colors here.
            Connections--;
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(0);          // transition back to the first scene.
            Debug.LogWarning("Disconnected from server");
        }
    }

    public override void OnEvent(ToggleAreaEffectorEvent evnt)      // either destroy the trap, or assign a player to own the guarded tile.
    {
        if(evnt.TrapEntity != null)
        {
            evnt.TrapEntity.GetComponent<TrapPlacement>().DestroyTrap();
        }
    }

    public override void OnEvent(GameEnded evnt)            // call this event when the winner is going to be announced
    {
        if(GameUI.UserInterface)
            GameUI.UserInterface.AnnounceWinner(evnt.WinnerName);
    }
}
