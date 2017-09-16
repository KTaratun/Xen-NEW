using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPrefScript : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    static public void SaveChar(string _key, CharacterScript _charScript)
    {
        // Save out pertinent character data
        PlayerPrefs.SetString(_key + ",name", _charScript.m_name);
        PlayerPrefs.SetString(_key + ",color", _charScript.m_color);
        PlayerPrefs.SetString(_key + ",actions", string.Join(";", _charScript.m_actions));
        PlayerPrefs.SetString(_key + ",exp", _charScript.m_exp.ToString());
        PlayerPrefs.SetString(_key + ",level", _charScript.m_level.ToString());
        PlayerPrefs.SetString(_key + ",gender", _charScript.m_gender.ToString());

        // Write out stats
        string stats = null;
        for (int i = 0; i < _charScript.m_stats.Length; i++)
        {
            if (i != 0)
                stats += ",";
            stats += _charScript.m_stats[i];
        }
        PlayerPrefs.SetString(_key + ",stats", stats);
    }

    static public CharacterScript LoadChar(string _key, CharacterScript _charScript)
    {
        _charScript.m_name = PlayerPrefs.GetString(_key + ",name");
        _charScript.m_color = PlayerPrefs.GetString(_key + ",color");
        _charScript.m_actions = PlayerPrefs.GetString(_key + ",actions").Split(';');
        _charScript.m_exp = int.Parse(PlayerPrefs.GetString(_key + ",exp"));
        _charScript.m_level = int.Parse(PlayerPrefs.GetString(_key + ",level"));
        _charScript.m_gender = int.Parse(PlayerPrefs.GetString(_key + ",gender"));

        _charScript.m_isDiabled = new int[_charScript.m_actions.Length];
        for (int i = 0; i < _charScript.m_isDiabled.Length; i++)
            _charScript.m_isDiabled[i] = 0;

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

        _charScript.m_isAlive = true;
        _charScript.m_currRadius = 0;
        _charScript.m_effects = new bool[(int)StatusScript.effects.TOT];

        return _charScript;
    }
}
