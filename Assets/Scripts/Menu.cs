using UnityEngine;
using System;
using Bolt;
using Bolt.Matchmaking;
using UdpKit;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class Menu : GlobalEventListener 
{
    private void Awake()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    [SerializeField] GameObject Server_Box_Prefab;
    [SerializeField] Transform Server_List_Content;
    private List<GameObject> JoinServerBoxes = new List<GameObject>();

    public void OnSetUserNameValueChanged(string NewName)
    {
        PlayerPrefs.SetString("username", NewName);
    }

    public void StartServer()
    {
        BoltLauncher.StartServer(); // start the server;
    }

    public override void BoltStartDone()
    {
        string newServerName = Regex.Replace(PlayerPrefs.GetString("ServerName"), @"[^a-zA-Z0-9 ]", "");
        if (BoltNetwork.IsServer)
            BoltMatchmaking.CreateSession(sessionID: newServerName, sceneToLoad: "Game");       // name the session ID.
    }

    public void StartClient()
    {
        BoltLauncher.StartClient();
    }

    public override void SessionListUpdated(Map<Guid, UdpSession> sessionList)
    {
        ClearSessions();
        foreach(var session in sessionList)
        {
            UdpSession PhotonSession = session.Value as UdpSession;

            GameObject joinButtonClone = Instantiate(Server_Box_Prefab,Server_List_Content);
            
            joinButtonClone.SetActive(true);
            joinButtonClone.GetComponentInChildren<Button>().onClick.AddListener(() => JoinGame(PhotonSession));
            joinButtonClone.GetComponent<ServerBoxController>().SetServerName(PhotonSession.HostName);
            joinButtonClone.GetComponent<ServerBoxController>().SetServerCount(PhotonSession.ConnectionsCurrent);
            JoinServerBoxes.Add(joinButtonClone);
            /*
            if(PhotonSession.Source == UdpSessionSource.Photon)
            {
                BoltMatchmaking.JoinSession(PhotonSession);

            }*/


        }
    }

    private void ClearSessions()
    {
        foreach(GameObject button in JoinServerBoxes)
        {
            Destroy(button);
        }
        JoinServerBoxes.Clear();
    }
    public void JoinGame(UdpSession PhotonSession)
    {
        BoltMatchmaking.JoinSession(PhotonSession);

    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void OnServerNameHasChanged(string newServerName)
    {
        PlayerPrefs.SetString("ServerName", newServerName);
    }

    public void ShutDownBolt()
    {
        BoltNetwork.Shutdown();
    }

}
