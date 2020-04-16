using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnergyButtonScript : ButtonScript {

    public GameObject[] m_energyPanel;

    // Use this for initialization
    new void Start ()
    {
        if (m_energyPanel.Length == 0)
            EnergyInit();
	}

    public void EnergyInit()
    {
        m_energyPanel = new GameObject[3];
        int j = 0;

        RectTransform[] children = transform.GetComponentsInChildren<RectTransform>();

        for (int i = 0; i < children.Length; i++)
        {
            if (children[i].tag == "Energy")
            {
                m_energyPanel[j] = children[i].gameObject;
                m_energyPanel[j].SetActive(false);
                j++;
            }
        }
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    public void SelectorButton()
    {
        ActionScript act = m_boardScript.m_currCharScript.m_currAction;
        CharacterScript charScript = m_boardScript.m_currCharScript;

        if (m_parent.name == "Status Selector")
        {
            StatusScript statScript = m_parent.m_cScript.GetComponents<StatusScript>()[m_parent.m_cScript.m_currStatus];
            if (act.m_name == "SUP(Defrag)")
                statScript.DestroyStatus(m_parent.m_cScript.transform.root.gameObject, true);
            else if (act.m_name == "SUP(Extension)")
            {
                statScript.m_lifeSpan += 3 + charScript.m_tempStats[(int)CharacterScript.sts.TEC];

                //for (int i = 0; i < statScript.m_statMod.Length; i++)
                //{
                //    if (statScript.m_statMod[i] > 0)
                //        statScript.m_statMod[i]++;
                //    else if (statScript.m_statMod[i] < 0)
                //        statScript.m_statMod[i]--;
                //}
            }
            //else if (actName == "Modification")
            //    for (int i = 0; i < statScript.m_statMod.Length; i++)
            //        m_main.m_cScript.m_stats[i] += statScript.m_statMod[i];

            InputManagerScript.ResumeGame();
        }
        else if (m_parent.name == "Energy Selector")
        {

            if (act.m_name == "ATK(Prismatic)")
                AddEnergy(2 + charScript.m_tempStats[(int)CharacterScript.sts.TEC]);
            else if (act.m_name == "ATK(Deplete)" || act.m_name == "ATK(Syphon)")
                SubtractEnergy(2 + charScript.m_tempStats[(int)CharacterScript.sts.TEC], m_main.m_cScript.m_player.m_energy);
        }
    }

    public void SetTotalEnergy(string energy)
    {
        if (m_energyPanel.Length == 0)
            EnergyInit();

        // Initialize panel with anything
        GameObject panel = m_energyPanel[0];

        // Check to see how many energy symbols we are going to need
        if (energy.Length == 0)
        {
            m_energyPanel[0].SetActive(false);
            m_energyPanel[1].SetActive(false);
            m_energyPanel[2].SetActive(false);

            return;
        }
        else if (energy.Length == 1)
        {
            m_energyPanel[0].SetActive(true);
            m_energyPanel[1].SetActive(false);
            m_energyPanel[2].SetActive(false);
            // m_energyPanel[3].SetActive(false);
            panel = m_energyPanel[0];
        }
        else if (energy.Length == 2)
        {
            m_energyPanel[1].SetActive(true);
            m_energyPanel[0].SetActive(false);
            m_energyPanel[2].SetActive(false);
            // m_energyPanel[3].SetActive(false);
            panel = m_energyPanel[1];
        }
        else if (energy.Length == 3)
        {
            m_energyPanel[2].SetActive(true);
            m_energyPanel[0].SetActive(false);
            m_energyPanel[1].SetActive(false);
            //m_energyPanel[3].SetActive(false);
            panel = m_energyPanel[2];
        }
        //else if (energy.Length == 4)
        //{
        //    m_energyPanel[3].SetActive(true);
        //    m_energyPanel[0].SetActive(false);
        //    m_energyPanel[1].SetActive(false);
        //    m_energyPanel[2].SetActive(false);
        //    panel = m_energyPanel[2];
        //}
        // Gather engergy symbols into an array
        Image[] orbs = panel.GetComponentsInChildren<Image>();

        // Assign energy symbols
        for (int i = 0; i < energy.Length; i++)
        {
            if (energy[i] == 'g')
                orbs[i].color = new Color(.7f, 1f, .65f, 1);
            else if (energy[i] == 'r')
                orbs[i].color = new Color(1, .45f, .5f, 1);
            else if (energy[i] == 'w')
                orbs[i].color = new Color(.85f, .85f, .85f, 1); // (1, 1, 1, 1)
            else if (energy[i] == 'b')
                orbs[i].color = new Color(.7f, .65f, 1, 1);
            else if (energy[i] == 'G')
                orbs[i].color = new Color(.4f, .65f, .35f, 1);
            else if (energy[i] == 'R')
                orbs[i].color = new Color(.85f, .15f, .2f, 1);
            else if (energy[i] == 'W')
                orbs[i].color = new Color(1, 1, 1, 1); // (.8f, .8f, .8f, 1)
            else if (energy[i] == 'B')
                orbs[i].color = new Color(.4f, .35f, 1, 1);
        }
    }

    public void AddEnergy(int _max)
    {
        int total = 0;
        for (int i = 0; i < transform.parent.childCount; i++)
            total += int.Parse(transform.parent.GetChild(i).GetComponentInChildren<Text>().text);

        if (total < _max)
        {
            Text text = transform.parent.GetChild(int.Parse(gameObject.name)).GetComponentInChildren<Text>();
            text.text = (int.Parse(text.text) + 1).ToString();
        }
    }

    public void SubtractEnergy(int _min, int[] _playerEnergy)
    {
        Debug.Log("Don't Know");

        int currTotal = 0;
        int origTotal = 0;

        for (int i = 0; i < transform.parent.childCount; i++)
        {
            currTotal += int.Parse(transform.parent.GetChild(i).GetComponentInChildren<Text>().text);
            origTotal += _playerEnergy[i];
        }

        if (_min > origTotal - currTotal && _playerEnergy[int.Parse(gameObject.name)] > 0)
        {
            Text text = transform.parent.GetChild(int.Parse(gameObject.name)).GetComponentInChildren<Text>();
            text.text = (int.Parse(text.text) - 1).ToString();
        }
    }

    public void ConfirmEnergySelection()
    {
        ActionScript act = m_boardScript.m_currCharScript.m_currAction;
        CharacterScript charScript = m_boardScript.m_currCharScript;
        PlayerScript playScript = charScript.m_player;
        int added = 0;

        if (act.m_name == "ATK(Syphon)" || act.m_name == "ATK(Deplete)")
        {
            for (int i = 0; i < transform.parent.childCount; i++)
            {
                if (act.m_name == "ATK(Syphon)")
                    playScript.m_energy[i] += m_main.m_cScript.m_player.m_energy[i] - int.Parse(transform.parent.GetChild(i).GetComponentInChildren<Text>().text);

                added += m_main.m_cScript.m_player.m_energy[i] - int.Parse(transform.parent.GetChild(i).GetComponentInChildren<Text>().text);
                m_main.m_cScript.m_player.m_energy[i] = int.Parse(transform.parent.GetChild(i).GetComponentInChildren<Text>().text);
            }
        }
        else
            for (int i = 0; i < transform.parent.childCount; i++)
            {
                added += int.Parse(transform.parent.GetChild(i).GetComponentInChildren<Text>().text);
                playScript.m_energy[i] += int.Parse(transform.parent.GetChild(i).GetComponentInChildren<Text>().text);
            }

        if (added > 2)
            if (act.m_name == "ATK(Prismatic)" || act.m_name == "ATK(Syphon)" || act.m_name == "ATK(Deplete)")
                charScript.ReceiveDamage((added - 2).ToString(), Color.white);

        playScript.SetEnergyPanel(charScript);
        InputManagerScript.
ResumeGame();
    }

    public void ResetEnergySelection()
    {
        ActionScript act = m_boardScript.m_currCharScript.m_currAction;
        if (act.m_name == "ATK(Syphon)" || act.m_name == "ATK(Deplete)")
            for (int i = 0; i < transform.parent.childCount; i++)
                transform.parent.GetChild(i).GetComponentInChildren<Text>().text = m_main.m_cScript.m_player.m_energy[i].ToString();
        else
            for (int i = 0; i < transform.parent.childCount; i++)
                transform.parent.GetChild(i).GetComponentInChildren<Text>().text = "0";
    }
}
