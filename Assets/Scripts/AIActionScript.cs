using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIActionScript : MonoBehaviour {

    public int m_value;
    public string m_action;
    public TileScript m_position;
    public List<GameObject> m_targets;

	// Use this for initialization
	void Start ()
    {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void CalculateValue(string _action, CharacterScript _target)
    {
        m_value = 0;
        m_targets.Add(_target.gameObject);
        m_action = _action;

        string eng = DatabaseScript.GetActionData(m_action, DatabaseScript.actions.ENERGY);
        int dmg = int.Parse(DatabaseScript.GetActionData(m_action, DatabaseScript.actions.DMG)) - _target.m_tempStats[(int)CharacterScript.sts.DEF];
        int rad = int.Parse(DatabaseScript.GetActionData(m_action, DatabaseScript.actions.RAD)) - _target.m_tempStats[(int)CharacterScript.sts.RAD];

        //if (rad > 0)
        //{
        //    CalculateRadius(_action, _target);
        //    return;
        //}

        if (_target.m_tempStats[(int)CharacterScript.sts.HP] - dmg <= 0)
            m_value += 100 - CharacterScript.ConvertedCost(eng);
        else
            m_value += 50 + CharacterScript.ConvertedCost(eng);
    }

    private void CalculateRadius(string _action, CharacterScript _target)
    {
        int rad = int.Parse(DatabaseScript.GetActionData(m_action, DatabaseScript.actions.RAD)) - _target.m_tempStats[(int)CharacterScript.sts.RAD];

        TileScript originalTile = m_targets[0].GetComponent<CharacterScript>().m_tile;
        originalTile.FetchTilesWithinRange(rad, Color.yellow, true, TileScript.targetRestriction.NONE, false);
        List<TileScript> tiles = originalTile.m_targetRadius;

        for (int i = 0; i < originalTile.m_targetRadius.Count; i++)
        {
            TileScript tarTile = originalTile.m_targetRadius[i].GetComponent<TileScript>();
            if (tarTile.m_holding && tarTile.m_holding.tag == "Player")
                m_targets.Add(tarTile.m_holding);


        }


        //for (int i = 0; i < length; i++)
        //{
        //
        //}
    }
}
