using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class ButtonScript : MonoBehaviour {

    // General
    public string m_action;
    public bool m_hovered;
    public GameObject m_object;
    public Color m_oldColor;
    public GameObject[] m_energyPanel;

    // References
    public GameObject m_camera;
    public PanelScript m_main;
    public PanelScript m_parent;
    public BoardScript m_boardScript;
    public AudioSource m_audio;



	// Use this for initialization
	void Start ()
    {
        m_audio = gameObject.AddComponent<AudioSource>();
        m_hovered = false;
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void HoverTrue(BaseEventData eventData)
    {
        if (m_boardScript && m_boardScript.m_isForcedMove && m_parent.tag != "Selector" ||
            PanelScript.m_confirmPanel.m_inView && m_parent != PanelScript.m_confirmPanel ||
            gameObject.tag == "Action Button" && m_boardScript && m_boardScript.m_camIsFrozen ||
            m_boardScript && m_boardScript.m_currCharScript.m_isAI)
            return;

        if (GetComponent<Button>() && GetComponent<Button>().interactable)
            m_audio.PlayOneShot(Resources.Load<AudioClip>("Sounds/Menu Sound 3"));

        if (m_boardScript && GetComponent<Button>())
            m_boardScript.m_hoverButton = GetComponent<Button>();

        // REFACTOR: Make this more like how status images are handled
        if (GetComponent<Button>() && GetComponent<Button>().name == "Turn Panel Energy Button")
        {
            CharacterScript charScript = m_object.GetComponent<CharacterScript>();

            //Image turnPanImage = charScript.turnPanel.GetComponent<Image>();
            //turnPanImage.color = Color.cyan;
            
            Renderer charRenderer = charScript.GetComponentInChildren<Renderer>();
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

        if (m_boardScript && m_boardScript.m_currButton)
            return;

        if (m_parent && m_parent.name == "Move Pass Panel")
        {
            m_hovered = true;
            m_object = m_boardScript.m_currCharScript.gameObject;
            if (gameObject.name == "Move")
                m_boardScript.m_currCharScript.MovementSelection(0);
            return;
        }
        else if (tag == "Action Button")
        {
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
                {
                    m_boardScript.m_currCharScript.m_isFree = GetComponent<Button>();
                    m_boardScript.m_currCharScript.m_currAction = m_action;
                    m_boardScript.m_currCharScript.ActionTargeting(m_boardScript.m_currCharScript.m_tile);
                }
                else
                {
                    CharacterScript charScript = m_object.GetComponent<CharacterScript>();
                    charScript.m_currAction = m_action;
                    charScript.ActionTargeting(charScript.m_tile);
                }
            }

            m_main.PopulatePanel();
            if (GetComponent<Button>().IsInteractable() && GetComponent<PanelScript>())
                GetComponent<PanelScript>().m_inView = true;
            m_hovered = true;
        }
    }

    public void HoverFalse()
    {
        if (m_boardScript)
            m_boardScript.m_hoverButton = null;

        if (gameObject.tag == "Action Button" && GetComponent<Button>().GetComponentInChildren<Text>().text == "EMPTY" || m_boardScript && m_boardScript.m_currCharScript.m_isAI
            || name == "Move" && m_boardScript.m_isForcedMove)
            return;

        if (GetComponent<Button>() && GetComponent<Button>().name == "Turn Panel Energy Button")
        {
            //Image turnPanImage = charScript.turnPanel.GetComponent<Image>();
            //turnPanImage.color = Color.cyan;
            Renderer charRenderer = m_object.GetComponentInChildren<Renderer>();
            charRenderer.material.color = m_object.GetComponent<CharacterScript>().m_teamColor;
            PanelScript hudPanScript = PanelScript.GetPanel("HUD Panel RIGHT");
            hudPanScript.ClosePanel();
            
            return;
        }
        else if (gameObject.tag == "Status Image")
        {
            m_main.ClosePanel();
            return;
        }

        m_hovered = false;

        if (!m_main)
            return;

        if (m_boardScript && !m_boardScript.m_currButton || !m_boardScript)
        {
            m_main.ClosePanel();
            if (m_boardScript)
                m_boardScript.m_currCharScript.m_currAction = "";
        }

        if (m_boardScript && !m_boardScript.m_currButton && GetComponent<Image>().color != Color.cyan)
        {
            if (GetComponent<PanelScript>())
                GetComponent<PanelScript>().m_inView = false;
            TileScript selectedTileScript = null;
            if (GetComponent<Button>().GetComponent<Image>().color == PanelScript.b_isFree)
                selectedTileScript = m_boardScript.m_currCharScript.m_tile;
            else
                selectedTileScript = m_object.GetComponent<CharacterScript>().m_tile;

            if (selectedTileScript.m_radius.Count > 0)
                selectedTileScript.ClearRadius();

            if (m_boardScript.m_highlightedTile)
            {
                selectedTileScript = m_boardScript.m_highlightedTile;
                if (selectedTileScript.m_targetRadius.Count > 0)
                    selectedTileScript.ClearRadius();
            }
        }
    }
    
    public void Select()
    {
        if (GetComponent<Button>().GetComponent<Image>().color == PanelScript.b_isDisallowed ||
            m_boardScript && m_object != m_boardScript.m_currCharScript.gameObject && GetComponent<Button>().GetComponent<Image>().color == Color.white ||
            m_boardScript && m_boardScript.m_isForcedMove && m_parent && m_parent.tag != "Selector" || 
            PanelScript.m_confirmPanel.m_inView && m_parent != PanelScript.m_confirmPanel ||
            m_boardScript && m_boardScript.m_camIsFrozen ||
            m_boardScript && m_boardScript.m_currCharScript.m_isAI)
            return;

        TileScript selectedTileScript = m_boardScript.m_currCharScript.m_tile.GetComponent<TileScript>();
        if (selectedTileScript.m_radius.Count > 0)
            selectedTileScript.ClearRadius();

        if (m_boardScript.m_highlightedTile)
        {
            selectedTileScript = m_boardScript.m_highlightedTile.GetComponent<TileScript>();
            selectedTileScript.ClearRadius();
        }

        if (m_boardScript.m_currButton)
        {
            m_boardScript.m_currButton.GetComponent<Image>().color = m_boardScript.m_currButton.GetComponent<ButtonScript>().m_oldColor;
            if (m_boardScript.m_currButton.GetComponent<PanelScript>())
                m_boardScript.m_currButton.GetComponent<PanelScript>().m_inView = false;

            if (gameObject == m_boardScript.m_currButton.gameObject)
            {
                m_boardScript.m_currButton = null;
                m_boardScript.m_currCharScript.m_currAction = "";
                if (!m_hovered)
                    GetComponent<ButtonScript>().m_main.ClosePanel();
                return;
            }
        }

        m_oldColor = GetComponent<Image>().color;
        GetComponent<Image>().color = Color.cyan;
        if (GetComponent<PanelScript>())
            GetComponent<PanelScript>().m_inView = true;
        m_boardScript.m_currButton = GetComponent<Button>();
        m_boardScript.m_currCharScript.m_currAction = m_action;
        m_boardScript.m_currButton.GetComponent<ButtonScript>().m_main.m_cScript = m_boardScript.m_currCharScript;
        m_boardScript.m_currButton.GetComponent<ButtonScript>().m_main.PopulatePanel();

        if (gameObject.tag == "Action Button")
        {

            if (PanelScript.GetPanel("Choose Panel").m_inView)
                m_boardScript.m_isForcedMove = null;

            PanelScript.GetPanel("ActionViewer Panel").m_panels[1].ClosePanel();

            if (m_oldColor == PanelScript.b_isSpecial)
            {
                m_boardScript.m_selected.ClearRadius();
                PanelScript.GetPanel("HUD Panel RIGHT").ClosePanel();
                return;
            }

            m_boardScript.m_currButton.GetComponent<ButtonScript>().m_main.m_cScript = m_boardScript.m_currCharScript;
            m_boardScript.m_currButton.GetComponent<ButtonScript>().m_main.PopulatePanel();
            m_boardScript.m_currCharScript.ActionTargeting(m_boardScript.m_currCharScript.m_tile);

            if (PanelScript.GetPanel("HUD Panel RIGHT").m_inView)
                PanelScript.GetPanel("HUD Panel RIGHT").PopulatePanel();
        }
        else if (gameObject.name == "Move" && !PanelScript.GetPanel("Choose Panel").m_inView)
        {
            PanelScript.GetPanel("ActionViewer Panel").ClosePanel();
            m_boardScript.m_currCharScript.MovementSelection(0);
        }
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
        camScript.m_target = m_object;
    }

    public void SelectorButton()
    {
        string actName = DatabaseScript.GetActionData(m_boardScript.m_currCharScript.m_currAction, DatabaseScript.actions.NAME);
        CharacterScript charScript = m_boardScript.m_currCharScript;

        if (m_parent.name == "Status Selector")
        {
            StatusScript statScript = m_parent.m_cScript.GetComponents<StatusScript>()[m_parent.m_cScript.m_currStatus];
            if (actName == "SUP(Delete)")
                statScript.DestroyStatus(m_parent.m_cScript.transform.root.gameObject, true);
            else if (actName == "SUP(Extension)")
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

            ResumeGame();
        }
        else if (m_parent.name == "Energy Selector")
        {

            if (actName == "ATK(Prismatic)")
                    AddEnergy(2 + charScript.m_tempStats[(int)CharacterScript.sts.TEC]);
            else if (actName == "ATK(Deplete)" || actName == "ATK(Syphon)")
                SubtractEnergy(2 + charScript.m_tempStats[(int)CharacterScript.sts.TEC], m_main.m_cScript.m_player.m_energy);
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
        PlayerScript playScript = charScript.m_player;
        int added = 0;

        if (actName == "ATK(Syphon)" || actName == "ATK(Deplete)")
        {
            for (int i = 0; i < m_parent.m_images.Length; i++)
            {
                if (actName == "ATK(Syphon)")
                    playScript.m_energy[i] += m_main.m_cScript.m_player.m_energy[i] - int.Parse(m_parent.m_images[i].GetComponentInChildren<Text>().text);

                added += m_main.m_cScript.m_player.m_energy[i] - int.Parse(m_parent.m_images[i].GetComponentInChildren<Text>().text);
                m_main.m_cScript.m_player.m_energy[i] = int.Parse(m_parent.m_images[i].GetComponentInChildren<Text>().text);
            }
        }
        else
            for (int i = 0; i < m_parent.m_images.Length; i++)
            {
                added += int.Parse(m_parent.m_images[i].GetComponentInChildren<Text>().text);
                playScript.m_energy[i] += int.Parse(m_parent.m_images[i].GetComponentInChildren<Text>().text);
            }

        if (added > 2)
            if (actName == "ATK(Prismatic)" || actName == "ATK(Syphon)" || actName == "ATK(Deplete)")
                charScript.ReceiveDamage((added - 2).ToString(), Color.white);

        playScript.SetEnergyPanel(charScript);
        ResumeGame();
    }

    public void ResetEnergySelection()
    {
        string actName = DatabaseScript.GetActionData(m_boardScript.m_currCharScript.m_currAction, DatabaseScript.actions.NAME);
        if (actName == "ATK(Syphon)" || actName == "ATK(Deplete)")
            for (int i = 0; i < m_parent.m_images.Length; i++)
                m_parent.m_images[i].GetComponentInChildren<Text>().text = m_main.m_cScript.m_player.m_energy[i].ToString();
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
            gameObject.GetComponent<Button>().onClick.AddListener(() => m_object.GetComponent<CharacterScript>().ActionStart());
        }
        else if (_confirm == "Choose Panel")
        {
            gameObject.GetComponent<Button>().onClick.AddListener(() => CloseAll());
        }
        else if (_confirm == "Clear Team")
        {
            Button b = EventSystem.current.currentSelectedGameObject.GetComponent<Button>();
            TeamMenuScript t = m_parent.m_main.GetComponent<TeamMenuScript>();
            gameObject.GetComponent<Button>().onClick.AddListener(() => t.ClearTeam(b));
        }
        else if (_confirm == "Move")
        {
            bool isForcedMove = false;
            if (m_boardScript.m_isForcedMove)
            {
                m_object = m_boardScript.m_isForcedMove;
                isForcedMove = true;
            }

            gameObject.GetComponent<Button>().onClick.AddListener(() => m_object.GetComponent<ObjectScript>().MovingStart(m_boardScript.m_selected, isForcedMove, false));
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
            PanelScript.GetPanel("HUD Panel LEFT").m_panels[(int)PanelScript.HUDPan.MOV_PASS].GetComponent<PanelScript>().m_buttons[1].GetComponent<ButtonScript>().Select();
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

    private void CloseAll()
    {
        m_boardScript.m_highlightedTile = null;
        m_boardScript.m_selected = null;
        m_boardScript.m_isForcedMove = null;
        m_boardScript.m_currCharScript.m_currAction = "";
        m_boardScript.m_currCharScript.m_tile.ClearRadius();
        PanelScript.GetPanel("HUD Panel RIGHT").ClosePanel();
        PanelScript.CloseHistory();
        PanelScript.GetPanel("HUD Panel LEFT").PopulatePanel();
    }

    public void ChangeScreen(string _screen)
    {
        SceneManager.LoadScene(_screen);
    }
}
