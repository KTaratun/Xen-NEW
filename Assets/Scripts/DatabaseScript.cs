using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DatabaseScript : MonoBehaviour {

    public enum actions { ID, NAME, ENERGY, HIT, DMG, RNG, RAD, CRT, EFFECT, TOT };
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
        WWW actionData = new WWW("http://localhost:8081/Xen_New/ActionsData.php");
        yield return actionData;
        string actionDataString = actionData.text;
        m_actions = actionDataString.Split(';');

        WWW presetsData = new WWW("http://localhost:8081/Xen_New/PresetsData.php");
        yield return presetsData;
        string presetsDataString = presetsData.text;
        m_presets = presetsDataString.Split(';');

        WWW statData = new WWW("http://localhost:8081/Xen_New/StatData.php");
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
}
