using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ButtonScript : MonoBehaviour {

    // Get rid of board
    public BoardScript m_boardScript;
    public PanelScript m_main;
    public GameObject m_character;
    public PanelScript m_parent;
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
        // REFACTOR: Make this more like how status images are handled
        if (GetComponent<Button>() && GetComponent<Button>().name == "Turn Panel Energy Button")
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
        else if (gameObject.tag == "Status Image")
        {
            m_main.m_inView = true;

            m_main.m_cScript = m_parent.m_cScript;
            m_main.m_cScript.m_currStatus = int.Parse(name);
            m_main.PopulatePanel();
            return;
        }

        Text text = GetComponent<Button>().GetComponentInChildren<Text>();

        // REFACTOR: That's so wack though
        if (text.text == "EMPTY" || !m_main || !m_parent || GetComponent<Image>().color == new Color(1, .5f, .5f, 1) ||
            m_parent && m_parent.m_main && m_parent.m_main.GetComponent<PanelScript>() && !m_parent.m_main.GetComponent<PanelScript>().m_inView)
            return;

        m_main.m_cScript = m_parent.m_cScript.GetComponent<CharacterScript>();
        if (m_parent.m_inView) // Need this check to avoid selecting another action while menu is moving
            m_main.m_cScript.m_currAction = name;
        if (m_parent.m_inView && m_boardScript) // Need this check to avoid selecting another action while menu is moving
        {
            CharacterScript charScript = m_boardScript.m_currPlayer.GetComponent<CharacterScript>();
            if (GetComponent<Button>().GetComponent<Image>().color == PanelScript.b_isFree)
                charScript.m_isFree = GetComponent<Button>();
            charScript.m_currAction = name;
        }

        m_main.PopulatePanel();
    }

    public void HoverFalse()
    {
        if (GetComponent<Button>() && GetComponent<Button>().name == "Turn Panel Energy Button")
        {
            CharacterScript charScript = m_character.GetComponent<CharacterScript>();

            //Image turnPanImage = charScript.turnPanel.GetComponent<Image>();
            //turnPanImage.color = Color.cyan;
            Renderer charRenderer = m_character.GetComponent<Renderer>();
            charRenderer.material.color = charScript.m_teamColor;
            PanelScript hudPanScript = charScript.m_boardScript.m_panels[(int)BoardScript.pnls.HUD_RIGHT_PANEL].GetComponent<PanelScript>();
            hudPanScript.m_inView = false;
            
            return;
        }
        else if (gameObject.tag == "Status Image")
            m_main.m_inView = false;

        if (!m_main)
            return;

        m_main.m_inView = false;
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
                orbs[i+1].color = new Color(.65f, .9f, .6f, 1);
            else if (energy[i] == 'r')
                orbs[i+1].color = new Color(1, .4f, .45f, 1);
            else if (energy[i] == 'w')
                orbs[i+1].color = new Color(1, 1, 1, 1);
            else if (energy[i] == 'b')
                orbs[i+1].color = new Color(.65f, .6f, 1, 1);
            else if (energy[i] == 'G')
                orbs[i+1].color = new Color(.45f, .7f, .4f, 1);
            else if (energy[i] == 'R')
                orbs[i+1].color = new Color(.9f, .2f, .25f, 1);
            else if (energy[i] == 'W')
                orbs[i+1].color = new Color(.9f, .9f, .9f, 1);
            else if (energy[i] == 'B')
                orbs[i+1].color = new Color(.45f, .4f, 1, 1);
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

    public void SelectorButton()
    {
        string actName = DatabaseScript.GetActionData(m_boardScript.m_currPlayer.GetComponent<CharacterScript>().m_currAction, DatabaseScript.actions.NAME);

        if (m_parent.name == "Status Selector")
        {
            StatusScript statScript = m_main.m_cScript.GetComponents<StatusScript>()[m_main.m_cScript.m_currStatus];
            if (actName == "Hack ATK")
                StatusScript.NewStatus(m_boardScript.m_currPlayer, statScript.m_action);
            if (actName == "Hack ATK" || actName == "Disrupting ATK")
                statScript.DestroyStatus(m_main.m_cScript.transform.root.gameObject);
            else if (actName == "Extension")
            {
                statScript.m_lifeSpan++;

                for (int i = 0; i < statScript.m_statMod.Length; i++)
                {
                    if (statScript.m_statMod[i] > 0)
                        statScript.m_statMod[i]++;
                    else if (statScript.m_statMod[i] < 0)
                        statScript.m_statMod[i]--;
                }
            }
            else if (actName == "Modification")
                for (int i = 0; i < statScript.m_statMod.Length; i++)
                    m_main.m_cScript.m_stats[i] += statScript.m_statMod[i];

            ResumeGame();
        }
        else if (m_parent.name == "Energy Selector")
        {
            if (actName == "Prismatic ATK")
                AddEnergy(2);
            else if (actName == "Channel")
                AddEnergy(1);
            else if (actName == "Syphon ATK")
                SubtractEnergy(1, m_main.m_cScript.m_player.GetComponent<PlayerScript>().m_energy);
            else if (actName == "Deplete ATK")
                SubtractEnergy(3, m_main.m_cScript.m_player.GetComponent<PlayerScript>().m_energy);
        }
    }

    public void AddEnergy(int _max)
    {
        int total = 0;
        for (int i = 0; i < m_parent.m_images.Length; i++)
           total += int.Parse(m_parent.m_images[i].GetComponentInChildren<Text>().text);

        if (total < _max)
        {
            Text text = m_parent.m_images[int.Parse(gameObject.name)].GetComponentInChildren<Text>();
            text.text = (int.Parse(text.text) + 1).ToString();
        }
    }

    public void SubtractEnergy(int _min, int[] _playerEnergy)
    {
        int currTotal = 0;
        int origTotal = 0;

        for (int i = 0; i < m_parent.m_images.Length; i++)
        {
            currTotal += int.Parse(m_parent.m_images[i].GetComponentInChildren<Text>().text);
            origTotal += _playerEnergy[i];
        }

        if (_min > origTotal - currTotal && _playerEnergy[int.Parse(gameObject.name)] > 0)
        {
            Text text = m_parent.m_images[int.Parse(gameObject.name)].GetComponentInChildren<Text>();
            text.text = (int.Parse(text.text) - 1).ToString();
        }
    }

    public void ConfirmEnergySelection()
    {
        string actName = DatabaseScript.GetActionData(m_boardScript.m_currPlayer.GetComponent<CharacterScript>().m_currAction, DatabaseScript.actions.NAME);
        CharacterScript charScript = m_boardScript.m_currPlayer.GetComponent<CharacterScript>();
        PlayerScript playScript = charScript.m_player.GetComponent<PlayerScript>();

        if (actName == "Syphon ATK" | actName == "Deplete ATK")
        {
            for (int i = 0; i < m_parent.m_images.Length; i++)
            {
                if (actName == "Syphon ATK")
                    playScript.m_energy[i] += m_main.m_cScript.m_player.GetComponent<PlayerScript>().m_energy[i] - int.Parse(m_parent.m_images[i].GetComponentInChildren<Text>().text);

                m_main.m_cScript.m_player.GetComponent<PlayerScript>().m_energy[i] = int.Parse(m_parent.m_images[i].GetComponentInChildren<Text>().text);
            }
        }
        else
            for (int i = 0; i < m_parent.m_images.Length; i++)
                playScript.m_energy[i] += int.Parse(m_parent.m_images[i].GetComponentInChildren<Text>().text);

        playScript.SetEnergyPanel();
        ResumeGame();
    }

    public void ResetEnergySelection()
    {
        string actName = DatabaseScript.GetActionData(m_boardScript.m_currPlayer.GetComponent<CharacterScript>().m_currAction, DatabaseScript.actions.NAME);
        if (actName == "Syphon ATK")
            for (int i = 0; i < m_parent.m_images.Length; i++)
                m_parent.m_images[i].GetComponentInChildren<Text>().text = m_main.m_cScript.m_player.GetComponent<PlayerScript>().m_energy[i].ToString();
        else
            for (int i = 0; i < m_parent.m_images.Length; i++)
                m_parent.m_images[i].GetComponentInChildren<Text>().text = "0";
    }

    public void ResumeGame()
    {
        m_parent.m_cScript.m_boardScript.m_isForcedMove = null;
        PanelScript.CloseHistory();
    }

    public void ConfirmationButton(string _confirm)
    {
        if (!m_main || PanelScript.GetRecentHistory() && PanelScript.GetRecentHistory().name != "Confirmation Panel")
            m_main = PanelScript.GetRecentHistory();
        m_parent.PopulatePanel();
        gameObject.GetComponent<Button>().onClick.RemoveAllListeners();

        CharacterScript charScript = null;
        if (m_character)
            charScript = m_character.GetComponent<CharacterScript>();

        if (_confirm == "Action")
        {
            gameObject.GetComponent<Button>().onClick.AddListener(() => charScript.Action());
        }
        else if (_confirm == "Cancel New Action")
        {
            TeamMenuScript tMenuScript = m_main.m_main.GetComponent<TeamMenuScript>();
            tMenuScript.m_oldButton = EventSystem.current.currentSelectedGameObject.GetComponent<Button>();
            gameObject.GetComponent<Button>().onClick.AddListener(() => tMenuScript.CloseLevelPanel(TeamMenuScript.menuPans.NEW_ACTION_PANEL));
        }
        else if (_confirm == "Move")
        {
            TileScript moverTile = charScript.m_tile.GetComponent<TileScript>();
            if (m_boardScript.m_isForcedMove)
            {
                charScript = m_boardScript.m_isForcedMove.GetComponent<CharacterScript>();
                moverTile = charScript.m_tile.GetComponent<TileScript>();
            }

            gameObject.GetComponent<Button>().onClick.AddListener(() => charScript.Movement(moverTile, m_boardScript.m_selected.GetComponent<TileScript>(), false));
        }
        else if (_confirm == "New Action")
        {
            TeamMenuScript tMenuScript = m_main.m_main.GetComponent<TeamMenuScript>();
            tMenuScript.m_oldButton = EventSystem.current.currentSelectedGameObject.GetComponent<Button>();
            gameObject.GetComponent<Button>().onClick.AddListener(() => tMenuScript.ReplaceAction());
        }
        else if (_confirm == "New Stats")
        {
            TeamMenuScript tMenuScript = m_main.m_main.GetComponent<TeamMenuScript>();
            PanelScript statPanScript = PanelScript.m_allPanels[(int)TeamMenuScript.menuPans.CHAR_VIEW].m_panels[1].GetComponent<PanelScript>();
            Button newStat = EventSystem.current.currentSelectedGameObject.GetComponent<Button>();
            string[] statSeparated = newStat.name.Split('|');

            for (int i = 0; i < 9; i++)
            {
                string[] s = statSeparated[i + 1].Split(':');
                string[] textSeparated = statPanScript.m_text[i].text.Split(':');
                int newNumber = m_main.m_cScript.m_stats[i] + int.Parse(s[1]);
                if (i == 0)
                    statPanScript.m_text[i].text = textSeparated[0] + ": " + newNumber.ToString() + "/" + newNumber.ToString();
                else if (i == (int)CharacterScript.sts.HIT || i == (int)CharacterScript.sts.EVA || i == (int)CharacterScript.sts.CRT)
                    statPanScript.m_text[i].text = textSeparated[0] + ": " + newNumber.ToString() + "%";
                else
                    statPanScript.m_text[i].text = textSeparated[0] + ": " + newNumber.ToString();

                if (int.Parse(s[1]) > 0)
                    statPanScript.m_text[i].color = new Color(0, .7f, .7f, 1);
                else if (int.Parse(s[1]) < 0)
                    statPanScript.m_text[i].color = new Color(.9f, .4f, 0, 1);
                else
                    statPanScript.m_text[i].color = Color.black;
            }

            if (tMenuScript.m_statButton)
                tMenuScript.m_statButton.image.color = Color.white;

            tMenuScript.m_statButton = newStat;
            newStat.image.color = Color.cyan;
            gameObject.GetComponent<Button>().onClick.AddListener(() => tMenuScript.AddStatAlteration());
        }
        else if (_confirm == "Pass")
        {
            charScript = m_main.m_cScript;
            gameObject.GetComponent<Button>().onClick.AddListener(() => charScript.Pass());
        }
        else if (_confirm == "Remove")
        {
            TeamMenuScript tMenuScript = m_parent.m_main.GetComponent<TeamMenuScript>();
            gameObject.GetComponent<Button>().onClick.AddListener(() => tMenuScript.Remove());
        }
        else if (_confirm == "Save")
        {
                TeamMenuScript tMenuScript = m_parent.m_main.GetComponent<TeamMenuScript>();
                tMenuScript.m_saveButton = EventSystem.current.currentSelectedGameObject.GetComponent<Button>();

                gameObject.GetComponent<Button>().onClick.AddListener(() => tMenuScript.Save());
        }
    }
}
