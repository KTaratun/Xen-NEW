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

    public bool CheckEnergy(string eng)
    {
        if (CheckIfGains(eng))
            return true;

        int[] engCheck =  new int[4];

        for (int i = 0; i < eng.Length; i++)
        {
            if (eng[i] == 'G')
                engCheck[0]++;
            else if (eng[i] == 'R')
                engCheck[1]++;
            else if (eng[i] == 'W')
                engCheck[2]++;
            else if (eng[i] == 'B')
                engCheck[3]++;
        }

        for (int i = 0; i < m_energy.Length; i++)
            if (engCheck[i] > m_energy[i])
                return false;

        return true;
    }

    public void RemoveRandomEnergy()
    {
        List<int> energyOverZero = new List<int>();
        for (int i = 0; i < m_energy.Length; i++)
            if (m_energy[i] > 0)
                energyOverZero.Add(m_energy[i]);

        if (energyOverZero.Count > 0)
        {
            int randomEnergy = Random.Range(0, energyOverZero.Count);
            m_energy[randomEnergy]--;
        }
    }
}
