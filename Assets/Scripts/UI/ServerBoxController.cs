using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class ServerBoxController : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI ServerNameText;
    [SerializeField] TextMeshProUGUI ServerCountNumerator;
    [SerializeField] TextMeshProUGUI ServerCountDenominator;

    private Color ServerFull = Color.red;           // full
    public Color CoolServer = Color.white;         // not full

    private string m_serverName;
    public string serverName
    {
        get {
            return m_serverName;
        }
        set
        {
            m_serverName = value;
            ServerNameText.text = m_serverName;
        }
    }

    private string m_serverCount;
    public string serverCount
    {
        get
        {
            return m_serverCount;
        }
        set
        {
            m_serverCount = value;
            ServerCountNumerator.text = m_serverCount;
            ServerCountNumerator.color = (ServerCountNumerator.text == "10") ? ServerFull : CoolServer;
            ServerCountDenominator.color = (ServerCountNumerator.text == "10") ? ServerFull : CoolServer;
        }
    }


    public void SetServerName(string server_Name)
    {
        serverName = server_Name;
    }

    public void SetServerCount(int count)
    {
        serverCount = count.ToString();
    }
}
