using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldScript : MonoBehaviour {

    public CharacterScript m_mainChar;
    public BoardScript m_board;

	// Use this for initialization
	void Start ()
    {
        //GameObject newChar = Instantiate(m_character);
        //m_mainChar = newChar.GetComponent<CharacterScript>();

        Invoke("MainCharInit", .1f);
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

    }

    private void MainCharInit()
    {
        m_mainChar.gameObject.name = "???";
        m_mainChar.m_name = "???";

        string[] acts = new string[1];
        acts[0] = "ATK(Push)";
        m_mainChar.m_actions = acts;
        m_mainChar.RetrieveActions();

        m_mainChar.m_isDiabled = new int[m_mainChar.m_actions.Length];
        for (int i = 0; i < m_mainChar.m_isDiabled.Length; i++)
            m_mainChar.m_isDiabled[i] = 0;

        // Load in stats
        string[] stats = { "12", "10", "5", "0", "0", "0", "0", "0" };

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
    }

    private void StoryCharInit(CharacterScript _char, string _name, string[] _acts, int _team, string[] _stats)
    {
        _char.gameObject.name = _name;
        _char.m_name = _name;
        _char.m_actions = _acts;
        if (_team == 0)
            _char.m_isAI = false;
        else
            _char.m_isAI = true;
        _char.m_color = PlayerScript.CheckCharColors(_char.m_actions);

        _char.m_isDiabled = new int[_char.m_actions.Length];
        for (int i = 0; i < _char.m_isDiabled.Length; i++)
            _char.m_isDiabled[i] = 0;

        // Load in stats
        if (_char.m_stats.Length == 0)
        {
            _char.m_stats = new int[(int)CharacterScript.sts.TOT];
            _char.m_tempStats = new int[(int)CharacterScript.sts.TOT];
            _char.InitializeStats();
        }

        for (int i = 0; i < _char.m_stats.Length; i++)
        {
            _char.m_stats[i] = int.Parse(_stats[i]);
            _char.m_tempStats[i] = int.Parse(_stats[i]);
        }

        _char.m_isAlive = true;
        _char.m_effects = new bool[(int)StatusScript.effects.TOT];




        _char.SetPopupSpheres("");
        m_board.m_characters.Add(_char.gameObject);

        if (_char.m_hasActed.Length == 0)
            _char.m_hasActed = new int[2];

        _char.m_hasActed[0] = 0;
        _char.m_hasActed[1] = 0;

        // Link to player
        PlayerScript playScript = m_board.m_players.GetComponents<PlayerScript>()[_team];
        _char.m_player = playScript;
        playScript.m_characters.Add(_char);
    }
}
