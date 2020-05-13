using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NetworkedGameScript : GameManagerScript
{
    public List<ObjectScript> m_netOBJs; // List of all objects for the network to index through

    // Start is called before the first frame update
    override protected void Start()
    {
        base.Start();

        m_netOBJs = new List<ObjectScript>();

        GameObject.Find("Network").GetComponent<CustomDirect>().m_netGame = this;
    }

    // Update is called once per frame
    new protected void Update()
    {
        base.Update();
    }

    override protected IEnumerator StartBattle()
    {
        m_board.RandomEnvironmentInit();

        if (!m_field)
        {
            if (GameObject.Find("Network").GetComponent<CustomDirect>().m_isStarted)
                ServerInit();
            else
                m_board.TeamInit();
        }

        m_camera.GetComponent<BoardCamScript>().m_zoomIn = true;
        m_camera.GetComponent<BoardCamScript>().m_rotate = false;
        m_panMan.GetPanel("HUD Panel LEFT").m_inView = true;
        m_panMan.GetPanel("Round Panel").m_inView = true;

        yield return new WaitForSeconds(.1f);

        NewTurn(false);
    }

    override public void NewRound()
    {
        base.NewRound();

        if (GameObject.Find("Network").GetComponent<CustomDirect>().m_isStarted)
        {
            CustomDirect network = GameObject.Find("Network").GetComponent<CustomDirect>();
            string roundData = PackUpRoundData();
            network.SendMessageCUSTOM(roundData);
        }
    }

    // Network
    public void ServerInit()
    {
        CustomDirect network = GameObject.Find("Network").GetComponent<CustomDirect>();

        for (int i = 0; i < network.m_clients.Count; i++)
        {
            PlayerScript playScript = m_board.m_players[i];
            playScript.m_characters = new List<CharacterScript>();
            playScript.m_bScript = m_board;
            playScript.m_energy = new int[4];

            //playScript.name = "Team " + (i + 1).ToString();

            string[] chars = network.m_clients[i].m_team.Split(';');

            for (int j = 0; j < chars.Length; j++)
            {
                string[] data = chars[j].Split('|');

                // Set up character
                GameObject[] characterTypes = Resources.LoadAll<GameObject>("OBJs/Characters");
                GameObject newChar = Instantiate(characterTypes[int.Parse(data[(int)PlayerPrefScript.netwrkPak.GENDER])]);
                newChar.name = data[(int)PlayerPrefScript.netwrkPak.NAME];

                CharacterScript cScript = newChar.GetComponent<CharacterScript>();
                cScript = PlayerPrefScript.ReadCharNetData(data, cScript);

                m_board.m_characters.Add(cScript);

                // Link to player
                cScript.m_player = playScript;
                playScript.name = "Team " + i.ToString();
                playScript.m_num = i;

                if (i == 0)
                    cScript.m_teamColor = new Color(0.906f, 0.638f, 0.322f, 1.000f);
                else if (i == 1)
                    cScript.m_teamColor = new Color(0.5489212f, 0.3215686f, 0.9058824f, 1.000f);

                cScript.CharInit();

                playScript.m_characters.Add(cScript);
                newChar.GetComponent<ObjectScript>().PlaceRandomly(m_board);
            }
        }

        string initData = PackUpInitData();
        network.SendMessageCUSTOM(initData);
    }

    public void ClientInit(string _data)
    {
        GameObject[] environmentalOBJs = Resources.LoadAll<GameObject>("OBJs/Environmental");

        string[] objs = _data.Split(';')[0].Split('|');
        string[] chars = _data.Split(';')[1].Split('/');

        for (int i = 0; i < objs.Length; i++)
        {
            string[] obj = objs[i].Split(',');
            GameObject newOBJ = m_board.SpawnEnvironmentOBJ(obj[0], int.Parse(obj[1]), int.Parse(obj[2]), int.Parse(obj[3]));

            //GameObject newOBJ = Instantiate(environmentalOBJs[int.Parse(obj[0])]);
            //newOBJ.GetComponent<ObjectScript>().SetRotation((TileLinkScript.nbors)int.Parse(obj[1]));
            //newOBJ.GetComponent<ObjectScript>().PlaceOBJ(m_board, int.Parse(obj[2]), int.Parse(obj[3]));
            m_board.m_obstacles.Add(newOBJ);
        }

        GameObject[] characterTypes = Resources.LoadAll<GameObject>("OBJs/Characters");
        for (int i = 0; i < chars.Length; i++)
        {
            string[] character = chars[i].Split('|');

            GameObject newChar = Instantiate(characterTypes[int.Parse(character[(int)PlayerPrefScript.netwrkPak.GENDER])]);
            newChar.name = character[(int)PlayerPrefScript.netwrkPak.NAME];

            CharacterScript cScript = newChar.GetComponent<CharacterScript>();
            cScript = PlayerPrefScript.ReadCharNetData(character, cScript);

            int team = int.Parse(character[(int)PlayerPrefScript.netwrkPak.TEAM]);

            m_board.m_characters.Add(cScript);

            PlayerScript p = m_board.m_players[team];

            p.m_characters = new List<CharacterScript>();
            p.m_bScript = m_board;
            p.m_energy = new int[4];
            p.m_num = team;

            // Link to player
            p.m_characters.Add(cScript);
            cScript.m_player = p;

            if (team == 0)
                cScript.m_teamColor = new Color(0.906f, 0.638f, 0.322f, 1.000f);
            else if (team == 1)
                cScript.m_teamColor = new Color(0.5489212f, 0.3215686f, 0.9058824f, 1.000f);

            cScript.CharInit();

            // Place on Board
            string[] pos = character[(int)PlayerPrefScript.netwrkPak.POS].Split(',');
            cScript.PlaceOBJ(m_board, int.Parse(pos[0]), int.Parse(pos[1]));
        }

        m_camera.GetComponent<BoardCamScript>().m_zoomIn = true;
        m_camera.GetComponent<BoardCamScript>().m_rotate = false;
        m_panMan.GetPanel("HUD Panel LEFT").m_inView = true;
        m_panMan.GetPanel("Round Panel").m_inView = true;
    }

    public void ClientRoundInit(string _data)
    {
        string[] roundOrder = _data.Split(';')[0].Split(',');
        string[] charSpeed = _data.Split(';')[1].Split(',');
        string[] powerUp = _data.Split(';')[2].Split(',');

        for (int i = 0; i < roundOrder.Length; i++)
        {
            CharacterScript charScript = m_netOBJs[int.Parse(roundOrder[i])].GetComponent<CharacterScript>();
            m_currRound.Add(charScript);
        }

        for (int i = 0; i < charSpeed.Length; i++)
            m_board.m_characters[i].GetComponent<CharacterScript>().m_tempStats[(int)CharacterScript.sts.SPD] = int.Parse(charSpeed[i]);

        m_livingPlayersInRound = m_currRound.Count;
        m_roundCount++;
        PanelScript roundPanScript = m_panMan.GetPanel("Round Panel");
        roundPanScript.PopulatePanel();

        GameObject.Find("Turn Panel").GetComponent<TurnPanelScript>().NewTurnOrder();

        m_board.SpawnItem(int.Parse(powerUp[0]), int.Parse(powerUp[1]), int.Parse(powerUp[2]));

        SlidingPanelScript roundPan = m_panMan.GetPanel("Round End Panel");
        roundPan.m_inView = true;
        roundPan.GetComponentInChildren<Text>().text = "NEW ROUND";

        NewTurn(true);
    }

    public string PackUpInitData()
    {
        string data = "READY~";
        for (int i = 0; i < m_board.m_obstacles.Count; i++)
        {
            ObjectScript obj = m_board.m_obstacles[i].GetComponent<ObjectScript>();

            data += obj.m_name  + "," + obj.m_tile.m_x.ToString() + "," + obj.m_tile.m_z.ToString() + "," + (int)obj.m_facing + "|";
        }

        data = data.Trim('|') + ';';

        for (int i = 0; i < m_board.m_players.Length; i++)
        {
            string charString;
            for (int j = 0; j < m_board.m_players[i].m_characters.Count; j++)
            {
                CharacterScript charScript = m_board.m_players[i].m_characters[j];
                charString = "";
                char symbol = '|';

                charString += charScript.m_name + symbol;
                charString += charScript.m_color + symbol;

                string actNums = null;
                for (int k = 0; k < charScript.m_actNames.Length; k++)
                {
                    if (k != 0)
                        actNums += ",";
                    actNums += charScript.m_actNames[k];
                }

                charString += actNums + symbol;

                charString += charScript.m_exp.ToString() + symbol;
                charString += charScript.m_level.ToString() + symbol;
                charString += charScript.m_gender.ToString() + symbol;
                charString += charScript.m_isAI.ToString() + symbol;


                string stats = null;
                for (int k = 0; k < charScript.m_stats.Length; k++)
                {
                    if (k != 0)
                        stats += ",";
                    stats += charScript.m_stats[k];
                }

                charString += stats + symbol;

                charString += charScript.m_totalHealth.ToString() + symbol;

                charString += i.ToString() + symbol;

                charString += charScript.m_tile.m_x.ToString() + ',' + charScript.m_tile.m_z.ToString();

                data += charString + "/";
            }
        }

        data = data.Trim('/');

        return data;
    }

    public string PackUpRoundData()
    {
        string data = "NEWROUND~";

        for (int i = 0; i < m_currRound.Count; i++)
            data += m_currRound[i].GetComponent<CharacterScript>().m_id.ToString() + ",";

        data = data.Trim(',') + ';';

        // Update speed
        for (int i = 0; i < m_board.m_characters.Count; i++)
            data += m_board.m_characters[i].GetComponent<CharacterScript>().m_tempStats[(int)CharacterScript.sts.SPD].ToString() + ",";

        data = data.Trim(',') + ';';

        PowerupScript pUp = m_board.m_powerUp.GetComponent<PowerupScript>();


        data += pUp.name + ',';
        data += m_board.m_powerUp.m_tile.m_x.ToString() + ',' + m_board.m_powerUp.m_tile.m_z.ToString();

        return data;
    }

    public void AddToOBJList(ObjectScript _obj)
    {
        _obj.m_id = m_netOBJs.Count;
        m_netOBJs.Add(_obj);
        //for (int i = 0; i < m_netOBJs.Length; i++)
        //{
        //    if (m_netOBJs[i] == null)
        //    {
        //        _obj.m_id = i;
        //        m_netOBJs[i] = _obj.gameObject;
        //        return;
        //    }
        //}
    }
}
