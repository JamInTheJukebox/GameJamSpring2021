using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class ServerBoxController : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI ServerNameText;
    [SerializeField] TextMeshProUGUI ServerCountText;

    private string m_serverName;
    public string serverName
    {
        get {
            return m_serverName;
        }
        set
        {
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
            ServerCountText.text = m_serverCount;
        }
    }

    public void SetServerName(string server_Name)
    {
        serverName = server_Name;
    }

    public void SetServerCount(int count)
    {
        serverCount = count.ToString() + "/10";
    }
}
