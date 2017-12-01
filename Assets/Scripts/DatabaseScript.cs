using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;

public class DatabaseScript : MonoBehaviour {

    public enum actions { ID, NAME, ENERGY, DMG, RNG, RAD, EFFECT, TOT };
    public enum presets { ID, NAME, COLORS, ACTIONS, EQUIPMENT, TOT }

    public string[] m_actions;
    public string[] m_presets;
    public string[] m_stat;
    // Character Keys: index in 2D array (2,1) and one of the following:
    // (,name), (,actions), (,stats), (,equipment), (,color)
    // ex. 0,1,name to retrieve the name of the second character slot of the first team.

	// Use this for initialization
	IEnumerator Start ()
    {
        //string ip = "http://" + Network.player.ipAddress;
        string ip = "http://192.168.1.15";

        //if (!GameObject.Find("Network").GetComponent<ServerScript>().m_isStarted)
        //{
        //    GameObject.Find("Network").GetComponent<ClientScript>().m
        //}

        string acts = ip + ":8081/Xen_New/ActionsData.php";
        WWW actionData = new WWW(acts);
        yield return actionData;
        string actionDataString = actionData.text;
        m_actions = actionDataString.Split(';');

        string presets = ip + ":8081/Xen_New/PresetsData.php";
        WWW presetsData = new WWW(presets);
        yield return presetsData;
        string presetsDataString = presetsData.text;
        m_presets = presetsDataString.Split(';');

        string stats = ip + ":8081/Xen_New/StatData.php";
        WWW statData = new WWW(stats);
        yield return statData;
        string statDataString = statData.text;
        m_stat = statDataString.Split(';');
    }

	// Update is called once per frame
	void Update () {
		
	}

    public string GetDataValue(string data, string index)
    {
        string value = data.Substring(data.IndexOf(index)+index.Length);
        if (value.Contains("|"))
            value = value.Remove(value.IndexOf("|"));
        return value;
    }

    public string[] GetActions(string data)
    {
        string index = "Actions:";
        string value = data.Substring(data.IndexOf(index) + index.Length);
        if (value.Contains("|"))
            value = value.Remove(value.IndexOf("|"));

        string[] acts = value.Split(',');

        for (int i = 0; i < acts.Length; i++)
            acts[i] = m_actions[int.Parse(acts[i]) - 1];

        return acts;
    }

    static public string GetActionData(string _action, actions _data)
    {
        string[] actSeparated = _action.Split('|');
        string[] data = actSeparated[(int)_data].Split(':');

        return data[1];
    }

    static public string ModifyActions(int _tec, string _action)
    {
        string newString = "";
        for (int i = 0; i < _action.Length; i++)
        {
            if (i < _action.Length - 1 && _action[i + 1] == '*')
            {
                int numConverted = _action[i] - '0';
                if (numConverted == -16)
                {
                    newString += ' ';
                    numConverted = 0;
                }
                int moddedNum = numConverted + _tec;
                string moddedString = moddedNum.ToString();
                newString += moddedString;
            }
            else
                newString += _action[i];
        }
        return newString;
    }
}
