﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Bolt;
using UnityEngine.UI;
using Bolt.Matchmaking;
using UdpKit;
public class GameManager : Bolt.EntityBehaviour<IGameManager> 
{
    #region Variables
    public enum e_GamePhases
    {
        Lobby = 0,
        StandBy = 1,
        Warning = 2,
        Danger = 3,
        End = 4
    }
    private e_GamePhases m_Game_State = 0;
    public e_GamePhases Game_State
    {
        get
        {
            return m_Game_State;
        }
        set
        {
            if (m_Game_State != value && BoltNetwork.IsServer)
            {
                state.Game_State = (int)value;
                m_Game_State = value;
                float a = (float)TileManager.instance.AllTiles.Count;
                if (TileManager.instance.AllTiles.Count == 0) { a = 1; }
                //a = Mathf.Clamp(a / 2, 0, 1);
                switch ((int)Game_State)            // use a multiplier for when there are very little amount of players.
                {
                    case 1:

                        TemporaryTimer = Mathf.Lerp(StandByPhaseTime,MinStandByPhaseTime,1/a*3);
                        break;
                    case 2:
                        TemporaryTimer = Mathf.Lerp(WarningPhaseTime,MinWarningPhaseTime,1/a*3);
                        break;
                    case 3:
                        TemporaryTimer = Mathf.Lerp(DangerTime,MinDangerTime,1/a * 3);
                        break;
                    case 4:
                        TemporaryTimer = Mathf.Lerp(FallTime,MinFallTime,1/a * 3);
                        break;
                }
            }
        }
    }

    bool GameHasStarted;
    [Header("Lobby Phase")]
    [SerializeField] [Range(2, 10)] int MaxPlayersToStart = 2;    // max amount of players for the host to start the game.
    [SerializeField] int Time_To_Start_Game = 5;             // when the host presses start, this amount of time will pass to start the game
    [Header("Stand By Phase")]
    [SerializeField] float StandByPhaseTime = 10f;                    // time that passes while the players are running around in the standbyPhase.
    [SerializeField] float MinStandByPhaseTime = 6f;

    [Header("Warning Phase")]
    [SerializeField] float WarningPhaseTime = 5f;               // decreases overtime
    [SerializeField] float MinWarningPhaseTime = 3f;               // decreases overtime
    float TemporaryTimer;


    // Other functionalities: Turn danger tiles red, spawn a crush tower above them.
    [Header("Danger Phase")]
    [SerializeField] float DangerTime = 4f;                          // decreases overtime.
    [SerializeField] float MinDangerTime = 5f;                          // decreases overtime.
    [SerializeField] float DangerSpeed;                         // the speed at which the crushing blocks fall.
    [Header("Tiles Failling Phase")]
    [SerializeField] float FallSpeed;                          // decreases overtime.
    [SerializeField] float FallTime;
    [SerializeField] float MinFallTime;

    [SerializeField] int AmountofTilesToDiscard = 1;                          // decreases overtime.

    [Header("Extra")]
    [SerializeField] Transform LobbyRoom;
    public static GameManager instance;
    [HideInInspector] public bool Game_Counter_Started;
    [HideInInspector] public bool Game_Started;

    [HideInInspector] public List<BoltEntity> AllPlayers = new List<BoltEntity>();      // when this list has only one player left, stop the game and tell the players who won.

    [Header("Game Attributes")]
    public float MaxWaitTimeForItems = 60;
    public float MinWaitTimeForItems = 25;
    private float WaitCounterForItems = 3;          // depends on # of players
    public int MaxItems;
    private Bolt.PrefabId[] ItemBox = new Bolt.PrefabId[4]{
        BoltPrefabs.Hammer_ItemBlock_Final, BoltPrefabs.Chicken_ItemBlock, BoltPrefabs.Trap_ItemBox,BoltPrefabs.Shield_ItemBlock           // left to change: Trap and add chicken
    };
    // only 3 items so far: hammer, shield, and trap
    public float ProbabilityToSpawnGuardedTile = 0.3f;
    #endregion



