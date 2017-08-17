﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class TeamMenuScript : MonoBehaviour {

    public enum menuPans { CHAR_SLOTS, CHAR_PANEL, CHAR_VIEW, PRESELECT_PANEL, ACTION_VIEW, SAVE_LOAD_PANEL, CONFIRMATION_PANEL, TOT_PANELS }

    public List<PanelScript> m_panels;
    public Button m_currButton;
    public Button m_saveButton;
    public GameObject m_currCharacter;
    public GameObject m_character;

	// Use this for initialization
	void Start ()
    {
        MenuPanelInit("Canvas");
        TeamInit();

        GameObject newChar = Instantiate(m_character);
        m_currCharacter = newChar;

        //PlayerPrefs.DeleteAll();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void MenuPanelInit(string _canvasName)
    {
        string canvasName = _canvasName;
        Canvas can = GameObject.Find(canvasName).GetComponent<Canvas>();
        PanelScript[] pans = can.GetComponentsInChildren<PanelScript>();

        for (int i = 0; i < pans.Length; i++)
        {
            if (pans[i].transform.parent.name == _canvasName)
                m_panels.Add(pans[i]);
        }
    }

    public bool CheckIfPanelOpen()
    {
        for (int i = 0; i < m_panels.Count; i++)
        {
            if (m_panels[i].m_inView)
                return true;
        }
        return false;
    }

    public void TeamInit()
    {
        for (int i = 0; i < 4; i++)
        {
            PanelScript panScript = m_panels[(int)menuPans.CHAR_SLOTS].m_panels[i].GetComponent<PanelScript>();
            Button[] team = panScript.m_buttons;
            for (int j = 0; j < 6; j++)
            {
                string key = i.ToString() + ',' + j.ToString() + ",name";
                string name = PlayerPrefs.GetString(key);
                if (name.Length > 0)
                {
                    key = i.ToString() + ',' + j.ToString() + ",color";
                    string color = PlayerPrefs.GetString(key);

                    SetCharSlot(team[j], name, color);
                    team[j].onClick = new Button.ButtonClickedEvent();
                    team[j].onClick.AddListener(() => m_panels[(int)menuPans.CHAR_VIEW].PopulatePanel());
                }
            }
        }
    }

    public void SetCharSlot(Button _button, string _name, string _color)
    {
        Text t = _button.GetComponentInChildren<Text>();
        t.text = _name;

        ButtonScript buttScript = _button.GetComponent<ButtonScript>();
        buttScript.SetTotalEnergy(_color);

        _button.GetComponent<Image>().color = new Color(.85f, .85f, .85f, 1);
    }

    public void CharacterAssignment()
    {
        if (CheckIfPanelOpen())
            return;

        m_panels[(int)menuPans.CHAR_PANEL].PopulatePanel();
        m_currButton = EventSystem.current.currentSelectedGameObject.GetComponent<Button>();
    }

    public void NewCharacter()
    {
        if (!m_panels[(int)menuPans.CHAR_PANEL].m_inView)
            return;
    }

    public void RandomCharacter()
    {
        m_panels[(int)menuPans.CHAR_PANEL].m_inView = false;

        CharacterScript cScript = m_currCharacter.GetComponent<CharacterScript>();

        string actions = null;
        int color = Random.Range(0, 4);

        cScript.m_name = RandomName();

        if (color == 0)
            cScript.m_color = "G";
        else if (color == 1)
            cScript.m_color = "R";
        else if (color == 2)
            cScript.m_color = "W";
        else if (color == 3)
            cScript.m_color = "B";

        cScript.m_actions = new string[4];

        DatabaseScript dbScript = gameObject.GetComponent<DatabaseScript>();
        int prevAct = 0;

        for (int i = 0; i < 4; i++)
        {
            int min = 0;
            int max = 3;
   
            if (i == 1)
            {
                min = 3; max = 6;
            }
            else if (i == 2)
            {
                min = 6; max = 10;
            }
            else if (i == 3) // New characters can have two -1 actions
            {
                min = 6; max = 14;
            }

            int randAct;
            do
            {
                randAct = Random.Range(min, max);
            } while (randAct == prevAct);

            prevAct = randAct;
            cScript.m_actions[i] = dbScript.m_actions[randAct + color * 16];
        }

        DatabaseScript db = gameObject.GetComponent<DatabaseScript>();

        string[] stat = db.m_stat[Random.Range(0, db.m_stat.Length - 2)].Split('|');

        cScript.InitializeStats();
        for (int i = 0; i < cScript.m_stats.Length; i++) // We're not using the 2 rare stats yet
        {
            string[] currStat = stat[i+1].Split(':');
            cScript.m_stats[i] += int.Parse(currStat[1]);
            cScript.m_tempStats[i] += int.Parse(currStat[1]);
        }

        Select();

        // Choose the number of random characters
        // Choose level range
        // Choose color restraints
    }

    public string RandomName()
    {
        string name = null;
        int len = Random.Range(3, 9);
        string con = "bbbcccdffgghhjjjkkklllmmmnnnppqrssstttvwxyz";
        string vow = "aeiou";

        for (int i = 0; i <= len; i++)
        {
            if (i % 2 == 0)
                name += con[Random.Range(0, con.Length)];

            if (i % 2 == 1)
                name += vow[Random.Range(0, vow.Length)];
        }

        return name;
    }

    // REFACTOR: Move to populate panel
    //public void PopulateCharacterViewer()
    //{
    //    // If another panel is open, don't open character viewer for already loaded character
    //    Button currB = EventSystem.current.currentSelectedGameObject.GetComponent<Button>();
    //    int res = 0;
    //
    //    m_panels[(int)menuPans.PRESELECT_PANEL].m_inView = false;
    //
    //    if (CheckIfPanelOpen())
    //        return;
    //
    //    PanelScript charViewScript = m_panels[(int)menuPans.CHAR_VIEW];
    //    charViewScript.m_inView = true;
    //
    //    Button[] buttons = charViewScript.m_buttons;
    //
    //    PanelScript actionScript = charViewScript.m_panels[0].GetComponent<PanelScript>();
    //    PanelScript statPan = charViewScript.m_panels[1].GetComponent<PanelScript>();
    //
    //    CharacterScript currCharScript = m_currCharacter.GetComponent<CharacterScript>();
    //    currCharScript.InitializeStats();
    //    actionScript.m_cScript = currCharScript;
    //
    //    res = 0;
    //    if (int.TryParse(currB.name, out res)) // If last pressed button is an int, it's a preset character panel button. Character slot buttons names are in the x,x format
    //    {
    //        DatabaseScript dbScript = gameObject.GetComponent<DatabaseScript>();
    //
    //        string[] presetDataSeparated = dbScript.m_presets[int.Parse(currB.name)].Split('|');
    //        string[] presetName = presetDataSeparated[(int)DatabaseScript.presets.NAME].Split(':');
    //        string[] presetColor = presetDataSeparated[(int)DatabaseScript.presets.COLORS].Split(':');
    //
    //        FillOutCharacterData(presetName[1], presetColor[1], dbScript.GetActions(dbScript.m_presets[int.Parse(currB.name)]), 0);
    //        charViewScript.m_cScript = currCharScript;
    //        charViewScript.PopulatePanel();
    //
    //        // Determine if select or remove will be visible
    //        for (int i = 0; i < buttons.Length; i++)
    //        {
    //            if (buttons[i].name == "Select Button" && buttons[i].gameObject.transform.position.x > 1000)
    //                buttons[i].gameObject.transform.SetPositionAndRotation(new Vector3(buttons[i].gameObject.transform.position.x - 1000, buttons[i].gameObject.transform.position.y, buttons[i].gameObject.transform.position.z), buttons[i].gameObject.transform.rotation);
    //            if (buttons[i].name == "Remove Button" && buttons[i].gameObject.transform.position.x < 1000)
    //                buttons[i].gameObject.transform.SetPositionAndRotation(new Vector3(buttons[i].gameObject.transform.position.x + 1000, buttons[i].gameObject.transform.position.y, buttons[i].gameObject.transform.position.z), buttons[i].gameObject.transform.rotation);
    //        }
    //    }
    //    else
    //    {
    //        m_currButton = currB;
    //
    //        currCharScript = PlayerPrefScript.LoadChar(currB.name, currCharScript);
    //
    //        charViewScript.m_cScript = currCharScript;
    //        charViewScript.PopulatePanel();
    //
    //        // Determine if select or remove will be visible
    //        for (int i = 0; i < buttons.Length; i++)
    //        {
    //            if (buttons[i].name == "Select Button" && buttons[i].gameObject.transform.position.x < 1000)
    //                buttons[i].gameObject.transform.SetPositionAndRotation(new Vector3(buttons[i].gameObject.transform.position.x + 1000, buttons[i].gameObject.transform.position.y, buttons[i].gameObject.transform.position.z), buttons[i].gameObject.transform.rotation);
    //            if (buttons[i].name == "Remove Button" && buttons[i].gameObject.transform.position.x > 1000)
    //                buttons[i].gameObject.transform.SetPositionAndRotation(new Vector3(buttons[i].gameObject.transform.position.x - 1000, buttons[i].gameObject.transform.position.y, buttons[i].gameObject.transform.position.z), buttons[i].gameObject.transform.rotation);
    //        }
    //    }
    //}

    public void FillOutCharacterData(string _name, string _color, string[] _actions, float _level)
    {
        CharacterScript currCharScript = m_currCharacter.GetComponent<CharacterScript>();
        currCharScript.m_name = _name;
        currCharScript.m_color = _color;
        currCharScript.m_actions = _actions;
        currCharScript.m_level = _level;
    }

    public void Select()
    {
        CharacterScript cScript = m_currCharacter.GetComponent<CharacterScript>();
        PlayerPrefScript.SaveChar(m_currButton.name, cScript);
        SetCharSlot(m_currButton, cScript.m_name, cScript.m_color);

        m_currButton.onClick = new Button.ButtonClickedEvent();
        m_currButton.onClick.AddListener(() => m_panels[(int)menuPans.CHAR_VIEW].PopulatePanel());

        PanelScript.CloseHistory();
    }

    // In order to make this function button friendly, I made it require setting m_currButton beforehand in order to work properly
    public void Remove()
    {
        // Change color of button back
        m_currButton.GetComponent<Image>().color = new Color(.6f, .6f, .6f, 1);

        // Reset energy
        ButtonScript buttScript = m_currButton.GetComponent<ButtonScript>();
        for (int k = 0; k < buttScript.m_energyPanel.Length; k++)
            buttScript.m_energyPanel[k].SetActive(false);

        // Close panel
        PanelScript charViewPan = m_panels[(int)menuPans.CHAR_VIEW];
        charViewPan.m_inView = false;

        // Change text back
        Text t = m_currButton.GetComponentInChildren<Text>();
        t.text = "EMPTY";

        // Remove character from playerprefs
        string key = m_currButton.name + ",actions";
        PlayerPrefs.DeleteKey(key);
        key = m_currButton.name + ",name";
        PlayerPrefs.DeleteKey(key);
        key = m_currButton.name + ",stats";
        PlayerPrefs.DeleteKey(key);

        m_currButton.onClick = new Button.ButtonClickedEvent();
        m_currButton.onClick.AddListener(() => CharacterAssignment());
    }

    public void Load()
    {
        m_saveButton = EventSystem.current.currentSelectedGameObject.GetComponent<Button>();

        if (PlayerPrefs.GetString(m_saveButton.name + "SAVE" + ",name").Length == 0)
            return;

        FillOutCharacterData(PlayerPrefs.GetString(m_saveButton.name + "SAVE" + ",name"), PlayerPrefs.GetString(m_saveButton.name + "SAVE" + ",color"),
            PlayerPrefs.GetString(m_saveButton.name + "SAVE" + ",actions").Split(';'), float.Parse(PlayerPrefs.GetString(m_saveButton.name + "SAVE" + ",level")));

        Select();
        m_panels[(int)menuPans.SAVE_LOAD_PANEL].m_inView = false;
    }

    public void Save()
    {
        CharacterScript cScript = m_currCharacter.GetComponent<CharacterScript>();

        PlayerPrefScript.SaveChar(m_saveButton.name + "SAVE", cScript);
        SetCharSlot(m_saveButton, cScript.m_name, cScript.m_color);

        PanelScript.CloseHistory();
    }

    public void RandomTeam()
    {
        PanelScript panScript = EventSystem.current.currentSelectedGameObject.GetComponent<Button>().transform.parent.GetComponent<PanelScript>();
        Button[] buttons = panScript.m_buttons;
        for (int i = 0; i < 6; i++)
        {
            m_currButton = buttons[i];
            RandomCharacter();
        }

        // Let the user decide how many random characters to add
        // What level the the characters will be
        // What colors
    }

    public void ClearTeam()
    {
        Button button = EventSystem.current.currentSelectedGameObject.GetComponent<Button>();
        PanelScript panScript = m_panels[(int)menuPans.CHAR_SLOTS].m_panels[int.Parse(button.transform.parent.name)].GetComponent<PanelScript>();
        Button[] team = panScript.m_buttons;
        for (int i = 0; i < 6; i++)
        {
            m_currButton = team[i];
            Remove();
        }
    }

    public void StartGame()
    {
        Application.LoadLevel("Scene1");
    }
}
