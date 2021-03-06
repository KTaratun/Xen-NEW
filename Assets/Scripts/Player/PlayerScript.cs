﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerScript : MonoBehaviour {

    public enum eng { GRN, RED, WHT, BLU, TOT }

    public List<CharacterScript> m_characters;
    public int m_num;
    public int[] m_energy;
    public BoardScript m_bScript;

    private SlidingPanelManagerScript m_panMan;
    private GameManagerScript m_gamMan;

    // Use this for initialization
    void Start ()
    {
        //m_energy = new int[4];

        if (GameObject.Find("Scene Manager"))
        {
            m_panMan = GameObject.Find("Scene Manager").GetComponent<SlidingPanelManagerScript>();
            m_gamMan = GameObject.Find("Scene Manager").GetComponent<GameManagerScript>();
        }
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    public void SetEnergyPanel(CharacterScript _caller)
    {
        Text[] text = null;
        if (_caller == m_gamMan.m_currCharScript)
            text = m_panMan.GetPanel("HUD Panel LEFT").transform.Find("Energy Panel").GetComponentsInChildren<Text>();
        else
            text = m_panMan.GetPanel("HUD Panel RIGHT").transform.Find("Energy Panel").GetComponentsInChildren<Text>();

        for (int i = 0; i < text.Length; i++)
            text[i].text = m_energy[i].ToString();
    }

    static public bool CheckIfGains(string _energy)
    {
        if (_energy[0] == 'G' || _energy[0] == 'R' || _energy[0] == 'W' || _energy[0] == 'B')
            return false;

        return true;
    }

    // Check to see if action is usable
    public bool CheckEnergy(string _eng)
    {
        if (CheckIfGains(_eng))
            return true;

        int[] engCheck =  new int[4];

        for (int i = 0; i < _eng.Length; i++)
        {
            if (_eng[i] == 'G')
                engCheck[0]++;
            else if (_eng[i] == 'R')
                engCheck[1]++;
            else if (_eng[i] == 'W')
                engCheck[2]++;
            else if (_eng[i] == 'B')
                engCheck[3]++;
        }

        for (int i = 0; i < m_energy.Length; i++)
            if (engCheck[i] > m_energy[i])
                return false;

        return true;
    }

    static public string CheckCharColors(ActionScript[] _actions)
    {
        string colors = "";

        for (int i = 0; i < _actions.Length; i++)
        {
            string actEng = _actions[i].m_energy;

            for (int j = 0; j < actEng.Length; j++)
            {
                char engToCheck = ' ';

                if (actEng[j] == 'g')
                    engToCheck = 'G';
                else if (actEng[j] == 'r')
                    engToCheck = 'R';
                else if (actEng[j] == 'w')
                    engToCheck = 'W';
                else if (actEng[j] == 'b')
                    engToCheck = 'B';
                else
                    engToCheck = actEng[j];

                bool newColor = false;  
                for (int k = 0; k < colors.Length; k++)
                {
                    if (engToCheck == colors[k])
                        break;

                    if (k == colors.Length - 1)
                        newColor = true;
                }

                if (newColor || colors.Length == 0)
                    colors += engToCheck;
            }
        }

        return colors;
    }

    public void AddRandomEnergy(int _num)
    {
        List<int> viableEnergy = new List<int>();

        for (int i = 0; i < m_characters.Count; i++)
        {
            if (!m_characters[i].m_isAlive)
                continue;

            string col = m_characters[i].m_color;

            for (int j = 0; j < col.Length; j++)
            {
                if (col[j] == 'G')
                    viableEnergy.Add((int)eng.GRN);
                else if (col[j] == 'R')
                    viableEnergy.Add((int)eng.RED);
                else if (col[j] == 'W')
                    viableEnergy.Add((int)eng.WHT);
                else if (col[j] == 'B')
                    viableEnergy.Add((int)eng.BLU);
            }
        }

        for (int i = 0; i < _num; i++)
            m_energy[viableEnergy[Random.Range(0, viableEnergy.Count)]]++;
    }

    public void RemoveRandomEnergy(int _num)
    {
        List<int> energyOverZero = new List<int>();
        for (int h = 0; h < _num; h++)
        {
            if (TotalEnergy() < 1)
                return;

            for (int i = 0; i < m_energy.Length; i++)
                if (m_energy[i] > 0)
                    energyOverZero.Add(i);

            if (energyOverZero.Count > 0)
            {
                int randomEnergy = Random.Range(0, energyOverZero.Count);
                m_energy[energyOverZero[randomEnergy]]--;
            }
            energyOverZero.Clear();
        }
    }

    public void GainRandomEnergyAI(int _num)
    {
        // No downside for having a lot of tech. Gives all eng to one color
        int engInd = CheckTeamColors();
        for (int h = 0; h < _num; h++)
            m_energy[engInd] += _num;
    }

    public int CheckTeamColors()
    {
        int[] colors = new int[4];
        for (int i = 0; i < m_characters.Count; i++)
        {
            if (m_characters[i].m_isAlive)
            {
                for (int j = 0; j < m_characters[i].m_color.Length; j++)
                {
                    if (m_characters[i].m_color[j] == 'G')
                        colors[0]++;
                    else if (m_characters[i].m_color[j] == 'R')
                        colors[1]++;
                    else if (m_characters[i].m_color[j] == 'W')
                        colors[2]++;
                    else if (m_characters[i].m_color[j] == 'B')
                        colors[3]++;
                }
            }
        }

        int mostValuableColorInd = 0;
        int currMost = colors[0];

        for (int i = 1; i < 4; i++)
        {
            if (colors[i] > currMost)
            {
                mostValuableColorInd = i;
                currMost = colors[i];
            }
        }

        return mostValuableColorInd;
    }

    public int TotalEnergy()
    {
        int total = 0;
        for (int i = 0; i < m_energy.Length; i++)
            total += m_energy[i];

        return total;
    }
}
