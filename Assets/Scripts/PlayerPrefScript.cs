using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPrefScript : MonoBehaviour {

    public enum netwrkPak { NAME, COLOR, ACTIONS, EXP, LVL, GENDER, AI, STATS, HP, TEAM, POS}

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    static public void SaveChar(string _key, CharacterScript _charScript, Color _buttonColor)
    {
        // Save out pertinent character data
        PlayerPrefs.SetString(_key + ",name", _charScript.m_name);
        PlayerPrefs.SetString(_key + ",color", _charScript.m_color);
        PlayerPrefs.SetString(_key + ",actions", string.Join(";", _charScript.m_actNames));
        PlayerPrefs.SetString(_key + ",exp", _charScript.m_exp.ToString());
        PlayerPrefs.SetString(_key + ",level", _charScript.m_level.ToString());
        PlayerPrefs.SetString(_key + ",gender", _charScript.m_gender.ToString());
        PlayerPrefs.SetString(_key + ",AI", _charScript.m_isAI.ToString());
        PlayerPrefs.SetString(_key + ",TeamColor", _buttonColor.ToString());

        // Write out stats
        string stats = null;
        for (int i = 0; i < _charScript.m_stats.Length; i++)
        {
            if (i != 0)
                stats += ",";
            stats += _charScript.m_stats[i];
        }
        PlayerPrefs.SetString(_key + ",stats", stats);
        PlayerPrefs.SetString(_key + ",HP", _charScript.m_totalHealth.ToString());
    }

    static public CharacterScript LoadChar(string _key, CharacterScript _charScript)
    {
        _charScript.m_name = PlayerPrefs.GetString(_key + ",name");
        _charScript.m_color = PlayerPrefs.GetString(_key + ",color");
        _charScript.m_actNames = PlayerPrefs.GetString(_key + ",actions").Split(';');
        _charScript.m_exp = int.Parse(PlayerPrefs.GetString(_key + ",exp"));
        _charScript.m_level = int.Parse(PlayerPrefs.GetString(_key + ",level"));
        _charScript.m_gender = int.Parse(PlayerPrefs.GetString(_key + ",gender"));
        _charScript.m_isAI = bool.Parse(PlayerPrefs.GetString(_key + ",AI"));

        string[] teamColor = PlayerPrefs.GetString(_key + ",TeamColor").Split('(')[1].Split(',');
        _charScript.m_teamColor = new Color(float.Parse(teamColor[0]), float.Parse(teamColor[1]), float.Parse(teamColor[2]));

        // Load in stats
        string[] stats = PlayerPrefs.GetString(_key + ",stats").Split(',');

        if (_charScript.m_stats.Length == 0)
        {
            _charScript.m_stats = new int[(int)CharacterScript.sts.TOT];
            _charScript.m_tempStats = new int[(int)CharacterScript.sts.TOT];
            _charScript.InitializeStats();
        }

        for (int i = 0; i < _charScript.m_stats.Length; i++)
        {
            _charScript.m_stats[i] = int.Parse(stats[i]);
            _charScript.m_tempStats[i] = int.Parse(stats[i]);
        }

        _charScript.m_totalHealth = int.Parse(PlayerPrefs.GetString(_key + ",HP"));
        _charScript.m_currHealth = _charScript.m_totalHealth;

        _charScript.m_isAlive = true;
        _charScript.m_effects = new bool[(int)StatusScript.effects.TOT];

        return _charScript;
    }

    static public string PackageForNetwork(int _ind)
    {
        string netString = "";
        string key = "0," + _ind.ToString();
        char symbol = '|';

        netString += PlayerPrefs.GetString(key + ",name") + symbol;
        netString += PlayerPrefs.GetString(key + ",color") + symbol;
        string[] actions = PlayerPrefs.GetString(key + ",actions").Split(';');
        for (int i = 0; i < actions.Length; i++)
            netString += actions[i] + ',';
        netString = netString.Trim(','); netString += symbol;
        netString += PlayerPrefs.GetString(key + ",exp") + symbol;
        netString += PlayerPrefs.GetString(key + ",level") + symbol;
        netString += PlayerPrefs.GetString(key + ",gender") + symbol;
        netString += PlayerPrefs.GetString(key + ",AI") + symbol;
        netString += PlayerPrefs.GetString(key + ",stats") + symbol;
        netString += PlayerPrefs.GetString(key + ",HP");

        return netString;
    }

    static public CharacterScript ReadCharNetData(string[] _data, CharacterScript _char)
    {
        _char.m_name = _data[(int)netwrkPak.NAME];
        _char.m_color = _data[(int)netwrkPak.COLOR];

        _char.m_actNames = _data[(int)netwrkPak.ACTIONS].Split(',');
        GameObject.Find("Scene Manager").GetComponent<ActionLoaderScript>().AddActions(_char);

        //_char.m_actions = actionsForChar;
        _char.m_exp = int.Parse(_data[(int)netwrkPak.EXP]);
        _char.m_level = int.Parse(_data[(int)netwrkPak.LVL]);
        _char.m_gender = int.Parse(_data[(int)netwrkPak.GENDER]);
        _char.m_isAI = bool.Parse(_data[(int)netwrkPak.AI]);

        // Load in stats
        string[] stats = _data[(int)netwrkPak.STATS].Split(',');

        if (_char.m_stats.Length == 0)
        {
            _char.m_stats = new int[(int)CharacterScript.sts.TOT];
            _char.m_tempStats = new int[(int)CharacterScript.sts.TOT];
            _char.InitializeStats();
        }

        for (int k = 0; k < _char.m_stats.Length; k++)
        {
            _char.m_stats[k] = int.Parse(stats[k]);
            _char.m_tempStats[k] = int.Parse(stats[k]);
        }

        _char.m_totalHealth = int.Parse(_data[(int)netwrkPak.HP]);
        _char.m_currHealth = _char.m_totalHealth;

        return _char;
    }
}
