using System.Collections;
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
        Wanring = 2,
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
                switch ((int)Game_State)            // use a multiplier for when there are very little amount of players.
                {
                    case 1:
                        TemporaryTimer = StandByPhaseTime;
                        break;
                    case 2:
                        TemporaryTimer = WarningPhaseTime;
                        break;
                    case 3:
                        TemporaryTimer = DangerTime;
                        break;
                    case 4:
                        TemporaryTimer = FallTime;
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
    [Header("Warning Phase")]
    [SerializeField] float WarningPhaseTime = 5f;               // decreases overtime
    float TemporaryTimer;


    // Other functionalities: Turn danger tiles red, spawn a crush tower above them.
    [Header("Danger Phase")]
    [SerializeField] float DangerTime = 4f;                          // decreases overtime.
    [SerializeField] float DangerSpeed;                         // the speed at which the crushing blocks fall.
    [Header("Tiles Failling Phase")]
    [SerializeField] float FallSpeed;                          // decreases overtime.
    [SerializeField] float FallTime;
    [SerializeField] int AmountofTilesToDiscard = 1;                          // decreases overtime.

    [Header("Extra")]
    [SerializeField] Transform LobbyRoom;

    public static GameManager instance;
    [HideInInspector] public bool Game_Started;
    #endregion



    #region Built-In Methods
    public override void Attached()
    {
        state.AddCallback("SafeIndices", SafeIndiceCallback);            // we changed a state. When the state is changed, the server will call the callback on everyone's computer.
        state.AddCallback("LayerOfIndice", LayerOfIndiceCallBack);
        GetComponentInChildren<Countdown>().StartCounterInteger = Time_To_Start_Game;
        instance = this;
    }

    #region Callbacks
    private void SafeIndiceCallback()           // phase 2
    {
        TileManager.instance.WarnPlayers(state.SafeIndices);
    }

    private void LayerOfIndiceCallBack()        // phase 4
    {
        TileManager.instance.DeleteTile(state.LayerOfIndice, state.FallingIndice);          // delete the tile.
    }

    #endregion

    public void StartCountDown()        // triggered by pressing J
    {
        // if there are not enough players, DO NOT START THE GAME!!
        /*
        if(EventManager.Instance.Connections < MaxPlayersToStart)
        {
            return;
        }*/
        // start the lobby countdown here.
        Game_Started = true;
        var evnt = StartLobbyCounter.Create();
        evnt.Message = "Starting the game...";
        evnt.Send();
    }

    public override void SimulateOwner()
    {
        if((int)Game_State != 0)
            HandleGameState();
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
            var nextEvent = ChangeGameState.Create();
            if ((int)Game_State == 1) // transition from standby to warning phase. Get the indices to warn the players.
            {
                string SafeIndices = TileManager.instance.SelectIndicesToMarkSafe();
                state.SafeIndices = SafeIndices;
            }

            else if((int)Game_State == 2)
            {
                // attack the players here.. The trigger from stage 2 to 3 should mark an effect that damages players.
                TileManager.instance.SpawnDanger();
            }

            else if ((int)Game_State == 3)          // end of damage phase. Mark all tiles as safe.
            {
                var evnt = ChangeGameState.Create();
                evnt.NewState = state.Game_State;
                evnt.Send();
                //TileManager.instance.SetTilesSafe();
            }
            else if((int) Game_State == 4)
            {
                int[] PlatformToDestroy = TileManager.instance.DeleteTileSelection();
                if (PlatformToDestroy[0] != 0 && PlatformToDestroy[1] != 0)     // center platform
                {
                    state.FallingIndice = PlatformToDestroy[1];
                    state.LayerOfIndice = PlatformToDestroy[0];                 // initiate callback
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
