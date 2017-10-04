using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class ButtonScript : MonoBehaviour {

    // Get rid of board
    public BoardScript m_boardScript;
    public PanelScript m_main;
    public CharacterScript m_cScript;
    public PanelScript m_parent;
    public GameObject m_camera;
    public GameObject[] m_energyPanel;
    public bool m_hovered;
    public string m_action;

	// Use this for initialization
	void Start ()
    {
        m_hovered = false;
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void HoverTrue(BaseEventData eventData)
    {
        m_boardScript.m_audio.PlayOneShot(Resources.Load<AudioClip>("Sounds/Menu Sound 3"));
        // REFACTOR: Make this more like how status images are handled
        if (GetComponent<Button>() && GetComponent<Button>().name == "Turn Panel Energy Button")
        {
            CharacterScript charScript = m_cScript.GetComponent<CharacterScript>();

            //Image turnPanImage = charScript.turnPanel.GetComponent<Image>();
            //turnPanImage.color = Color.cyan;
            
            Renderer charRenderer = m_cScript.GetComponentInChildren<Renderer>();
            charRenderer.material.color = Color.cyan;
            PanelScript hudPanScript = PanelScript.GetPanel("HUD Panel RIGHT");
            hudPanScript.m_cScript = charScript;
            hudPanScript.PopulatePanel();

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
        if (m_boardScript.m_currButton)
            return;

        if (m_parent.name == "Move Pass Panel")
        {
            m_hovered = true;
            m_cScript = m_boardScript.m_currCharScript;
            if (gameObject.name == "Move")
                m_boardScript.m_currCharScript.MovementSelection(0);
            return;
        }

        Text text = GetComponent<Button>().GetComponentInChildren<Text>();

        // REFACTOR: That's so wack though
        if (text.text == "EMPTY" || !m_main || !m_parent || GetComponent<Image>().color == new Color(1, .5f, .5f, 1) ||
            m_parent && m_parent.m_main && m_parent.m_main.GetComponent<PanelScript>() && !m_parent.m_main.GetComponent<PanelScript>().m_inView)
            return;

        m_main.m_cScript = m_parent.m_cScript.GetComponent<CharacterScript>();
        if (m_parent.m_inView) // Need this check to avoid selecting another action while menu is moving
            m_main.m_cScript.m_currAction = m_action;
        if (m_parent.m_inView && m_boardScript) // Need this check to avoid selecting another action while menu is moving
        {
            if (GetComponent<Button>().GetComponent<Image>().color == PanelScript.b_isFree)
                m_cScript.m_isFree = GetComponent<Button>();
            m_cScript.m_currAction = m_action;
            m_cScript.ActionTargeting();
        }

        m_main.PopulatePanel();
        m_hovered = true;
    }

    public void HoverFalse()
    {
        if (gameObject.tag == "Action Button" && GetComponent<Button>().GetComponentInChildren<Text>().text == "EMPTY")
            return;

        if (GetComponent<Button>() && GetComponent<Button>().name == "Turn Panel Energy Button")
        {
            //Image turnPanImage = charScript.turnPanel.GetComponent<Image>();
            //turnPanImage.color = Color.cyan;
            Renderer charRenderer = m_cScript.gameObject.GetComponentInChildren<Renderer>();
            charRenderer.material.color = m_cScript.m_teamColor;
            PanelScript hudPanScript = PanelScript.GetPanel("HUD Panel RIGHT");
            hudPanScript.m_inView = false;
            
            return;
        }
        else if (gameObject.tag == "Status Image")
            m_main.m_inView = false;

        if (!m_main)
            return;
        
        if (!m_boardScript.m_currButton)
            m_main.m_inView = false;

        m_hovered = false;

        if (m_boardScript && !m_boardScript.m_currButton && GetComponent<Image>().color == new Color(1, 1, 1, 1))
        {
            TileScript selectedTileScript = m_cScript.m_tile.GetComponent<TileScript>();
            if (selectedTileScript.m_radius.Count > 0)
                selectedTileScript.ClearRadius(selectedTileScript);

            if (m_boardScript.m_highlightedTile)
            {
                selectedTileScript = m_boardScript.m_highlightedTile.GetComponent<TileScript>();
                if (selectedTileScript.m_targetRadius.Count > 0)
                    selectedTileScript.ClearRadius(selectedTileScript);
            }
        }
    }

    public void Select()
    {
        if (m_cScript != m_boardScript.m_currCharScript)
            return;

        if (m_boardScript.m_currButton)
        {
            if (!m_hovered)
            {
                TileScript selectedTileScript = m_boardScript.m_currCharScript.m_tile.GetComponent<TileScript>();
                if (selectedTileScript.m_radius.Count > 0)
                    selectedTileScript.ClearRadius(selectedTileScript);

                if (m_boardScript.m_highlightedTile)
                {
                    selectedTileScript = m_boardScript.m_highlightedTile.GetComponent<TileScript>();
                    if (selectedTileScript.m_targetRadius.Count > 0)
                        selectedTileScript.ClearRadius(selectedTileScript);
                }
            }

            m_boardScript.m_currButton.GetComponent<Image>().color = Color.white;

            if (gameObject == m_boardScript.m_currButton.gameObject)
            {
                m_boardScript.m_currButton = null;
                if (!m_hovered)
                    GetComponent<ButtonScript>().m_main.m_inView = false;
                return;
            }
        }
        
        GetComponent<Image>().color = Color.cyan;
        m_boardScript.m_currButton = GetComponent<Button>();
        m_boardScript.m_currCharScript.m_currAction = m_action;
        m_boardScript.m_currButton.GetComponent<ButtonScript>().m_main.m_cScript = m_boardScript.m_currCharScript;
        m_boardScript.m_currButton.GetComponent<ButtonScript>().m_main.PopulatePanel();

        if (gameObject.tag == "Action Button")
        {
            m_boardScript.m_currButton.GetComponent<ButtonScript>().m_main.m_cScript = m_boardScript.m_currCharScript;
            m_boardScript.m_currButton.GetComponent<ButtonScript>().m_main.PopulatePanel();
            m_boardScript.m_currCharScript.ActionTargeting();
        }
        else if (gameObject.name == "Move")
            m_boardScript.m_currCharScript.MovementSelection(0);
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
                orbs[i+1].color = new Color(.7f, 1f, .65f, 1);
            else if (energy[i] == 'r')
                orbs[i+1].color = new Color(1, .45f, .5f, 1);
            else if (energy[i] == 'w')
                orbs[i+1].color = new Color(.85f, .85f, .85f, 1); // (1, 1, 1, 1)
            else if (energy[i] == 'b')
                orbs[i+1].color = new Color(.7f, .65f, 1, 1);
            else if (energy[i] == 'G')
                orbs[i+1].color = new Color(.4f, .65f, .35f, 1);
            else if (energy[i] == 'R')
                orbs[i+1].color = new Color(.85f, .15f, .2f, 1);
            else if (energy[i] == 'W')
                orbs[i+1].color = new Color(1, 1, 1, 1); // (.8f, .8f, .8f, 1)
            else if (energy[i] == 'B')
                orbs[i+1].color = new Color(.4f, .35f, 1, 1);
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
        camScript.m_target = m_cScript.gameObject;
    }

    public void SelectorButton()
    {
        string actName = DatabaseScript.GetActionData(m_boardScript.m_currCharScript.m_currAction, DatabaseScript.actions.NAME);
        CharacterScript charScript = m_boardScript.m_currCharScript;

        if (m_parent.name == "Status Selector")
        {
            StatusScript statScript = m_main.m_cScript.GetComponents<StatusScript>()[m_main.m_cScript.m_currStatus];
            //if (actName == "Hack ATK")
            //    StatusScript.NewStatus(m_boardScript.m_currPlayer, statScript.m_action);
            if (actName == "Disrupting ATK") // actName == "Hack ATK" || used to be here
                statScript.DestroyStatus(m_main.m_cScript.transform.root.gameObject);
            else if (actName == "Extension")
            {
                statScript.m_lifeSpan += 2 + charScript.m_tempStats[(int)CharacterScript.sts.TEC];

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

            ResumeGame();
        }
        else if (m_parent.name == "Energy Selector")
        {

            if (actName == "Prismatic ATK")
                    AddEnergy(2 + charScript.m_tempStats[(int)CharacterScript.sts.TEC]);
            else if (actName == "Deplete ATK" || actName == "Syphon ATK")
                SubtractEnergy(2 + charScript.m_tempStats[(int)CharacterScript.sts.TEC], m_main.m_cScript.m_player.GetComponent<PlayerScript>().m_energy);
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
        string actName = DatabaseScript.GetActionData(m_boardScript.m_currCharScript.m_currAction, DatabaseScript.actions.NAME);
        CharacterScript charScript = m_boardScript.m_currCharScript;
        PlayerScript playScript = charScript.m_player.GetComponent<PlayerScript>();
        int added = 0;

        if (actName == "Syphon ATK" || actName == "Deplete ATK")
        {
            for (int i = 0; i < m_parent.m_images.Length; i++)
            {
                if (actName == "Syphon ATK")
                    playScript.m_energy[i] += m_main.m_cScript.m_player.GetComponent<PlayerScript>().m_energy[i] - int.Parse(m_parent.m_images[i].GetComponentInChildren<Text>().text);

                added += m_main.m_cScript.m_player.GetComponent<PlayerScript>().m_energy[i] - int.Parse(m_parent.m_images[i].GetComponentInChildren<Text>().text);
                m_main.m_cScript.m_player.GetComponent<PlayerScript>().m_energy[i] = int.Parse(m_parent.m_images[i].GetComponentInChildren<Text>().text);
            }
        }
        else
            for (int i = 0; i < m_parent.m_images.Length; i++)
            {
                added += int.Parse(m_parent.m_images[i].GetComponentInChildren<Text>().text);
                playScript.m_energy[i] += int.Parse(m_parent.m_images[i].GetComponentInChildren<Text>().text);
            }

        if (added > 2)
            if (actName == "Prismatic ATK" || actName == "Syphon ATK" || actName == "Deplete ATK")
                charScript.ReceiveDamage((added - 2).ToString(), Color.white);

        playScript.SetEnergyPanel();
        ResumeGame();
    }

    public void ResetEnergySelection()
    {
        string actName = DatabaseScript.GetActionData(m_boardScript.m_currCharScript.m_currAction, DatabaseScript.actions.NAME);
        if (actName == "Syphon ATK" || actName == "Deplete ATK")
            for (int i = 0; i < m_parent.m_images.Length; i++)
                m_parent.m_images[i].GetComponentInChildren<Text>().text = m_main.m_cScript.m_player.GetComponent<PlayerScript>().m_energy[i].ToString();
        else
            for (int i = 0; i < m_parent.m_images.Length; i++)
                m_parent.m_images[i].GetComponentInChildren<Text>().text = "0";
    }

    public void ResumeGame()
    {
        m_parent.m_cScript.m_boardScript.m_isForcedMove = null;
        m_parent.m_cScript.m_boardScript.m_camIsFrozen = false;
        PanelScript.CloseHistory();
    }

    public void ConfirmationButton(string _confirm)
    {
        if (!m_main || PanelScript.GetRecentHistory() && PanelScript.GetRecentHistory().name != "Confirmation Panel")
            m_main = PanelScript.GetRecentHistory();
        m_parent.PopulatePanel();
        gameObject.GetComponent<Button>().onClick.RemoveAllListeners();

        if (_confirm == "Action")
        {
            gameObject.GetComponent<Button>().onClick.AddListener(() => m_cScript.ActionAnimation());
        }
        else if (_confirm == "Clear Team")
        {
            Button b = EventSystem.current.currentSelectedGameObject.GetComponent<Button>();
            TeamMenuScript t = m_parent.m_main.GetComponent<TeamMenuScript>();
            gameObject.GetComponent<Button>().onClick.AddListener(() => t.ClearTeam(b));
        }
        else if (_confirm == "Move")
        {
            TileScript moverTile = m_cScript.m_tile.GetComponent<TileScript>();
            TileScript selectedTile = m_boardScript.m_selected.GetComponent<TileScript>();
            if (m_boardScript.m_isForcedMove)
            {
                m_cScript = m_boardScript.m_isForcedMove.GetComponent<CharacterScript>();
                moverTile = m_cScript.m_tile.GetComponent<TileScript>();
            }

            gameObject.GetComponent<Button>().onClick.AddListener(() => m_cScript.Movement(moverTile, selectedTile, false));
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
            PanelScript statPanScript = PanelScript.GetPanel("CharacterViewer Panel").m_panels[1].GetComponent<PanelScript>();
            Button newStat = EventSystem.current.currentSelectedGameObject.GetComponent<Button>();
            string[] statSeparated = newStat.name.Split('|');

            for (int i = 1; i < 8; i++)
            {
                string[] s = statSeparated[i].Split(':');
                string[] textSeparated = statPanScript.m_text[i].text.Split(':');
                int newNumber = m_main.m_cScript.m_stats[i - 1] + int.Parse(s[1]);
                if (i == 0)
                    statPanScript.m_text[i].text = textSeparated[0] + ": " + newNumber.ToString() + "/" + newNumber.ToString();
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
        else if (_confirm == "None Action")
        {
            TeamMenuScript tMenuScript = m_main.m_main.GetComponent<TeamMenuScript>();
            tMenuScript.m_oldButton = EventSystem.current.currentSelectedGameObject.GetComponent<Button>();
            gameObject.GetComponent<Button>().onClick.AddListener(() => tMenuScript.CloseLevelPanel(0));
        }
        else if (_confirm == "None Stats")
        {
            TeamMenuScript tMenuScript = m_main.m_main.GetComponent<TeamMenuScript>();
            tMenuScript.m_statButton = null;
            gameObject.GetComponent<Button>().onClick.AddListener(() => tMenuScript.CloseLevelPanel(1));
        }
        else if (_confirm == "Pass")
        {
            PanelScript.GetPanel("HUD Panel LEFT").m_panels[(int)CharacterScript.HUDPan.MOV_PASS].GetComponent<PanelScript>().m_buttons[1].GetComponent<ButtonScript>().Select();
            PanelScript.GetPanel("Confirmation Panel").m_buttons[1].onClick.AddListener(() => m_boardScript.Pass());
        }
        else if (_confirm == "Random Team")
        {
            Button b = EventSystem.current.currentSelectedGameObject.GetComponent<Button>();
            TeamMenuScript t = m_parent.m_main.GetComponent<TeamMenuScript>();
            gameObject.GetComponent<Button>().onClick.AddListener(() => t.RandomTeam(b));
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

    public void ChangeScreen(string _screen)
    {
        SceneManager.LoadScene(_screen);
    }
}
