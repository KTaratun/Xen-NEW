using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerScript : MonoBehaviour {

    public enum eng { GRN, RED, WHT, BLU, TOT }

    public List<GameObject> m_characters;
    public GameObject m_energyPanel;
    public int m_num;
    public int[] m_energy;

	// Use this for initialization
	void Start ()
    {
        m_energy = new int[4];
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void SetEnergyPanel()
    {
        Text[] text = m_energyPanel.GetComponentsInChildren<Text>();

        for (int i = 0; i < text.Length; i++)
            text[i].text = m_energy[i].ToString();
    }

    static public bool CheckIfGains(string _energy)
    {
        if (_energy[0] == 'G' || _energy[0] == 'R' || _energy[0] == 'W' || _energy[0] == 'B')
            return false;

        return true;
    }

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

    static public string CheckCharColors(string[] _actions)
    {
        string colors = "";

        for (int i = 0; i < _actions.Length; i++)
        {
            string actEng = DatabaseScript.GetActionData(_actions[i], DatabaseScript.actions.ENERGY);
            if (CheckIfGains(actEng))
                continue;

            for (int j = 0; j < actEng.Length; j++)
            {
                bool newColor = false;  
                for (int k = 0; k < colors.Length; k++)
                {
                    if (actEng[j] == colors[k])
                        break;

                    if (k == colors.Length - 1)
                        newColor = true;
                }

                if (newColor || colors.Length == 0)
                {
                    if (actEng[j] == 'g' || actEng[j] == 'G')
                        colors += 'G';
                    else if (actEng[j] == 'r' || actEng[j] == 'R')
                        colors += 'R';
                    else if (actEng[j] == 'w' || actEng[j] == 'W')
                        colors += 'W';
                    else if (actEng[j] == 'b' || actEng[j] == 'B')
                        colors += 'B';
                }
            }
        }

        return colors;
    }

    public void RemoveRandomEnergy()
    {
        List<int> energyOverZero = new List<int>();
        for (int i = 0; i < m_energy.Length; i++)
            if (m_energy[i] > 0)
                energyOverZero.Add(i);

        if (energyOverZero.Count > 0)
        {
            int randomEnergy = Random.Range(0, energyOverZero.Count);
            m_energy[energyOverZero[randomEnergy]]--;
        }
    }
}
