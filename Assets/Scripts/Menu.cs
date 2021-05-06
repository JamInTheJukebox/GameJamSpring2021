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
    // used for initializing settings.
    public Toggle Y_Toggle;
    [Header("Hat Icons")]
    public Image HatContainer;
    public List<Sprite> Hats = new List<Sprite>();
    [Header("Eye Icons")]
    public Image EyeContainer;
    public List<Sprite> Eyes = new List<Sprite>();

    private int m_HatID;
    public int HatID                // 0: nothing, 1: Party Hat, 2: Popo Head, 3: Hat Text, 4: Thinning
    {
        get { return m_HatID; }
        set
        {
            if(value != m_HatID)
            {
                m_HatID = value;
                if(m_HatID < 0)
                {
                    m_HatID = PlayerPersonalization.NumberOfHats;        // max ID;
                }
                else if(m_HatID > PlayerPersonalization.NumberOfHats)
                {
                    m_HatID = 0;
                }

                // update hat logo.
                HatContainer.sprite = Hats[m_HatID];
                PlayerPrefs.SetInt("HatID", m_HatID);
                print(m_HatID);
            }
        }
    }
    private int m_EyeID;
    public int EyeID
    {
        get { return m_EyeID; }
        set
        {
            if(value != m_EyeID)
            {
                m_EyeID = value;
                if (m_EyeID < 0)
                {
                    m_EyeID = PlayerPersonalization.NumberOfEyewear;        // max ID;
                }
                else if (m_EyeID > PlayerPersonalization.NumberOfEyewear)
                {
                    m_EyeID = 0;
                }
                // update eye logo
                // update eye ID in text.
                EyeContainer.sprite = Eyes[m_EyeID];
                PlayerPrefs.SetInt("EyeID", m_EyeID);
                print(m_EyeID);

            }
        }
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;  
        HatID = PlayerPrefs.GetInt("HatID");
        EyeID = PlayerPrefs.GetInt("EyeID");
        LobbyTiles.ResetStaticLists();
        Player_Colors.PlayerMaterials.Clear();

        HatContainer.sprite = Hats[HatID];
        EyeContainer.sprite = Eyes[EyeID];

        Y_Toggle.isOn = PlayerSettings.Mouse_Y_Invert;
    }
    public void OnSetUserNameValueChanged(string NewName)
    {
        PlayerPrefs.SetString("username", NewName);
    }

    public void StartServer()
    {
        // error: Trying to connect to session then trying to host game.
        BoltLauncher.StartServer(); // start the server;
    }

    public override void BoltStartDone()
    {
        string newServerName = Regex.Replace(PlayerPrefs.GetString("ServerName"), @"[^a-zA-Z0-9 ]", "");
        if (PlayerPrefs.GetString("ServerName").Length == 0)        // server names cannot be empty
        {
            PlayerPrefs.SetString("ServerName", "DefaultServerName" + UnityEngine.Random.Range(0, 10000000000));         // assign random server name
            newServerName = PlayerPrefs.GetString("ServerName");
            print(newServerName);
        }
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
            var ButtonClone = joinButtonClone.GetComponent<ServerBoxController>();
            ButtonClone.SetServerName(PhotonSession.HostName);
            ButtonClone.SetServerCount(PhotonSession.ConnectionsCurrent);
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
        if(BoltNetwork.IsRunning)           // if there is a boltnetwork instance, shutdown.
            BoltNetwork.Shutdown();
    }


    public void ChangeInvertY(bool newY)
    {
        PlayerSettings.On_Y_Invert_Changed(newY);
    }

    public void ChangeToolTips(bool newTool)
    {
        PlayerSettings.On_ToolTip_Changed(newTool);
    }

    public void ChangeHatID(int Delta)
    {
        HatID += Delta;
    }

    public void ChangeEyeID(int Delta)
    {
        EyeID += Delta;
    }
}
