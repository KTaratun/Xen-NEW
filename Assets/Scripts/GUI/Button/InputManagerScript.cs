using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class InputManagerScript : MonoBehaviour {

	// Use this for initialization
	void Start ()
    {
	}
	
	// Update is called once per frame
	void Update ()
    {
        // REFACTOR
        if (Input.GetMouseButtonDown(1) && name == "Confirmation Panel" && PanelManagerScript.m_history.Count > 0)
            if (PanelManagerScript.GetCurrentPanel().name == "Confirmation Panel" || PanelManagerScript.GetCurrentPanel().name != "Confirmation Panel" && !PanelManagerScript.m_locked)
                OnRightClick();
    }

    public void OnRightClick()
    {
        //AudioClip a = Resources.Load<AudioClip>("Sounds/Menu Sound 2");
        //m_audio.PlayOneShot(a);

        BoardScript board = GameObject.Find("Board").GetComponent<BoardScript>();

        if (board && board.m_selected)
            board.m_selected = null;

        if (PanelManagerScript.m_history.Count > 0)
        {
            PanelManagerScript.RemoveFromHistory("");
            if (PanelManagerScript.m_history.Count > 0)
                PanelManagerScript.m_history[PanelManagerScript.m_history.Count - 1].m_slideScript.m_inView = true;
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

    static public void ResumeGame()
    {
        BoardScript board = GameObject.Find("Board").GetComponent<BoardScript>();

        board.m_isForcedMove = null;
        board.m_camIsFrozen = false;
        PanelManagerScript.CloseHistory();
    }

    static public void ConfirmationButton(Button _button, string _confirm)
    {
        PanelScript parent = null;
        GameObject gO = _button.gameObject;
        ButtonScript buttScript = _button.GetComponent<ButtonScript>();

        if (!buttScript.m_main || PanelManagerScript.GetCurrentPanel() && PanelManagerScript.GetCurrentPanel().name != "Confirmation Panel")
            parent = PanelManagerScript.GetCurrentPanel();

        parent.PopulatePanel();
        _button.GetComponent<Button>().onClick.RemoveAllListeners();

        if (_confirm == "Action")
        {
            _button.GetComponent<Button>().onClick.AddListener(() => ActionScript.ActionStart(buttScript.m_object.GetComponent<CharacterScript>()));
        }
        else if (_confirm == "Choose Panel")
        {
            _button.GetComponent<Button>().onClick.AddListener(() => CloseAll());
        }
        else if (_confirm == "Clear Team")
        {
            Button b = EventSystem.current.currentSelectedGameObject.GetComponent<Button>();
            TeamMenuScript t = parent.m_main.GetComponent<TeamMenuScript>();
            _button.GetComponent<Button>().onClick.AddListener(() => t.ClearTeam(b));
        }
        else if (_confirm == "Move")
        {
            bool isForcedMove = false;
            if (buttScript.m_boardScript.m_isForcedMove)
            {
                buttScript.m_object = buttScript.m_boardScript.m_isForcedMove;
                isForcedMove = true;
            }

            _button.GetComponent<Button>().onClick.AddListener(() => buttScript.m_object.GetComponent<ObjectScript>().MovingStart(buttScript.m_boardScript.m_selected, isForcedMove, false));
        }
        else if (_confirm == "New Action")
        {
            TeamMenuScript tMenuScript = buttScript.m_main.m_main.GetComponent<TeamMenuScript>();
            tMenuScript.m_oldButton = EventSystem.current.currentSelectedGameObject.GetComponent<Button>();
            _button.GetComponent<Button>().onClick.AddListener(() => tMenuScript.ReplaceAction());
        }
        else if (_confirm == "New Stats")
        {
            TeamMenuScript tMenuScript = buttScript.m_main.m_main.GetComponent<TeamMenuScript>();
            PanelScript statPanScript = PanelManagerScript.GetPanel("CharacterViewer Panel").m_panels[1].GetComponent<PanelScript>();
            Button newStat = EventSystem.current.currentSelectedGameObject.GetComponent<Button>();
            string[] statSeparated = newStat.name.Split('|');

            for (int i = 1; i < 8; i++)
            {
                string[] s = statSeparated[i].Split(':');
                string[] textSeparated = statPanScript.m_text[i].text.Split(':');
                int newNumber = buttScript.m_main.m_cScript.m_stats[i - 1] + int.Parse(s[1]);
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
            _button.GetComponent<Button>().onClick.AddListener(() => tMenuScript.AddStatAlteration());
        }
        else if (_confirm == "None Action")
        {
            TeamMenuScript tMenuScript = buttScript.m_main.GetComponent<TeamMenuScript>();
            tMenuScript.m_oldButton = EventSystem.current.currentSelectedGameObject.GetComponent<Button>();
            _button.GetComponent<Button>().onClick.AddListener(() => tMenuScript.CloseLevelPanel(0));
        }
        else if (_confirm == "None Stats")
        {
            TeamMenuScript tMenuScript = buttScript.m_main.GetComponent<TeamMenuScript>();
            tMenuScript.m_statButton = null;
            _button.GetComponent<Button>().onClick.AddListener(() => tMenuScript.CloseLevelPanel(1));
        }
        else if (_confirm == "Pass")
        {
            PanelManagerScript.GetPanel("HUD Panel LEFT").m_panels[(int)PanelScript.HUDPan.MOV_PASS].GetComponent<PanelScript>().m_buttons[1].GetComponent<ButtonScript>().Select();
            PanelManagerScript.GetPanel("Confirmation Panel").m_buttons[1].onClick.AddListener(() => buttScript.m_boardScript.Pass());
        }
        else if (_confirm == "Random Team")
        {
            Button b = EventSystem.current.currentSelectedGameObject.GetComponent<Button>();
            TeamMenuScript t = parent.m_main.GetComponent<TeamMenuScript>();
            _button.GetComponent<Button>().onClick.AddListener(() => t.RandomTeam(b));
        }
        else if (_confirm == "Remove")
        {
            TeamMenuScript tMenuScript = parent.m_main.GetComponent<TeamMenuScript>();
            _button.GetComponent<Button>().onClick.AddListener(() => tMenuScript.Remove());
        }
        else if (_confirm == "Save")
        {
                TeamMenuScript tMenuScript = parent.m_main.GetComponent<TeamMenuScript>();
                tMenuScript.m_saveButton = EventSystem.current.currentSelectedGameObject.GetComponent<Button>();

            _button.GetComponent<Button>().onClick.AddListener(() => tMenuScript.Save());
        }
    }

    static private void CloseAll()
    {
        BoardScript board = GameObject.Find("Board").GetComponent<BoardScript>();

        board.m_highlightedTile = null;
        board.m_selected = null;
        board.m_isForcedMove = null;
        board.m_currCharScript.m_currAction = "";
        board.m_currCharScript.m_tile.ClearRadius();

        PanelManagerScript.GetPanel("HUD Panel RIGHT").m_slideScript.ClosePanel();
        PanelManagerScript.CloseHistory();
        PanelManagerScript.GetPanel("HUD Panel LEFT").PopulatePanel();
    }

    public void ChangeScreen(string _screen)
    {
        SceneManager.LoadScene(_screen);
    }
}
