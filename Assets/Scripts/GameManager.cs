using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Bolt;
using UnityEngine.UI;
using Bolt.Matchmaking;
using UdpKit;
public class GameManager : Bolt.EntityBehaviour<IGameManager> 
{ 
    public enum e_GamePhases
    {
        Lobby = 0,
        StandBy = 1,
        Wanring = 2,
        Danger = 3,
        End = 4
    }

    private e_GamePhases m_GameState = 0;
    public e_GamePhases Game_State
    {
        get { return m_GameState; }
        set
        {
            if(m_GameState != value)
            {

            }
        }
    }

    bool GameHasStarted;
    [Header("Lobby Phase")]
    [SerializeField][Range(2,10)] int MaxPlayersToStart = 2;    // max amount of players for the host to start the game.
    [SerializeField] float Time_To_Start_Game = 5f;             // when the host presses start, this amount of time will pass to start the game
    [Header("Stand By Phase")]
    [SerializeField] float StandByPhaseTime = 10f;                    // time that passes while the players are running around in the standbyPhase.
    [Header("Warning Phase")]
    [SerializeField] float WarningPhaseTime = 5f;               // decreases overtime
    // Other functionalities: Turn danger tiles red, spawn a crush tower above them.
    [Header("Danger Phase")]
    [SerializeField] float DangerTime;                          // decreases overtime.
    [SerializeField] float DangerSpeed;                         // the speed at which the crushing blocks fall.
    [Header("Extra")]
    [SerializeField] Transform LobbyRoom;

    public static GameManager instance;
    [HideInInspector] public bool Game_Started;

    public override void Attached()
    {
        instance = this;
    }


    public void StartCountDown()
    {
        // start the lobby countdown here.
        Game_Started = true;
        var evnt = StartLobbyCounter.Create();
        evnt.Message = "Starting the game...";
        evnt.Send();
    }

    public void CancelCountdown()
    {

    }
}
