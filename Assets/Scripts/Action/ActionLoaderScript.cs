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
        m_allActions = m_allActions.Union(m_aC.m_blueActions).ToList();
        m_allActions = m_allActions.Union(m_aC.m_redActions).ToList();
    }

    public void AddActions(CharacterScript _charScript)
    {
        string[] actNames = _charScript.m_actNames;

        foreach (string actName in actNames)
        {
            foreach (Act act in m_allActions)
                if (act.m_name == actName)
                {
                    PopulateAction(_charScript, act);
                    break;
                }
        }
    }

    private void PopulateAction(CharacterScript _charScript, Act _act)
    {
        ActionScript aS = _charScript.gameObject.AddComponent<ActionScript>();
        aS.m_name = _act.m_name;
        aS.m_energy = _act.m_energy;
        aS.m_damage = _act.m_damage;
        aS.m_range = _act.m_range;
        aS.m_radius = _act.m_radius;
        aS.m_effect = _act.m_effect;

        _charScript.m_actions.Add(aS);
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
