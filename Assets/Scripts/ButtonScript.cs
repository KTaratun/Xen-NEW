using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ButtonScript : MonoBehaviour {

    public GameObject actionViewer;
    public GameObject actionPanel;
    public GameObject character;
    public GameObject[] energyPanel;
    public string action;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void HoverTrue(BaseEventData eventData)
    {
        if (GetComponent<Button>().name == "Turn Panel Energy Button")
        {
            CharacterScript charScript = character.GetComponent<CharacterScript>();

            //Image turnPanImage = charScript.turnPanel.GetComponent<Image>();
            //turnPanImage.color = Color.cyan;
            Renderer charRenderer = character.GetComponent<Renderer>();
            charRenderer.material.color = Color.cyan;
            PanelScript hudPanScript = charScript.boardScript.panels[(int)BoardScript.pnls.HUD_RIGHT_PANEL].GetComponent<PanelScript>();
            hudPanScript.text[0].text = charScript.name;
            hudPanScript.text[1].text = "HP: " + charScript.tempStats[(int)CharacterScript.sts.HP] + "/" + charScript.stats[(int)CharacterScript.sts.HP];
            hudPanScript.inView = true;

            return;
        }

        if (!GetComponent<Button>().interactable || !actionViewer)
            return;

        PanelScript actViewScript = actionViewer.GetComponent<PanelScript>();
        actViewScript.inView = true;

        PanelScript actPanScript = actionPanel.GetComponent<PanelScript>();
        actViewScript.character = actPanScript.character;
        actViewScript.cScript = actPanScript.cScript;
        if (actPanScript.inView)
            actViewScript.cScript.currAction = name;
        actViewScript.PopulateText();
    }

    public void HoverFalse()
    {
        if (GetComponent<Button>().name == "Turn Panel Energy Button")
        {
            CharacterScript charScript = character.GetComponent<CharacterScript>();

            //Image turnPanImage = charScript.turnPanel.GetComponent<Image>();
            //turnPanImage.color = Color.cyan;
            Renderer charRenderer = character.GetComponent<Renderer>();
            charRenderer.material.color = Color.white;
            PanelScript hudPanScript = charScript.boardScript.panels[(int)BoardScript.pnls.HUD_RIGHT_PANEL].GetComponent<PanelScript>();
            hudPanScript.inView = false;
            
            return;
        }

        if (!actionViewer)
            return;

        PanelScript actPanel = actionViewer.GetComponent<PanelScript>();
        actPanel.inView = false;
    }

    public void SetTotalEnergy(string energy)
    {
        // Initialize panel with anything
        GameObject panel = energyPanel[0];

        // Check to see how many energy symbols we are going to need
        if (energy.Length == 1)
        {
            energyPanel[0].SetActive(true);
            energyPanel[1].SetActive(false);
            energyPanel[2].SetActive(false);
            panel = energyPanel[0];
        }
        else if (energy.Length == 2)
        {
            energyPanel[1].SetActive(true);
            energyPanel[0].SetActive(false);
            energyPanel[2].SetActive(false);
            panel = energyPanel[1];
        }
        else if (energy.Length == 3)
        {
            energyPanel[2].SetActive(true);
            energyPanel[0].SetActive(false);
            energyPanel[1].SetActive(false);
            panel = energyPanel[2];
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
}
