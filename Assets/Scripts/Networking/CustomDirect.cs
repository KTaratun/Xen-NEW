using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.Types;
using UnityEngine.Networking.Match;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System;

public class CustomServerClient
{
    public int m_connectionId;
    public string m_playerName;
    public string m_team;
}

public class CustomDirect : DirectSetup
{ 
    public List<ServerClient> m_clients = new List<ServerClient>();
    private bool m_isServer = false;
    public int m_ourClientId;
    public bool m_isStarted = false;

    public NetworkedGameScript m_netGame;
    public BoardScript m_board;

    override protected void Update()
    {
        if (m_HostId == -1)
            return;

        var networkEvent = NetworkEventType.Nothing;
        int connectionId;
        int channelId;
        int receivedSize;
        byte error;

        // Get events from the relay connection
        networkEvent = NetworkTransport.ReceiveRelayEventFromHost(m_HostId, out error);
        if (networkEvent == NetworkEventType.ConnectEvent)
            Debug.Log("Relay server connected");
        if (networkEvent == NetworkEventType.DisconnectEvent)
            Debug.Log("Relay server disconnected");

        do
        {
            // Get events from the server/client game connection
            networkEvent = NetworkTransport.ReceiveFromHost(m_HostId, out connectionId, out channelId,
                m_ReceiveBuffer, (int)m_ReceiveBuffer.Length, out receivedSize, out error);
            if ((NetworkError)error != NetworkError.Ok)
            {
                Debug.LogError("Error while receiveing network message: " + (NetworkError)error);
            }

            switch (networkEvent)
            {
                case NetworkEventType.ConnectEvent:
                    {
                        Debug.Log("Connected through relay, ConnectionID:" + connectionId +
                            " ChannelID:" + channelId);
                        m_ConnectionEstablished = true;
                        m_ConnectionIds.Add(connectionId);
                        if (m_isServer)
                            OnConnection(connectionId);
                        else
                        {
                            DontDestroyOnLoad(gameObject);
                            SceneManager.LoadScene("Scene1");
                        }
                        break;
                    }
                case NetworkEventType.DataEvent:
                    {
                        Debug.Log("Data event, ConnectionID:" + connectionId +
                            " ChannelID: " + channelId +
                            " Received Size: " + receivedSize);
                        m_Reader = new NetworkReader(m_ReceiveBuffer);
                        m_LastReceivedMessage = m_Reader.ReadString();
                        HandleMessage(m_LastReceivedMessage, connectionId);
                        break;
                    }
                case NetworkEventType.DisconnectEvent:
                    {
                        Debug.Log("Connection disconnected, ConnectionID:" + connectionId);
                        break;
                    }
                case NetworkEventType.Nothing:
                    break;
            }
        } while (networkEvent != NetworkEventType.Nothing);
    }

    override protected void StartServer(string relayIp, int relayPort, NetworkID networkId, NodeID nodeId)
    {
        base.StartServer(relayIp, relayPort, networkId, nodeId);

        TeamInit();

        m_isStarted = true;
        m_isServer = true;
        SceneManager.LoadScene("Scene1");
    }

    void TeamInit()
    {
        ServerClient c = new ServerClient();
        c.m_connectionId = 0;
        c.m_playerName = "SERVER";
        
        string teamString = "";
        for (int i = 0; i < 4; i++)
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
    }

    private void HandleMessage(string _msg, int _conId)
    {
        Debug.Log(_msg);

        string[] splitData = _msg.Split('~');

        switch (splitData[0])
        {
            // SERVER
            case "NAMEIS":
                OnNameIs(splitData[1], _conId);
                break;
            case "TEAMIS":
                OnTeamIs(splitData[1], _conId);
                break;

            // CLIENT
            case "ASKNAME":
                OnAskName(splitData);
                break;
            case "READY":
                GameObject.Find("Scene Manager").GetComponent<NetworkedGameScript>().ClientInit(splitData[1]);
                break;
            case "NEWROUND":
                GameObject.Find("Scene Manager").GetComponent<NetworkedGameScript>().ClientRoundInit(splitData[1]);
                break;
            case "CON":
                SpawnCharacters(splitData[1], int.Parse(splitData[2]));
                break;

            // BOTH
            case "MOVESTART":
                OnMove(splitData[1]);
                break;
            case "ACTSTART":
                OnAct(splitData[1]);
                break;
            case "NEWTURN":
                GameObject.Find("Scene Manager").GetComponent<NetworkedGameScript>().NewTurn(false);
                break;

            //case "DC":
            //    PlayerDisconnected(int.Parse(splitData[1]));
            //    break;

            default:
                Debug.Log("Invalid Message: " + _msg);
                break;
        }
    }

