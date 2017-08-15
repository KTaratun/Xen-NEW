using System.Collections;
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
                    team[j].onClick.AddListener(() => PopulateCharacterViewer());
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

        m_panels[(int)menuPans.CHAR_PANEL].m_inView = true;
        m_panels[(int)menuPans.CHAR_PANEL].SetButtons();
        m_currButton = EventSystem.current.currentSelectedGameObject.GetComponent<Button>();
    }

    public void NewCharacter()
    {
        if (!m_panels[(int)menuPans.CHAR_PANEL].m_inView)
            return;
    }

    public void LoadCharacter()
    {
        PanelScript panScript = m_panels[(int)menuPans.SAVE_LOAD_PANEL];

        m_panels[(int)menuPans.CHAR_PANEL].m_inView = false;

        if (EventSystem.current.currentSelectedGameObject.GetComponent<Button>().name == "Save Button")
            panScript.m_text[0].text = "SAVE";
        else if (EventSystem.current.currentSelectedGameObject.GetComponent<Button>().name == "Load")
            panScript.m_text[0].text = "LOAD";

        panScript.m_inView = true;

        if (m_panels[(int)menuPans.CHAR_VIEW].m_inView)
        {
            m_panels[(int)menuPans.CHAR_VIEW].m_inView = false;
            panScript.m_parent = m_panels[(int)menuPans.CHAR_VIEW].gameObject;
        }
        else
            panScript.m_parent = null;

        Button[] saveSlots = panScript.m_buttons;
        for (int i = 0; i < 9; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                string key = i.ToString() + ',' + j.ToString() + "SAVE" + ",name";
                string name = PlayerPrefs.GetString(key);
                if (name.Length > 0)
                {
                    key = i.ToString() + ',' + j.ToString() + "SAVE" + ",color";
                    string color = PlayerPrefs.GetString(key);

                    SetCharSlot(saveSlots[4 * i + j], name, color);
                }
            }
        }
    }

    public void PresetCharacter()
    {
        if (!m_panels[(int)menuPans.CHAR_PANEL].m_inView)
            return;

        PanelScript presetSelectScript = m_panels[(int)menuPans.PRESELECT_PANEL];
        presetSelectScript.m_inView = true;
        m_panels[(int)menuPans.CHAR_PANEL].m_inView = false;

        DatabaseScript dbScript = gameObject.GetComponent<DatabaseScript>();

        for (int i = 0; i < presetSelectScript.m_buttons.Length; i++)
        {
            Button butt = presetSelectScript.m_buttons[i];
            Text t = butt.GetComponentInChildren<Text>();
            t.text = dbScript.GetDataValue(dbScript.m_presets[i], "Name:");
            butt.name = i.ToString();
            butt.onClick.RemoveAllListeners();
            butt.onClick.AddListener(() => PopulateCharacterViewer());
            ButtonScript buttScript = butt.GetComponent<ButtonScript>();
            buttScript.SetTotalEnergy(dbScript.GetDataValue(dbScript.m_presets[i], "Colors:"));
        }
    }

    public void RandomCharacter()
    {
        if (!m_panels[(int)menuPans.CHAR_PANEL].m_inView)
            return;

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
                min = 3;
                max = 6;
            }
            else if (i == 2)
            {
                min = 6;
                max = 10;
            }
            else if (i == 3) // New characters can have two -1 actions
            {
                min = 6;
                max = 14;
            }

            int randAct;
            do
            {
                randAct = Random.Range(min, max);
            } while (randAct == prevAct);

            prevAct = randAct;
            cScript.m_actions[i] = dbScript.m_actions[randAct + color * 16];
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

    public void PopulateCharacterViewer()
    {
        // If another panel is open, don't open character viewer for already loaded character
        Button currB = EventSystem.current.currentSelectedGameObject.GetComponent<Button>();
        int res = 0;

        m_panels[(int)menuPans.PRESELECT_PANEL].m_inView = false;

        if (CheckIfPanelOpen())
            return;

        PanelScript charViewScript = m_panels[(int)menuPans.CHAR_VIEW];
        charViewScript.m_inView = true;

        Button[] buttons = charViewScript.m_buttons;

        PanelScript actionScript = charViewScript.m_panels[0].GetComponent<PanelScript>();
        PanelScript statPan = charViewScript.m_panels[1].GetComponent<PanelScript>();

        CharacterScript currCharScript = m_currCharacter.GetComponent<CharacterScript>();
        actionScript.m_cScript = currCharScript;

        res = 0;
        if (int.TryParse(currB.name, out res)) // If last pressed button is an int, it's a preset character panel button. Character slot buttons names are in the x,x format
        {
            charViewScript.m_parent = m_panels[(int)menuPans.PRESELECT_PANEL].gameObject;
            DatabaseScript dbScript = gameObject.GetComponent<DatabaseScript>();

            string[] presetDataSeparated = dbScript.m_presets[int.Parse(currB.name)].Split('|');

            // Fill out energy
            string[] presetColor = presetDataSeparated[(int)DatabaseScript.presets.COLORS].Split(':');
            ButtonScript buttScript = buttons[0].GetComponent<ButtonScript>(); // button[0] == energy
            buttons[0].name = presetColor[1];
            buttScript.SetTotalEnergy(presetColor[1]);

            // Fill out name
            string[] presetName = presetDataSeparated[(int)DatabaseScript.presets.NAME].Split(':');
            charViewScript.m_text[1].text = presetName[1];

            FillOutCharacterData(presetName[1], presetColor[1], dbScript.GetActions(dbScript.m_presets[int.Parse(currB.name)]));

            // Fill out status
            statPan.m_cScript = currCharScript;
            statPan.PopulatePanel();

            // Fill out actions
            actionScript.PopulatePanel();

            // Determine if select or remove will be visible
            for (int i = 0; i < buttons.Length; i++)
            {
                if (buttons[i].name == "Select Button" && buttons[i].gameObject.transform.position.x > 1000)
                    buttons[i].gameObject.transform.SetPositionAndRotation(new Vector3(buttons[i].gameObject.transform.position.x - 1000, buttons[i].gameObject.transform.position.y, buttons[i].gameObject.transform.position.z), buttons[i].gameObject.transform.rotation);
                if (buttons[i].name == "Remove Button" && buttons[i].gameObject.transform.position.x < 1000)
                    buttons[i].gameObject.transform.SetPositionAndRotation(new Vector3(buttons[i].gameObject.transform.position.x + 1000, buttons[i].gameObject.transform.position.y, buttons[i].gameObject.transform.position.z), buttons[i].gameObject.transform.rotation);
            }
        }
        else
        {
            m_currButton = currB;
            charViewScript.m_parent = null;
            ButtonScript buttScript = buttons[0].GetComponent<ButtonScript>(); // button[0] == energy

            string name = PlayerPrefs.GetString(currB.name + ",name");
            string color = PlayerPrefs.GetString(currB.name + ",color");
            string[] actions = PlayerPrefs.GetString(currB.name + ",actions").Split(';');

            // Fill out name
            charViewScript.m_text[1].text = name;
            // Fill out energy
            buttScript.SetTotalEnergy(color);
            // Fill out actions
            currCharScript.m_actions = actions;

            FillOutCharacterData(name, color, actions);

            actionScript.PopulatePanel();

            // Determine if select or remove will be visible
            for (int i = 0; i < buttons.Length; i++)
            {
                if (buttons[i].name == "Select Button" && buttons[i].gameObject.transform.position.x < 1000)
                    buttons[i].gameObject.transform.SetPositionAndRotation(new Vector3(buttons[i].gameObject.transform.position.x + 1000, buttons[i].gameObject.transform.position.y, buttons[i].gameObject.transform.position.z), buttons[i].gameObject.transform.rotation);
                if (buttons[i].name == "Remove Button" && buttons[i].gameObject.transform.position.x > 1000)
                    buttons[i].gameObject.transform.SetPositionAndRotation(new Vector3(buttons[i].gameObject.transform.position.x - 1000, buttons[i].gameObject.transform.position.y, buttons[i].gameObject.transform.position.z), buttons[i].gameObject.transform.rotation);
            }
        }
    }

    public void FillOutCharacterData(string _name, string _color, string[] _actions)
    {
        CharacterScript currCharScript = m_currCharacter.GetComponent<CharacterScript>();
        currCharScript.m_name = _name;
        currCharScript.m_color = _color;
        currCharScript.m_actions = _actions;
    }

    public void Select()
    {
        PanelScript charViewScript = m_panels[(int)menuPans.CHAR_VIEW];
        charViewScript.m_inView = false;

        CharacterScript cScript = m_currCharacter.GetComponent<CharacterScript>();

        // Save out pertinent character data
        string key = m_currButton.name + ",actions";
        string combo = string.Join(";", cScript.m_actions);
        PlayerPrefs.SetString(key, combo);
        key = m_currButton.name + ",name";
        PlayerPrefs.SetString(key, cScript.m_name);
        key = m_currButton.name + ",color";
        PlayerPrefs.SetString(key, cScript.m_color);

        SetCharSlot(m_currButton, cScript.m_name, cScript.m_color);
        m_currButton.onClick = new Button.ButtonClickedEvent();
        m_currButton.onClick.AddListener(() => PopulateCharacterViewer());

        // Write out stats
        //key = m_currButton.name + ",stats";
        //for (int i = 0; i < cScript.m_stats.Length; i++)
        //    stats += "," + cScript.m_stats[i];
        //PlayerPrefs.SetString(key, stats);
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

    public void SaveLoadButtonPress()
    {
        m_saveButton = EventSystem.current.currentSelectedGameObject.GetComponent<Button>();

        if (PlayerPrefs.GetString(m_saveButton.name + "SAVE" + ",name").Length == 0)
            return;

        if (m_panels[(int)menuPans.SAVE_LOAD_PANEL].m_text[0].text == "SAVE")
        {
            m_panels[(int)menuPans.CONFIRMATION_PANEL].m_inView = true;
            m_panels[(int)menuPans.CONFIRMATION_PANEL].m_buttons[1].onClick = new Button.ButtonClickedEvent();
            m_panels[(int)menuPans.CONFIRMATION_PANEL].m_buttons[1].onClick.AddListener(() => Save());
        }
        else if (m_panels[(int)menuPans.SAVE_LOAD_PANEL].m_text[0].text == "LOAD")
        {
            FillOutCharacterData(PlayerPrefs.GetString(m_saveButton.name + "SAVE" + ",name"), PlayerPrefs.GetString(m_saveButton.name + "SAVE" + ",color"),
                PlayerPrefs.GetString(m_saveButton.name + "SAVE" + ",actions").Split(';'));
            Select();
            m_panels[(int)menuPans.SAVE_LOAD_PANEL].m_inView = false;
        }

    }

    public void Save()
    {
        CharacterScript cScript = m_currCharacter.GetComponent<CharacterScript>();

        // Save out pertinent character data
        string key = m_saveButton.name + "SAVE" + ",actions";
        string combo = string.Join(";", cScript.m_actions);
        PlayerPrefs.SetString(key, combo);
        key = m_saveButton.name + "SAVE" + ",name";
        PlayerPrefs.SetString(key, cScript.m_name);
        key = m_saveButton.name + "SAVE" + ",color";
        PlayerPrefs.SetString(key, cScript.m_color);

        SetCharSlot(m_saveButton, cScript.m_name, cScript.m_color);

        m_panels[(int)menuPans.CONFIRMATION_PANEL].m_inView = false;
        m_panels[(int)menuPans.SAVE_LOAD_PANEL].m_inView = false;
    }

    public void RandomTeam()
    {
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
