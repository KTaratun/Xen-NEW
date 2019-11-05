using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class ServerClient
{
    public int m_connectionId;
    public string m_playerName;
    public string m_team;
}

public class ServerScript : MonoBehaviour {

    private const int MAX_CONNECTION = 100;

    private int m_port = 5357;
    private int m_hostId;
    //private int m_webHostId;
    public int m_reliableChannel;
    public int m_unreliableChannel;
    public int m_reliableFragmentedChannel;

    public bool m_isStarted = false;
    private byte m_error;

    public List<ServerClient> m_clients = new List<ServerClient>();


	// Use this for initialization
	void Start ()
    {
    }
	
	// Update is called once per frame
	void Update ()
    {
        if (!m_isStarted)
            return;

        int recHostId;
        int connectionId;
        int channelId;
        byte[] recBuffer = new byte[1024];
        int bufferSize = 1024;
        int dataSize;
        byte error;
        NetworkEventType recData = NetworkTransport.Receive(out recHostId, out connectionId, out channelId, recBuffer, bufferSize, out dataSize, out error);
        switch (recData)
        {
            case NetworkEventType.ConnectEvent:    //2
                Debug.Log("Player " + connectionId + " has connected");
                OnConnection(connectionId);
                break;
            case NetworkEventType.DataEvent:       //3
                string msg = Encoding.Unicode.GetString(recBuffer, 0, dataSize);
                Debug.Log("Receiving from " + connectionId + " : " + msg);
                string[] splitData = msg.Split('~');

                switch (splitData[0])
                {
                    case "NAMEIS":
                        OnNameIs(connectionId, splitData[1]);
                        break;
                    case "TEAMIS":
                        OnTeamIs(connectionId, splitData[1]);
                        break;
                    case "MOVESTART":
                        OnMove(splitData[1]);
                        break;
                    case "TURNEND":
                        GameObject.Find("Board").GetComponent<BoardScript>().EndTurn(true);
                        break;
                    default:
                        Debug.Log("Invalid Message: " + msg);
                        break;
                }
                break;
            case NetworkEventType.DisconnectEvent: //4
                Debug.Log("Player " + connectionId + " has disconnected");
                OnDisconnection(connectionId);
                break;
        }
    }

    private void OnNameIs(int _conId, string _name)
    {
        // Link the name to the connection Id
        m_clients.Find(x => x.m_connectionId == _conId).m_playerName = _name;

        // Tell everybody that a new player has connected
        Send("CON~" + _name + '~' + _conId, m_reliableChannel, m_clients);
    }

    private void OnTeamIs(int _conId, string _team)
    {
        // Link the name to the connection Id
        m_clients.Find(x => x.m_connectionId == _conId).m_team = _team;

        //// Tell everybody that a new player has connected
        //Send("CON~" + _name + '~' + _conId, m_reliableChannel, m_clients);
    }

    private void OnMove(string _data)
    {
        int objId = int.Parse(_data.Split('|')[0]);
        int tileId = int.Parse(_data.Split('|')[1]);
        bool isForced = bool.Parse(_data.Split('|')[2]);

        BoardScript b = GameObject.Find("Board").GetComponent<BoardScript>();
        b.m_netOBJs[objId].GetComponent<ObjectScript>().MovingStart(b.m_tiles[tileId], isForced, true);
    }

    private void OnConnection(int _conId)
    {
        //Add him to list
        ServerClient c = new ServerClient();
        c.m_connectionId = _conId;
        c.m_playerName = "TEMP";
        m_clients.Add(c);

        // When the player joins the server, tell him his ID
        // Request his name and send the name to all other players
        string msg = "ASKNAME~" + _conId + "~";

        foreach(ServerClient sc in m_clients)
            msg += sc.m_playerName + '%' + sc.m_connectionId + '~';

        msg = msg.Trim('~');

        // ASKNAME|3|DAVE%1|MICHAEL%2|TEMP%3

        Send(msg, m_reliableChannel, _conId);
    }

    private void OnDisconnection(int _conId)
    {
        // Remove this player from our client list
        m_clients.Remove(m_clients.Find(x => x.m_connectionId == _conId));

        // Tell everyone that someone else has disconnected
        Send("DC~" + _conId, m_reliableChannel, m_clients);
    }

    public void Send(string _message, int _channelId, int _conId)
    {
        List<ServerClient> c = new List<ServerClient>();
        c.Add(m_clients.Find(x => x.m_connectionId == _conId));
        Send(_message, _channelId, c);

    }

    public void Send(string _message, int _channelId, List<ServerClient> c)
    {
        Debug.Log("Sending: " + _message);
        byte[] msg = Encoding.Unicode.GetBytes(_message);
        for (int i = 1; i < m_clients.Count; i++)
            NetworkTransport.Send(m_hostId, m_clients[i].m_connectionId, _channelId, msg, _message.Length * sizeof(char), out m_error); 
    }

    public void Connect()
    {
        NetworkTransport.Init();
        ConnectionConfig cc = new ConnectionConfig();

        m_reliableChannel = cc.AddChannel(QosType.Reliable);
        m_unreliableChannel = cc.AddChannel(QosType.Unreliable);
        //m_reliableFragmentedChannel = cc.AddChannel(QosType.ReliableSequenced);

        HostTopology topo = new HostTopology(cc, MAX_CONNECTION);

        m_hostId = NetworkTransport.AddHost(topo, m_port, null);
        //m_webHostId = NetworkTransport.AddWebsocketHost(topo, m_port, null);

        ServerClient c = new ServerClient();
        c.m_connectionId = 0;
        c.m_playerName = "SERVER";

        string teamString = "";
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

        c.m_team = teamString;
        m_clients.Add(c);

        m_isStarted = true;

        DontDestroyOnLoad(gameObject);
        DontDestroyOnLoad(GameObject.Find("Database"));
        SceneManager.LoadScene("Scene1");
    }
}