    public void SendMessageCUSTOM(string _msg)
    {
        m_Writer.SeekZero();
        m_Writer.Write(_msg);
        byte error;

        if (m_isServer)
            for (int i = 0; i < m_ConnectionIds.Count; ++i)
            {
                NetworkTransport.Send(m_HostId,
                    m_ConnectionIds[i], 0, m_Writer.AsArray(), m_Writer.Position, out error);
                if ((NetworkError)error != NetworkError.Ok)
                    Debug.LogError("Failed to send message: " + (NetworkError)error);
            }
        else
        {
            NetworkTransport.Send(m_HostId,
                    m_ConnectionIds[0], 0, m_Writer.AsArray(), m_Writer.Position, out error);
            if ((NetworkError)error != NetworkError.Ok)
                Debug.LogError("Failed to send message: " + (NetworkError)error);

        }
    }

    public void CreateMatch()
    {
        m_NetworkMatch.CreateMatch(m_MatchName, 4, true, "", "", "", 0, 0, OnMatchCreate);
    }

    public void JoinFirst()
    {
        m_NetworkMatch.ListMatches(0, 1, "", true, 0, 0, (success, info, matches) =>
        {
            if (success && matches.Count > 0)
                m_NetworkMatch.JoinMatch(matches[0].networkId, "", "", "", 0, 0, OnMatchJoined);
        });
    }

    public bool CheckIfMine(GameObject _obj)
    {
        CharacterScript cScript = _obj.GetComponent<CharacterScript>();

        if (!m_isStarted || cScript.m_player.m_num == m_ourClientId)
            return true;

        return false;
    }




    // SERVER
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

        foreach (ServerClient sc in m_clients)
            msg += sc.m_playerName + '%' + sc.m_connectionId + '~';

        msg = msg.Trim('~');

        // ASKNAME|3|DAVE%1|MICHAEL%2|TEMP%3

        SendMessageCUSTOM(msg);
    }
    private void OnNameIs(string _name, int _conId)
    {
        // Link the name to the connection Id
        m_clients.Find(x => x.m_connectionId == _conId).m_playerName = _name;

        // Tell everybody that a new player has connected
        SendMessageCUSTOM("CON~" + _name + '~' + _conId);
    }
    
    private void OnTeamIs(string _team, int _conId)
    {
        // Link the name to the connection Id
        m_clients.Find(x => x.m_connectionId == _conId).m_team = _team;
    
        //// Tell everybody that a new player has connected
        //Send("CON~" + _name + '~' + _conId, m_reliableChannel, m_clients);
    }
    private void OnDisconnection(int _conId)
    {
        // Remove this player from our client list
        m_clients.Remove(m_clients.Find(x => x.m_connectionId == _conId));

        // Tell everyone that someone else has disconnected
        SendMessageCUSTOM("DC~" + _conId);
    }
    


    // CLIENT
    private void OnAskName(string[] _data)
    {
        // Set this client's ID
        m_ourClientId = int.Parse(_data[1]);
        
        // Send our name to the server
        SendMessageCUSTOM("NAMEIS~" + "FRANK");
        
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
    
        for (int i = 0; i < 4; i++)
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

        SendMessageCUSTOM(teamString);
    
        //Player p = new Player();
        //m_players.Add(_conId, p);
        //p.m_chars = 
    }
    
    private void OnMove(string _data)
    {
        string[] split = _data.Split('|');
        int objId = int.Parse(split[0]);
        int tileId = int.Parse(split[1]);
        bool isForced = bool.Parse(split[2]);

        m_netGame.m_netOBJs[objId].GetComponent<ObjectScript>().MovingStart(m_board.m_tiles[tileId], isForced, true);
    }

    private void OnAct(string _data)
    {
        string[] split = _data.Split('|');
        int objId = int.Parse(split[0]);
        int tileId = int.Parse(split[1]);
        string actName = split[2];
        string[] df = split[3].Split(',');
        List<int> targetIds = new List<int>(Array.ConvertAll(split[3].Split(','), int.Parse));

        m_board.m_selected = m_board.m_tiles[tileId];
        ActionScript action = m_netGame.m_netOBJs[objId].GetComponent<CharacterScript>().FindActionByName(actName);
        m_netGame.m_currCharScript.m_currAction = action;

        for (int i = 0; i < targetIds.Count; i++)
            m_netGame.m_currCharScript.m_targets.Add(m_netGame.m_netOBJs[targetIds[i]].gameObject);
        
        action.ActionStart(true);
    }
}