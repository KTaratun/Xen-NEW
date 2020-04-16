using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ActionPanelScript : PanelScript 
{
    private BoardScript m_board;

    // Use this for initialization
    new void Start () {
        base.Start();

        if (GameObject.Find("Board").GetComponent<BoardScript>())
            m_board = GameObject.Find("Board").GetComponent<BoardScript>();
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    override public void PopulatePanel()
    {
        ResetButtons();

        //if (GameObject.Find("Board"))
        //    ActionsAvailable();
        //else
            for (int i = 0; i < m_cScript.m_actions.Count; i++)
                PopulateButton(i);
    }

    public void MakeHidden(Button _button)
    {
        _button.transform.Find("Text").GetComponent<Text>().text = "???";
        ActionButtonScript buttScript = _button.GetComponent<ActionButtonScript>();
        buttScript.SetTotalEnergy("");
    }

    public void ActionsAvailable()
    {
        int activeCount = 0;
        for (int i = 0; i < m_cScript.m_actions.Count; i++)
        {
            ActionScript act = m_cScript.m_actions[i];

            Button currButton = transform.GetChild(i).GetComponent<Button>();
            ActionButtonScript buttScript = currButton.GetComponent<ActionButtonScript>();

            buttScript.m_action = m_cScript.m_actions[i];
            buttScript.SetTotalEnergy(act.m_energy);
            buttScript.m_object = m_cScript.gameObject;
            currButton.GetComponentInChildren<Text>().text = act.m_name;

            CharacterScript currCharScript = m_board.m_currCharScript;
            string currCharActName = "";
            if (currCharScript.m_currAction)
                currCharActName = currCharScript.m_currAction.m_name;

            // ACTION PREVENTION
            // REFACTOR
            if (act.m_isDisabled > 0)
                currButton.GetComponent<Image>().color = b_isDisallowed;
            else
            {
                currButton.GetComponent<Image>().color = new Color(1, 1, 1, 1);
                if (m_cScript.m_player.CheckEnergy(act.m_energy) && m_cScript.m_hasActed[(int)CharacterScript.trn.ACT] == false && 
                    !m_panMan.GetPanel("Choose Panel").m_inView)
                    currButton.interactable = true;
                else
                    currButton.interactable = false;
            }

            activeCount++;
        }

        for (int i = activeCount; i < transform.childCount; i++)
            transform.GetChild(i).GetComponent<Button>().interactable = false;
    }

    private void PopulateButton(int _ind)
    {
        ActionScript act = m_cScript.m_actions[_ind];
        Button currButton = transform.GetChild(_ind).GetComponent<Button>();

        if (!act.m_isRevealed && m_cScript.m_player != m_board.m_currCharScript.m_player)
        {
            MakeHidden(currButton);
            return;
        }

        currButton.name = act.m_name;
        ActionButtonScript buttScript = currButton.GetComponent<ActionButtonScript>();
        buttScript.m_action = act;

        Text t = currButton.GetComponentInChildren<Text>();

        t.text = act.m_name;
        buttScript.SetTotalEnergy(act.m_energy);


        if (m_board)
        {
            CharacterScript currCharScript = m_board.m_currCharScript;
            string currCharActName = "";
            if (currCharScript.m_currAction)
                currCharActName = currCharScript.m_currAction.m_name;

            // ACTION PREVENTION
            if (m_cScript.m_hasActed[(int)CharacterScript.trn.ACT] == true || act.m_isDisabled > 0)
                currButton.GetComponent<Image>().color = b_isDisallowed;
            else
            {
                currButton.GetComponent<Image>().color = new Color(1, 1, 1, 1);
                if (m_cScript.m_player.CheckEnergy(act.m_energy))
                {
                    if (m_cScript)
                        currButton.onClick.AddListener(() => act.ActionTargeting(currCharScript.m_tile));
                    currButton.interactable = true;
                }
            }
        }
        else
            currButton.interactable = true;
    }

    public void ResetButtons()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            Button currButton = transform.GetChild(i).GetComponent<Button>();

            currButton.onClick.RemoveAllListeners();
            currButton.interactable = false;
            currButton.GetComponent<Image>().color = new Color(1, 1, 1, 1);
            if (currButton.GetComponent<SlidingPanelScript>())
                currButton.GetComponent<SlidingPanelScript>().m_inView = false;
            Text t = currButton.GetComponentInChildren<Text>();
            t.text = "EMPTY";
            EnergyButtonScript buttScript = currButton.GetComponent<EnergyButtonScript>();

            for (int k = 0; k < buttScript.m_energyPanel.Length; k++)
                buttScript.m_energyPanel[k].SetActive(false);
        }
    }
}
