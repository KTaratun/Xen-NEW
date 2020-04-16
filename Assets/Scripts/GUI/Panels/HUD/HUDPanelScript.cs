using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUDPanelScript : SlidingPanelScript
{
    // Start is called before the first frame update
    new protected void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    new void Update()
    {
        base.Update();
    }

    override public void PopulatePanel()
    {
        base.PopulatePanel();
        //m_panels[0].GetComponent<Image>().color = new Color(m_cScript.m_teamColor.r + 0.3f, m_cScript.m_teamColor.g + 0.3f, m_cScript.m_teamColor.b + 0.3f, 1);

        transform.Find("Character/Name").GetComponent<Text>().text = m_cScript.m_name;
        transform.Find("Parameters/HP").GetComponent<Text>().text = "HP: " + m_cScript.m_currHealth + "/" + m_cScript.m_totalHealth;

        Image[] statSyms = transform.Find("Parameters/Images").GetComponentsInChildren<Image>();
        Transform vals = transform.Find("Parameters/Values");

        for (int i = 0; i < (int)CharacterScript.sts.TOT -1; i++)
        {
            //if (m_cScript.m_tempStats[i].ToString().Length > 1 && m_cScript.m_tempStats[i] != -1)
            vals.gameObject.GetComponentsInChildren<Text>()[i].text = m_cScript.m_tempStats[i].ToString();
            //else
            //    GetComponentsInChildren<Text>()[i + 1].text = spacing + " " + m_cScript.m_tempStats[i];

            if (m_cScript.m_tempStats[i] == m_cScript.m_stats[i])
                statSyms[i].color = Color.white;
            else if (m_cScript.m_tempStats[i] > m_cScript.m_stats[i])
                statSyms[i].color = StatusScript.c_buffColor;
            else if (m_cScript.m_tempStats[i] < m_cScript.m_stats[i])
                statSyms[i].color = StatusScript.c_debuffColor;
        }

        if (GetComponentInChildren<Button>())
        {
            Button button = GetComponentInChildren<Button>();
            EnergyButtonScript buttScript = button.GetComponent<EnergyButtonScript>();
            buttScript.SetTotalEnergy(m_cScript.m_color);

            PanelScript statusButtonsPan = transform.Find("Status/Status Buttons").GetComponent<PanelScript>();
            statusButtonsPan.m_cScript = m_cScript;
            HUDStatusScript.StatusSymbolSetup(statusButtonsPan.transform);
        }

        PanelScript actButtonPan = transform.Find("Action Panel").GetComponent<PanelScript>();
        actButtonPan.m_cScript = m_cScript;
        actButtonPan.PopulatePanel();

        m_cScript.m_player.SetEnergyPanel(m_cScript);

        if (name == "HUD Panel LEFT")
        {
            PanelScript movePassPan = transform.Find("Move Pass Panel").GetComponent<PanelScript>();
            movePassPan.transform.Find("Move").GetComponent<ButtonScript>().m_object = m_cScript.gameObject;
            if (m_cScript.m_effects[(int)StatusScript.effects.IMMOBILE])
            {
                movePassPan.transform.Find("Move").GetComponent<Image>().color = b_isDisallowed;
                movePassPan.transform.Find("Move").GetComponent<Button>().interactable = false;
            }
            else
                movePassPan.transform.Find("Move").GetComponent<Image>().color = Color.white;
        }
    }
}
