using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ButtonScript : MonoBehaviour {

    public GameObject m_actionViewer;
    public GameObject m_actionPanel;
    public GameObject m_character;
    public GameObject m_camera;
    public GameObject[] m_energyPanel;
    public string m_action;

	// Use this for initialization
	void Start ()
    {
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void HoverTrue(BaseEventData eventData)
    {
        if (GetComponent<Button>().name == "Turn Panel Energy Button")
        {
            CharacterScript charScript = m_character.GetComponent<CharacterScript>();

            //Image turnPanImage = charScript.turnPanel.GetComponent<Image>();
            //turnPanImage.color = Color.cyan;
            Renderer charRenderer = m_character.GetComponent<Renderer>();
            charRenderer.material.color = Color.cyan;
            PanelScript hudPanScript = charScript.m_boardScript.m_panels[(int)BoardScript.pnls.HUD_RIGHT_PANEL].GetComponent<PanelScript>();
            hudPanScript.m_text[0].text = charScript.name;
            hudPanScript.m_text[1].text = "HP: " + charScript.m_tempStats[(int)CharacterScript.sts.HP] + "/" + charScript.m_stats[(int)CharacterScript.sts.HP];
            hudPanScript.m_inView = true;

            return;
        }

        Text text = GetComponent<Button>().GetComponentInChildren<Text>();

        if (text.text == "EMPTY" || !m_actionViewer)
            return;

        PanelScript actViewScript = m_actionViewer.GetComponent<PanelScript>();
        actViewScript.m_inView = true;

        PanelScript actPanScript = m_actionPanel.GetComponent<PanelScript>();
        actViewScript.m_character = actPanScript.m_character;
        actViewScript.m_cScript = actPanScript.m_cScript;
        if (actPanScript.m_inView)
            actViewScript.m_cScript.m_currAction = name;
        actViewScript.PopulateText();
    }

    public void HoverFalse()
    {
        if (GetComponent<Button>().name == "Turn Panel Energy Button")
        {
            CharacterScript charScript = m_character.GetComponent<CharacterScript>();

            //Image turnPanImage = charScript.turnPanel.GetComponent<Image>();
            //turnPanImage.color = Color.cyan;
            Renderer charRenderer = m_character.GetComponent<Renderer>();
            charRenderer.material.color = Color.white;
            PanelScript hudPanScript = charScript.m_boardScript.m_panels[(int)BoardScript.pnls.HUD_RIGHT_PANEL].GetComponent<PanelScript>();
            hudPanScript.m_inView = false;
            
            return;
        }

        if (!m_actionViewer)
            return;

        PanelScript actPanel = m_actionViewer.GetComponent<PanelScript>();
        actPanel.m_inView = false;
    }

    public void SetTotalEnergy(string energy)
    {
        // Initialize panel with anything
        GameObject panel = m_energyPanel[0];

        // Check to see how many energy symbols we are going to need
        if (energy.Length == 1)
        {
            m_energyPanel[0].SetActive(true);
            m_energyPanel[1].SetActive(false);
            m_energyPanel[2].SetActive(false);
            panel = m_energyPanel[0];
        }
        else if (energy.Length == 2)
        {
            m_energyPanel[1].SetActive(true);
            m_energyPanel[0].SetActive(false);
            m_energyPanel[2].SetActive(false);
            panel = m_energyPanel[1];
        }
        else if (energy.Length == 3)
        {
            m_energyPanel[2].SetActive(true);
            m_energyPanel[0].SetActive(false);
            m_energyPanel[1].SetActive(false);
            panel = m_energyPanel[2];
        }

        // Gather engergy symbols into an array
        Image[] orbs = panel.GetComponentsInChildren<Image>();

        // Assign energy symbols
        for (int i = 0; i < energy.Length; i++)
        {
            if (energy[i] == 'g')
                orbs[i+1].color = new Color(.55f, .8f, .5f, 1);
            else if (energy[i] == 'r')
                orbs[i+1].color = new Color(1, .3f, .35f, 1);
            else if (energy[i] == 'w')
                orbs[i+1].color = new Color(1, 1, 1, 1);
            else if (energy[i] == 'b')
                orbs[i+1].color = new Color(.55f, .5f, 1, 1);
            else if (energy[i] == 'G')
                orbs[i+1].color = new Color(.35f, .6f, .3f, 1);
            else if (energy[i] == 'R')
                orbs[i+1].color = new Color(.8f, .1f, .15f, 1);
            else if (energy[i] == 'W')
                orbs[i+1].color = new Color(.8f, .8f, .8f, 1);
            else if (energy[i] == 'B')
                orbs[i+1].color = new Color(.35f, .3f, 1, 1);
        }
    }

    //public void SetUniqueEnergy(string energy)
    //{
    //    GameObject panel = energyPanel[0];

    //    int numEle = 0;
    //    List<char> used = new List<char>();

    //    for (int i = 0; i < energy.Length; i++)
    //    {
    //        if (used.Contains(energy[i]))
    //            continue;

    //        used.Add(energy[i]);
    //        numEle++;
    //    }

    //    if (numEle== 1)
    //    {
    //        energyPanel[0].SetActive(true);
    //        panel = energyPanel[0];
    //    }
    //    else if (numEle == 2)
    //    {
    //        energyPanel[1].SetActive(true);
    //        panel = energyPanel[1];
    //    }
    //    else if (numEle == 3)
    //    {
    //        energyPanel[2].SetActive(true);
    //        panel = energyPanel[2];
    //    }

    //    Image[] orbs = panel.GetComponentsInChildren<Image>();

    //    for (int i = 0; i < num; i++)
    //    {
    //        if (energy[i] == 'g')
    //            orbs[i + 1].color = new Color(.5f, 1, .5f, 1);
    //        else if (energy[i] == 'r')
    //            orbs[i + 1].color = new Color(1, .5f, .5f, 1);
    //        else if (energy[i] == 'w')
    //            orbs[i + 1].color = new Color(1, 1, 1, 1);
    //        else if (energy[i] == 'b')
    //            orbs[i + 1].color = new Color(.5f, .5f, 1, 1);
    //        else if (energy[i] == 'G')
    //            orbs[i + 1].color = new Color(.1f, .9f, .1f, 1);
    //        else if (energy[i] == 'R')
    //            orbs[i + 1].color = new Color(.9f, .1f, .1f, 1);
    //        else if (energy[i] == 'W')
    //            orbs[i + 1].color = new Color(.9f, .9f, .9f, 1);
    //        else if (energy[i] == 'B')
    //            orbs[i + 1].color = new Color(.1f, .1f, .9f, 1);
    //    }
    //}

    public void SetCameraTarget()
    {
        CameraScript camScript = m_camera.GetComponent<CameraScript>();
        camScript.m_target = m_character;
    }
}