    #region Built-In Methods
    public override void Attached()
    {
        GetComponentInChildren<Countdown>().StartCounterInteger = Time_To_Start_Game;
        instance = this;
        WaitCounterForItems = Random.Range(MinWaitTimeForItems, MaxWaitTimeForItems);
    }

    public void InitializePlayerList()
    {
        var players = FindObjectsOfType<Health>();
        foreach(var player in players)
        {
            AllPlayers.Add(player.GetComponent<BoltEntity>());
        }
    }

    public void PlayerLost(BoltEntity LosingPlayer)
    {
        AllPlayers.Remove(LosingPlayer);
        if(AllPlayers.Count == 1)
        {
            // player won!
            var name = AllPlayers[0].GetComponent<PlayerPersonalization>().GetName();
            var evnt = GameEnded.Create();
            evnt.WinnerName = name;
            evnt.Send();

        }
    }

    public void StartCountDown()        // triggered by pressing J
    {
        if(EventManager.Instance.Connections < MaxPlayersToStart)
        {
            BoltLog.Warn("There are not enough players to start the game!");
            return;
        }
        Game_Counter_Started = true;
        var evnt = StartLobbyCounter.Create();
        evnt.Message = "Starting the game...";
        evnt.Send();
    }

    public override void SimulateOwner()
    {

        if((int)Game_State != 0)
        {
            WaitCounterForItems -= BoltNetwork.FrameDeltaTime;
            HandleGameState();
            if(WaitCounterForItems <= 0)
            {
                int minNumber = 1;
                minNumber = TileManager.instance.AllTiles.Count / 10 + 1;
                int MaxNumber = minNumber + MaxItems;
                int NumItems = Random.Range(minNumber, MaxNumber);


                // spawn item

                Bolt.PrefabId[] randomItems = new Bolt.PrefabId[NumItems];
                for(int i = 0; i < NumItems; i++)
                {
                    int randomItem = Random.Range(0, ItemBox.Length);
                    randomItems[i] = ItemBox[randomItem];
                }
                TileManager.instance.SpawnItems(randomItems);
                WaitCounterForItems = Random.Range(MinWaitTimeForItems, MaxWaitTimeForItems);
            }
        }
    }

    public void HandleGameState()
    {
        TemporaryTimer -= BoltNetwork.FrameDeltaTime;       // subtract by the frame delta time of the server.

        switch ((int)Game_State)    // actions during event.
        {
            case 1:
                // spawn weapons
                // spawn traps
                break;
            case 2:
                break;
            case 3:
                break;
            case 4:
                break;
        }

        if(TemporaryTimer < 0)      // End of the Phase. Proceed to next event
        {
            Debug.Log("Ending State: " + Game_State);
            var evnt = ChangeGameState.Create();
            evnt.NewState = state.Game_State;
            if ((int)Game_State == 1) // transition from standby to warning phase. Get the indices to warn the players.
            {
                string SafeIndices = TileManager.instance.SelectIndicesToMarkSafe();
                state.SafeIndices = SafeIndices;
                evnt.SafeIndices = state.SafeIndices;
                evnt.Send();
            }

            else if((int)Game_State == 2)
            {
                evnt.Send();
                // attack the players here.. The trigger from stage 2 to 3 should mark an effect that damages players.
            }

            else if ((int)Game_State == 3)          // end of damage phase. Mark all tiles as safe.
            {
                evnt.Send();
                //TileManager.instance.SetTilesSafe();
            }
            else if((int) Game_State == 4)
            {
                string PlatformsToDestroy = TileManager.instance.DeleteTileSelection();
                if (PlatformsToDestroy != "")     // center platform
                {
                    state.FallingIndice = PlatformsToDestroy;
                    evnt.FallingIndices = state.FallingIndice;
                    evnt.Send();
                }
            }
            Game_State = (Game_State == e_GamePhases.End) ? (e_GamePhases.StandBy) : (Game_State + 1);      // if you are at the last phase, move to phase 1, otherwise, keep going up by one.
        }
    }

    public void SpawnRandomTrap()
    {

    }

    public void SpawnRandomWeapon()
    {

    }
    #endregion

}
