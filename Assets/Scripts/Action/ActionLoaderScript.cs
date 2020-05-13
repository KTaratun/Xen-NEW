using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ActionLoaderScript : MonoBehaviour
{
    private ActionContainerScript m_aC;

    // Update is called once per frame
    public List<Act> m_allActions = new List<Act>();

    // Use this for initialization
    void Start()
    {
        m_aC = ActionContainerScript.Load();

        m_allActions = m_allActions.Union(m_aC.m_whiteActions).ToList();
        m_allActions = m_allActions.Union(m_aC.m_greenActions).ToList();
        m_allActions = m_allActions.Union(m_aC.m_redActions).ToList();
        m_allActions = m_allActions.Union(m_aC.m_blueActions).ToList();
    }

    public void AddActions(CharacterScript _charScript)
    {
        string[] actNames = _charScript.m_actNames;

        int count;

        foreach (string actName in actNames)
        {
            count = 0;
            foreach (Act act in m_allActions)
            {
                if (act.m_name == actName)
                {
                    PopulateAction(_charScript, act, count);
                    break;
                }
                count++;
            }
        }
    }

    private void PopulateAction(CharacterScript _charScript, Act _act, int _ind)
    {
        ActionScript aS = _charScript.gameObject.AddComponent<ActionScript>();

        aS.m_name = _act.m_name;
        aS.m_energy = _act.m_energy;
        aS.m_animation = _act.m_animation;
        aS.m_effect = _act.m_effect;
        aS.m_id = _ind;

        if (_act.m_name == "ATK(Dark)")
            PopulateDark(aS);
        else
        {
            aS.m_damage = _act.m_damage;
            aS.m_range = _act.m_range;
            aS.m_radius = _act.m_radius;
        }

        _charScript.m_actions.Add(aS);
    }

    public void PopulateDark(ActionScript _aS)
    {
        _aS.m_damage = Random.Range(4, 10);

        int rangeMod = 11 - _aS.m_damage;
        _aS.m_range = Random.Range(1, rangeMod);

        if (_aS.m_range > 2 && _aS.m_damage < 6)
            _aS.m_radius = 2;
        if (_aS.m_range > 2 && _aS.m_damage < 8)
            _aS.m_radius = 1;
    }

    public string ModifyActions(int _tec, string _action)
    {
        // MAKE SPECIAL TEXT FOR REBOOT

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
                if (moddedNum < 0)
                    moddedNum = 0;

                string moddedString = moddedNum.ToString();
                newString += moddedString;
            }
            else
                newString += _action[i];
        }
        return newString;
    }
}
