using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

//public class Player
//{
//    public string m_playerName;
//    public List<CharacterScript> m_chars;
//    public int m_connectionId;
//    public int[] m_energy;
//    public NetworkedBoardScript m_bScript;
//}

//public class FromServer
//{
//    //public string m_oBJPos;
//    //public string m_players;
//    public string m_data;
//}


public class ClientScript : MonoBehaviour {

    private const int MAX_CONNECTION = 100;

    private int m_port = 5357;
    private const int WEB_PORT = 26001;
    private int m_hostId;

    public int m_reliableChannel;
    public int m_unreliableChannel;

    private int m_ourClientId;
    public int m_connectionId;

    private float m_connectionTime;
    public bool m_isConnected = false;
    private bool m_isStarted = false;
    
    private byte error;

    private string m_name;
    //public FromServer m_fromServer;
    //public Dictionary<int, Player> m_players = new Dictionary<int, Player>();

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update ()
    {
        if (!m_isConnected)
            return;

        int recHostId;
        int connectionId;
        int channelId;
        byte[] recBuffer = new byte[2048]; //1024
        int bufferSize = 2048;
        int dataSize;
        byte error;
        NetworkEventType recData = NetworkTransport.Receive(out recHostId, out connectionId, out channelId, recBuffer, bufferSize, out dataSize, out error);
        switch (recData)
        {
            case NetworkEventType.DataEvent:
                string msg = Encoding.Unicode.GetString(recBuffer, 0, dataSize);
                Debug.Log("Receiving: " + msg);
                string[] splitData = msg.Split('~');

                switch(splitData[0])
                {
                    case "ASKNAME":
                        OnAskName(splitData);
                        break;
                    case "READY":
                        //GameObject.Find("Board").GetComponent<BoardScript>().ClientInit(splitData[1]);
                        break;
                    case "MOVESTART":
                        OnMove(splitData[1]);
                        break;
                    case "TURNEND":
                        GameObject.Find("Scene Manager").GetComponent<GameManagerScript>().EndTurn();
                        break;
                    case "NEWROUND":
                        //GameObject.Find("Board").GetComponent<BoardScript>().ClientRoundInit(splitData[1]);
                        break;
                    case "CON":
                        SpawnCharacters(splitData[1], int.Parse(splitData[2]));
                        break;
                    case "DC":
                        PlayerDisconnected(int.Parse(splitData[1]));
                        break;

                    default:
                        Debug.Log("Invalid Message: " + msg);
                        break;
                }
                break;
        }
    }

    private void OnAskName(string[] _data)
    {
        // Set this client's ID
        m_ourClientId = int.Parse(_data[1]);

        // Send our name to the server
        Send("NAMEIS~" + m_name, m_reliableChannel);

        // Create all the other players
        for (int i = 2; i < _data.Length - 1; i++)
        {
            string[] d = _data[i].Split('%');
            SpawnCharacters(d[0], int.Parse(d[1]));
        }
    }

    private void SpawnCharacters(string _playerName, int _conId)
    {
        // do all initation code

        if (_conId == m_ourClientId)
        {
            // Add mobility
            // Remove Canvas
            m_isStarted = true;
        }

        string teamString = "TEAMIS~";

        for (int i = 0; i < 6; i++)
        {
            string key = 0.ToString() + ',' + i.ToString();
            string name = PlayerPrefs.GetString(key + ",name");
            string charString = "";
            if (name.Length > 0)
                charString = PlayerPrefScript.PackageForNetwork(i);

            if (charString != "")
                teamString += charString + ";";
        }

        teamString = teamString.Trim(';');

        Send(teamString, m_reliableChannel);

        //Player p = new Player();
        //m_players.Add(_conId, p);
        //p.m_chars = 
    }

    private void OnMove(string _data)
    {
        int objId = int.Parse(_data.Split('|')[0]);
        int tileId = int.Parse(_data.Split('|')[1]);
        bool isForced = bool.Parse(_data.Split('|')[2]);

        //NetworkedBoardScript b = GameObject.Find("Board").GetComponent<NetworkedBoardScript>();
        //b.m_netOBJs[objId].GetComponent<ObjectScript>().MovingStart(b.m_tiles[tileId], isForced, true);
    }

    private void PlayerDisconnected(int _conId)
    {
        //Destroy
        //m_players.Remove(_conId);
    }

    public void Send(string _message, int _channelId)
    {
        Debug.Log("Sending: " + _message);
        byte[] msg = Encoding.Unicode.GetBytes(_message);
        NetworkTransport.Send(m_hostId, m_connectionId, _channelId, msg, _message.Length * sizeof(char), out error);
    }

    public void Connect()
    {
        NetworkTransport.Init();
        ConnectionConfig cc = new ConnectionConfig();

        m_reliableChannel = cc.AddChannel(QosType.Reliable);
        m_unreliableChannel = cc.AddChannel(QosType.Unreliable);

        HostTopology topo = new HostTopology(cc, MAX_CONNECTION);

        m_hostId = NetworkTransport.AddHost(topo, 0);

//#if UNITY_WEBGL && UNITY_EDITOR
        //m_connectionId = NetworkTransport.Connect(m_hostId, "192.168.1.14", WEB_PORT, 0, out error);
//#else
        m_connectionId = NetworkTransport.Connect(m_hostId, "192.168.1.14", m_port, 0, out error);
//#endif
        m_name = "TEAM " + m_connectionId.ToString();

        m_connectionTime = Time.time;
        m_isConnected = true;

        DontDestroyOnLoad(gameObject);
        //DontDestroyOnLoad(GameObject.Find("Database"));
        SceneManager.LoadScene("Scene1");
    }

    void OnApplicationQuit()
    {
        NetworkTransport.Shutdown();
    }
}
