﻿using System.Collections;
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

    //public void CalculateValue(string _action, CharacterScript _owner, CharacterScript _target)
    //{
    //    m_value = 0;
    //    m_targets.Add(_target.gameObject);
    //    m_action = _action;

    //    string eng = DatabaseScript.GetActionData(m_action, DatabaseScript.actions.ENERGY);
    //    int dmg = int.Parse(DatabaseScript.GetActionData(m_action, DatabaseScript.actions.DMG)) + _owner.m_tempStats[(int)CharacterScript.sts.DMG] - _target.m_tempStats[(int)CharacterScript.sts.DEF];
    //    int rad = int.Parse(DatabaseScript.GetActionData(m_action, DatabaseScript.actions.RAD)) + _owner.m_tempStats[(int)CharacterScript.sts.RAD];

    //    if (rad > 0)
    //    {
    //        if (CalculateRadius(_action, _owner));
    //            return;
    //    }

    //    if (_target.m_tempStats[(int)CharacterScript.sts.HP] - dmg <= 0)
    //        m_value += 100 - ActionScript.ConvertedCost(eng);
    //    else
    //        m_value += 50 + ActionScript.ConvertedCost(eng);
    //}

    private bool CalculateRadius(ActionScript _action, CharacterScript _owner)
    {
        //TileScript originalTile = m_targets[0].GetComponent<CharacterScript>().m_tile;
        //originalTile.FetchTilesWithinRange(_owner, _action.m_radius, TileScript.c_radius, true, TileScript.targetRestriction.NONE, false);
        ////List<TileScript> tiles = originalTile.m_targetRadius;
        //
        //for (int i = 0; i < originalTile.m_targetRadius.Count; i++)
        //{
        //    TileScript tarTile = originalTile.m_targetRadius[i];
        //    if (tarTile.m_holding && tarTile.m_holding.tag == "Player" && tarTile.m_holding != m_targets[0] && tarTile.m_holding.GetComponent<CharacterScript>().m_isAlive)
        //        m_targets.Add(tarTile.m_holding);
        //}
        //
        //if (originalTile.m_targetRadius.Count > 0)
        //    originalTile.ClearRadius();
        //
        //int tarVal = 0;
        //for (int i = 0; i < m_targets.Count; i++)
        //{
        //    if (m_targets[i].GetComponent<CharacterScript>().m_player == _owner.m_player)
        //        tarVal--;
        //    else
        //        tarVal++;
        //}
        //
        //if (tarVal > 1)
        //{
        //    m_value += 200;
        //    return true;
        //}
        //
        return false;

        //for (int i = 0; i < length; i++)
        //{
        //
        //}
    }
}
