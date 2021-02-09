using UnityEngine;
using System;
using Bolt;
using Bolt.Matchmaking;
using UdpKit;

public class Menu : GlobalEventListener 
{
    public void StartServer()
    {
        BoltLauncher.StartServer(); // start the server;
    }

    public override void BoltStartDone()
    {
        if(BoltNetwork.IsServer)
            BoltMatchmaking.CreateSession(sessionID: "test", sceneToLoad: "Game");       // name the session ID.
    }

    public void StartClient()
    {
        BoltLauncher.StartClient();
    }

    public override void SessionListUpdated(Map<Guid, UdpSession> sessionList)
    {
        foreach(var session in sessionList)
        {
            UdpSession PhotonSession = session.Value as UdpSession;

            if(PhotonSession.Source == UdpSessionSource.Photon)
            {
                BoltMatchmaking.JoinSession(PhotonSession);

            }
        }
    }
}
