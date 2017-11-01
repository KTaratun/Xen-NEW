using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldScript : MonoBehaviour {

    public CharacterScript m_mainChar;
    public GameObject m_character;
    public BoardScript m_board;

	// Use this for initialization
	void Start ()
    {
        GameObject newChar = Instantiate(m_character);
        m_mainChar = newChar.GetComponent<CharacterScript>();

        MainCharInit();
    }
	
	// Update is called once per frame
	void Update ()
    {
        if (!m_board || m_board && m_board.m_battle)
            return;

        Inputs();
	}

    private void Inputs()
    {
        //float forwardMoveSpeed = 0.1f;
        //if (Input.GetKey(KeyCode.W))
        //    m_mainChar.transform.SetPositionAndRotation(new Vector3(m_mainChar.transform.position.x + transform.forward.x * forwardMoveSpeed, m_mainChar.transform.position.y, m_mainChar.transform.position.z + transform.forward.z * forwardMoveSpeed), Quaternion.identity);
        //if (Input.GetKey(KeyCode.S))
        //    m_mainChar.transform.SetPositionAndRotation(new Vector3(m_mainChar.transform.position.x + transform.forward.x * -forwardMoveSpeed, m_mainChar.transform.position.y, m_mainChar.transform.position.z + transform.forward.z * -forwardMoveSpeed), Quaternion.identity);
        //if (Input.GetKey(KeyCode.A))
        //    m_mainChar.transform.SetPositionAndRotation(new Vector3(m_mainChar.transform.position.x + transform.right.x * -forwardMoveSpeed, m_mainChar.transform.position.y, m_mainChar.transform.position.z + transform.right.z * -forwardMoveSpeed), Quaternion.identity);
        //if (Input.GetKey(KeyCode.D))
        //    m_mainChar.transform.SetPositionAndRotation(new Vector3(m_mainChar.transform.position.x + transform.right.x * forwardMoveSpeed, m_mainChar.transform.position.y, m_mainChar.transform.position.z + transform.right.z * forwardMoveSpeed), Quaternion.identity);
    }

    private void MainCharInit()
    {
        string[] acts = new string[1];
        acts[0] = "ID:1|Name:ATK(Push)|Energy:gg|Damage:1|Range:1|Radius:0|Effect:Move opponent up to 3 * spaces.";

        m_mainChar.gameObject.name = "???";
        m_mainChar.m_name = "???";
        m_mainChar.m_color = "G";
        m_mainChar.m_actions = acts;
        m_mainChar.m_exp = 0;
        m_mainChar.m_level = 1;
        m_mainChar.m_gender = 0;
        m_mainChar.m_isAI = false;

        m_mainChar.m_isDiabled = new int[m_mainChar.m_actions.Length];
        for (int i = 0; i < m_mainChar.m_isDiabled.Length; i++)
            m_mainChar.m_isDiabled[i] = 0;

        // Load in stats
        string[] stats = { "12", "10", "0", "0", "5", "0", "0", "0" };

        if (m_mainChar.m_stats.Length == 0)
        {
            m_mainChar.m_stats = new int[(int)CharacterScript.sts.TOT];
            m_mainChar.m_tempStats = new int[(int)CharacterScript.sts.TOT];
            m_mainChar.InitializeStats();
        }

        for (int i = 0; i < m_mainChar.m_stats.Length; i++)
        {
            m_mainChar.m_stats[i] = int.Parse(stats[i]);
            m_mainChar.m_tempStats[i] = int.Parse(stats[i]);
        }

        m_mainChar.m_isAlive = true;
        m_mainChar.m_currRadius = 0;
        m_mainChar.m_effects = new bool[(int)StatusScript.effects.TOT];




        m_mainChar.SetPopupSpheres("");
        m_board.m_characters.Add(m_mainChar.gameObject);

        if (m_mainChar.m_hasActed.Length == 0)
            m_mainChar.m_hasActed = new int[2];

        m_mainChar.m_hasActed[0] = 0;
        m_mainChar.m_hasActed[1] = 0;

        // Link to player
        PlayerScript playScript = m_board.m_players[0].GetComponent<PlayerScript>();
        m_mainChar.m_player = playScript;
        playScript.m_characters.Add(m_mainChar);
    }

    public void MainCharJoinBattle()
    {
        TileScript closest = null;
        for (int i = 0; i < m_board.m_tiles.Length; i++)
        {
            if (!closest || Vector3.Distance(m_board.m_tiles[i].gameObject.transform.position, m_mainChar.transform.position) <
                Vector3.Distance(closest.transform.position, m_mainChar.transform.position))
                closest = m_board.m_tiles[i];
        }

        m_mainChar.m_tile = closest;
        m_mainChar.m_boardScript = m_board;
    }
}
