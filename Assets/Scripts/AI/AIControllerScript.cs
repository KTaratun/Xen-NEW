using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIControllerScript : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    //public void AITurn()
    //{
    //    if (!m_anim)
    //        m_anim = GetComponentInChildren<Animator>();

    //    print(m_name + " started their turn.\n");

    //    m_boardScript.m_camIsFrozen = true;

    //    List<AIActionScript> viableActions = new List<AIActionScript>();
    //    for (int i = 0; i < m_boardScript.m_characters.Count; i++)
    //    {
    //        CharacterScript target = m_boardScript.m_characters[i].GetComponent<CharacterScript>();
    //        if (!target.m_isAlive)
    //            continue;

    //        List<TileScript> path = null;
    //        if (!m_effects[(int)StatusScript.effects.IMMOBILE])
    //            path = m_tile.AITilePlanning(target.m_tile);
    //        else
    //            path = new List<TileScript>();

    //        print(m_name + " Chose a path to " + target.m_name + "\n");

    //        AIActionScript newAct = CheckViableActions(target, path);
    //        print(m_name + " Chose optimal action to use on " + target.m_name + "\n");
    //        if (newAct)
    //            viableActions.Add(newAct);
    //    }

    //    AIActionScript mostViable = null;
    //    for (int i = 0; i < viableActions.Count; i++)
    //        if (i == 0 || viableActions[i].m_value > mostViable.m_value)
    //            mostViable = viableActions[i];

    //    if (mostViable && mostViable.m_value > 0)
    //        print(m_name + " decided that attacking " + mostViable.m_targets[0].name + " with " +
    //            DatabaseScript.GetActionData(mostViable.m_action, DatabaseScript.actions.NAME) + " is most viable. \n");
    //    else
    //        print(m_name + " decided that moving is most viable. \n");

    //    if (mostViable && mostViable.m_value > 0)
    //    {
    //        m_targets = mostViable.m_targets;
    //        m_currAction = mostViable.m_action;
    //        if (mostViable.m_position)
    //            MovingStart(mostViable.m_position, false, false);
    //        else
    //        {
    //            m_boardScript.m_selected = m_targets[0].GetComponent<CharacterScript>().m_tile;
    //            transform.LookAt(m_boardScript.m_selected.transform);
    //            if (TileScript.CaclulateDistance(m_tile, m_boardScript.m_selected) > 1)
    //                m_anim.Play("Ranged", -1, 0);
    //            else
    //                m_anim.Play("Melee", -1, 0);

    //            m_hasActed[0] = 3;
    //            m_hasActed[1] = 3;
    //        }
    //    }
    //    else
    //    {
    //        m_currAction = "";
    //        if (mostViable)
    //            MovingStart(mostViable.m_position, false, false);
    //        else
    //            m_boardScript.m_camIsFrozen = false;
    //        m_hasActed[0] = 3;
    //        m_hasActed[1] = 3;
    //    }

    //    for (int i = 0; i < viableActions.Count; i++)
    //        Destroy(viableActions[i]);

    //    print(m_name + " has exited AITurn.\n");

    //    // check all targets
    //    // value will be based on distance if outside of range after moving
    //    // if within range of an attack before or after moving, value is based on end result of best potential action on target
    //}

    //public AIActionScript CheckViableActions(CharacterScript _target, List<TileScript> _path)
    //{
    //    List<AIActionScript> viableActs = new List<AIActionScript>();
    //    TileScript newTile = null;

    //    for (int i = 0; i < m_actions.Length; i++)
    //    {
    //        string name = DatabaseScript.GetActionData(m_actions[i], DatabaseScript.actions.NAME);
    //        string eng = DatabaseScript.GetActionData(m_actions[i], DatabaseScript.actions.ENERGY);

    //        m_currAction = m_actions[i];
    //        bool found = false;

    //        if (m_effects[(int)StatusScript.effects.DELAY] && PlayerScript.CheckIfGains(eng) && eng.Length == 2 ||
    //            m_effects[(int)StatusScript.effects.HINDER] && !CheckIfAttack(name) ||
    //            m_effects[(int)StatusScript.effects.WARD] && CheckIfAttack(name) ||
    //            m_isDiabled[i] > 0)
    //            continue;
    //        else if (_target != this)
    //        {
    //            for (int j = 0; j < _path.Count; j++)
    //            {
    //                if (j > m_tempStats[(int)sts.MOV])
    //                    break;

    //                ActionTargeting(_path[j]);

    //                for (int k = 0; k < _path[j].m_radius.Count; k++)
    //                {
    //                    if (_path[j].m_radius[k].m_holding && _path[j].m_radius[k].m_holding == _target.gameObject)
    //                    {
    //                        found = true;
    //                        newTile = _path[j];
    //                        break;
    //                    }
    //                }
    //                _path[j].ClearRadius();
    //                if (found)
    //                    break;
    //            }
    //            if (_path.Count == 0)
    //            {
    //                ActionTargeting(m_tile);

    //                for (int k = 0; k < m_tile.m_radius.Count; k++)
    //                {
    //                    if (m_tile.m_radius[k].m_holding && m_tile.m_radius[k].m_holding == _target.gameObject)
    //                    {
    //                        found = true;
    //                        break;
    //                    }
    //                }
    //                m_tile.ClearRadius();
    //            }
    //        }

    //        if (found && m_player.CheckEnergy(eng))
    //        {
    //            if (CheckIfAttack(name) && _target.m_player == m_player || !CheckIfAttack(name) && _target != m_player)
    //                continue;

    //            AIActionScript newAct = gameObject.AddComponent<AIActionScript>();
    //            if (newTile)
    //                newAct.m_position = newTile;
    //            else
    //                newAct.m_position = null;

    //            newAct.m_targets = new List<GameObject>();
    //            newAct.CalculateValue(m_actions[i], this, _target);
    //            viableActs.Add(newAct);
    //        }
    //    }

    //    AIActionScript mostViable = null;

    //    if (viableActs.Count > 0)
    //    {
    //        for (int i = 0; i < viableActs.Count; i++)
    //            if (i == 0 || viableActs[i].m_value > mostViable.m_value)
    //                mostViable = viableActs[i];

    //        for (int i = 0; i < viableActs.Count; i++)
    //        {
    //            if (viableActs[i] != mostViable)
    //                Destroy(viableActs[i]);
    //        }
    //    }
    //    else if (_target.m_player != m_player && _path.Count > 1)
    //    {
    //        mostViable = gameObject.AddComponent<AIActionScript>();
    //        if (_path.Count > m_tempStats[(int)sts.MOV] - 1)
    //            mostViable.m_position = _path[m_tempStats[(int)sts.MOV] - 1];
    //        else
    //            mostViable.m_position = _path[_path.Count - 1];

    //        mostViable.m_value = -TileScript.CaclulateDistance(mostViable.m_position, _target.m_tile);
    //    }
    //    else
    //        return null;

    //    return mostViable;
    //}
}
