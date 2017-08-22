using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class TeamMenuScript : MonoBehaviour {

    public enum menuPans { CHAR_SLOTS, CHAR_PANEL, CHAR_VIEW, PRESELECT_PANEL, ACTION_VIEW, SAVE_LOAD_PANEL,
        NEW_ACTION_PANEL, NEW_STATS_PANEL, CONFIRMATION_PANEL, TOT_PANELS }

    public Button m_currButton;
    public Button m_saveButton;
    public Button m_oldButton;
    public Button m_statButton;
    public CharacterScript m_currCharScript;
    public GameObject m_character;

	// Use this for initialization
	void Start ()
    {
        PanelScript.MenuPanelInit("Canvas");
        TeamInit();

        GameObject newChar = Instantiate(m_character);
        m_currCharScript = newChar.GetComponent<CharacterScript>();

        //PlayerPrefs.DeleteAll();
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    public void TeamInit()
    {
        for (int i = 0; i < 4; i++)
        {
            PanelScript panScript = PanelScript.m_allPanels[(int)menuPans.CHAR_SLOTS].m_panels[i].GetComponent<PanelScript>();
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
                    team[j].onClick.AddListener(() => PanelScript.m_allPanels[(int)menuPans.CHAR_VIEW].PopulatePanel());
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
        if (PanelScript.CheckIfPanelOpen())
            return;

        PanelScript.m_allPanels[(int)menuPans.CHAR_PANEL].PopulatePanel();
        m_currButton = EventSystem.current.currentSelectedGameObject.GetComponent<Button>();
    }

    public void NewCharacter()
    {
        if (!PanelScript.m_allPanels[(int)menuPans.CHAR_PANEL].m_inView)
            return;
    }

    public void RandomCharacter()
    {
        PanelScript.m_allPanels[(int)menuPans.CHAR_PANEL].m_inView = false;

        int color = Random.Range(0, 4);

        m_currCharScript.m_name = RandomName();

        if (color == 0)
            m_currCharScript.m_color = "G";
        else if (color == 1)
            m_currCharScript.m_color = "R";
        else if (color == 2)
            m_currCharScript.m_color = "W";
        else if (color == 3)
            m_currCharScript.m_color = "B";

        m_currCharScript.m_actions = new string[4];

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
            m_currCharScript.m_actions[i] = dbScript.m_actions[randAct + color * 16];
        }

        DatabaseScript db = gameObject.GetComponent<DatabaseScript>();

        string[] stat = db.m_stat[Random.Range(0, db.m_stat.Length)].Split('|');

        m_currCharScript.InitializeStats();
        for (int i = 0; i < m_currCharScript.m_stats.Length; i++) // We're not using the 2 rare stats yet
        {
            string[] currStat = stat[i+1].Split(':');
            m_currCharScript.m_stats[i] += int.Parse(currStat[1]);
            m_currCharScript.m_tempStats[i] += int.Parse(currStat[1]);
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

    public void FillOutCharacterData(string _name, string _color, string[] _actions, int _exp, int _level)
    {
        m_currCharScript.m_name = _name;
        m_currCharScript.m_color = _color;
        m_currCharScript.m_actions = _actions;
        m_currCharScript.m_exp = _exp;
        m_currCharScript.m_level = _level;
    }

    public void Select()
    {
        PlayerPrefScript.SaveChar(m_currButton.name, m_currCharScript);
        SetCharSlot(m_currButton, m_currCharScript.m_name, m_currCharScript.m_color);

        m_currButton.onClick = new Button.ButtonClickedEvent();
        m_currButton.onClick.AddListener(() => PanelScript.m_allPanels[(int)menuPans.CHAR_VIEW].PopulatePanel());

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
        PanelScript.CloseHistory();

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
            PlayerPrefs.GetString(m_saveButton.name + "SAVE" + ",actions").Split(';'), int.Parse(PlayerPrefs.GetString(m_saveButton.name + "SAVE" + ",exp")),
            int.Parse(PlayerPrefs.GetString(m_saveButton.name + "SAVE" + ",level")));

        Select();
        PanelScript.m_allPanels[(int)menuPans.SAVE_LOAD_PANEL].m_inView = false;
    }

    public void Save()
    {
        CharacterScript cScript = m_currCharScript;

        PlayerPrefScript.SaveChar(m_saveButton.name + "SAVE", cScript);
        SetCharSlot(m_saveButton, cScript.m_name, cScript.m_color);

        PanelScript.CloseHistory();
    }

    public void LevelUp()
    {
        PanelScript.m_locked = true;
        m_currCharScript.m_exp -= 10;
        m_currCharScript.m_level++;

        for (int i = 0; i < PanelScript.m_allPanels[(int)menuPans.CHAR_VIEW].m_buttons.Length; i++)
        {
            if (PanelScript.m_allPanels[(int)menuPans.CHAR_VIEW].m_buttons[i].GetComponentInChildren<Text>().text == "REMOVE" ||
                PanelScript.m_allPanels[(int)menuPans.CHAR_VIEW].m_buttons[i].GetComponentInChildren<Text>().text == "SAVE" ||
                PanelScript.m_allPanels[(int)menuPans.CHAR_VIEW].m_buttons[i].GetComponentInChildren<Text>().text == "LEVEL UP")
                PanelScript.m_allPanels[(int)menuPans.CHAR_VIEW].m_buttons[i].interactable = false;
        }

        PanelScript.m_allPanels[(int)menuPans.NEW_ACTION_PANEL].PopulatePanel();

        if (m_currCharScript.m_level == 3 || m_currCharScript.m_level == 5)
            PanelScript.m_allPanels[(int)menuPans.NEW_STATS_PANEL].PopulatePanel();

        if (m_currCharScript.m_level < 6)
        {
            m_currCharScript.m_stats[(int)CharacterScript.sts.HP] += 2;
            m_currCharScript.m_tempStats[(int)CharacterScript.sts.HP] += 2;
            PlayerPrefScript.SaveChar(m_currButton.name, m_currCharScript);
            PanelScript.m_allPanels[(int)menuPans.CHAR_VIEW].PopulatePanel();
        }
        else
            PlayerPrefScript.SaveChar(m_currButton.name, m_currCharScript);
    }

    public string NewRandomAction(Button[] _buttons)
    {
        DatabaseScript db = GetComponent<DatabaseScript>();
        string newAct = null;
        bool actOK = true;
        string color = "";

        for (int i = 0; i < _buttons.Length; i++)
            _buttons[i].image.color = Color.white;

        // Copy our normal color in order to temporarily alter it
        for (int i = 0; i < m_currCharScript.m_color.Length; i++)
            color += m_currCharScript.m_color[i];

        // Add all our level 0 colors to our color
        for (int i = 0; i < m_currCharScript.m_actions.Length; i++)
        {
            if (PlayerScript.CheckIfGains(m_currCharScript.m_actions[i]))
            {
                string actEng = DatabaseScript.GetActionData(m_currCharScript.m_actions[i], DatabaseScript.actions.ENERGY);
                bool newEng = false;
                for (int j = 0; j < color.Length; j++)
                {
                    if (actEng[0] == 'g' && color[j] != 'G' || actEng[0] == 'r' && color[j] != 'R' ||
                        actEng[0] == 'w' && color[j] != 'W' || actEng[0] == 'b' && color[j] != 'B')
                        newEng = true;
                }

                if (newEng)
                {
                    if (actEng[0] == 'g')
                        color += 'G';
                    else if (actEng[0] == 'r')
                        color += 'R';
                    else if (actEng[0] == 'w')
                        color += 'W';
                    else if (actEng[0] == 'b')
                        color += 'B';
                }
            }
        }

        do
        {
            actOK = true;
            int roll = Random.Range(0, 10);
            newAct = db.m_actions[Random.Range(0, db.m_actions.Length)];
            string actEng = DatabaseScript.GetActionData(newAct, DatabaseScript.actions.ENERGY);

            // If one of the other new buttons already has this action, skip it
            for (int i = 0; i < _buttons.Length; i++)
                if (newAct == _buttons[i].name)
                    actOK = false;

            // If the character already has this action, skip it
            for (int i = 0; i < m_currCharScript.m_actions.Length; i++)
                if (newAct == m_currCharScript.m_actions[i])
                    actOK = false;

            // If the action is outside your max color range, skip it
            if (m_currCharScript.m_color.Length == 3 && !PlayerScript.CheckIfGains(actEng))
                for (int i = 0; i < actEng.Length; i++)
                {
                    bool withinColors = false;
                    for (int j = 0; j < m_currCharScript.m_color.Length; j++)
                    {
                        if (actEng[i] == m_currCharScript.m_color[j])
                            withinColors = true;

                        if (j == m_currCharScript.m_color.Length - 1 && !withinColors)
                            actOK = false;
                    }
                }

            if (actOK)
            {
                // There is a random chance that the action will just go through even if it's not their color
                actOK = false;
                if (roll == 9 || PlayerScript.CheckIfGains(actEng))
                    actOK = true;
                else
                {
                    for (int i = 0; i < actEng.Length; i++)
                    {
                        actOK = false;
                        for (int j = 0; j < color.Length; j++)
                        {
                            if (actEng[i] == color[j])
                                actOK = true;
                        }
                        if (!actOK)
                            break;
                    }
                }
            }
            

        } while (!actOK);

        return newAct;
    }

    public void NewActionSelection()
    {
        PanelScript actPanScript = PanelScript.m_allPanels[(int)menuPans.CHAR_VIEW].m_panels[0].GetComponent<PanelScript>();
        Button newAct = EventSystem.current.currentSelectedGameObject.GetComponent<Button>();

        if (m_saveButton)
            m_saveButton.image.color = Color.white;
        m_saveButton = newAct;
        m_saveButton.image.color = Color.cyan;

        for (int i = 0; i < actPanScript.m_panels.Length; i++)
        {
            Button[] buttons = actPanScript.m_panels[i].GetComponentsInChildren<Button>();
            if (PlayerScript.CheckIfGains(DatabaseScript.GetActionData(newAct.name, DatabaseScript.actions.ENERGY)) && i == 0 ||
                !PlayerScript.CheckIfGains(DatabaseScript.GetActionData(newAct.name, DatabaseScript.actions.ENERGY)) &&
                DatabaseScript.GetActionData(newAct.name, DatabaseScript.actions.ENERGY).Length == 1 && i == 1 ||
                !PlayerScript.CheckIfGains(DatabaseScript.GetActionData(newAct.name, DatabaseScript.actions.ENERGY)) &&
                DatabaseScript.GetActionData(newAct.name, DatabaseScript.actions.ENERGY).Length == 2 && i == 2 ||
                !PlayerScript.CheckIfGains(DatabaseScript.GetActionData(newAct.name, DatabaseScript.actions.ENERGY)) &&
                DatabaseScript.GetActionData(newAct.name, DatabaseScript.actions.ENERGY).Length == 3 && i == 3)
            {
                ColorActionButtons(buttons, true);
            }
            else
                ColorActionButtons(buttons, false);
        }
    }

    private void ColorActionButtons(Button[] _buttons, bool _color)
    {
        if (_color)
        {
            for (int j = 0; j < _buttons.Length; j++)
            {
                if (m_currCharScript.m_actions.Length >= 8 && _buttons[j].GetComponentInChildren<Text>().text == "EMPTY")
                    continue;

                _buttons[j].interactable = true;
                _buttons[j].image.color = Color.cyan;
                _buttons[j].onClick.RemoveAllListeners();
                Button[] conButtons = PanelScript.m_confirmPanel.m_buttons;
                _buttons[j].onClick.AddListener(() => conButtons[1].GetComponent<ButtonScript>().ConfirmationButton("New Action"));
            }
        }
        else
        {
            for (int j = 0; j < _buttons.Length; j++)
            {
                _buttons[j].interactable = false;
                _buttons[j].image.color = Color.white;
                _buttons[j].onClick.RemoveAllListeners();
            }
        }
    }

    public void ReplaceAction()
    {
        string oldActString = m_oldButton.GetComponentInChildren<Text>().text;

        if (oldActString == "EMPTY")
        {
            string[] newActions = new string[m_currCharScript.m_actions.Length + 1];

            for (int i = 0; i < newActions.Length; i++)
            {
                if (i != m_currCharScript.m_actions.Length)
                    newActions[i] = m_currCharScript.m_actions[i];
                else
                    newActions[i] = m_saveButton.name;
            }

            m_currCharScript.m_actions = newActions;
        }
        else
        {
            for (int i = 0; i < m_currCharScript.m_actions.Length; i++)
            {
                if (oldActString == DatabaseScript.GetActionData(m_currCharScript.m_actions[i], DatabaseScript.actions.NAME))
                    m_currCharScript.m_actions[i] = m_saveButton.name;
            }
        }

        m_currCharScript.m_color = PlayerScript.CheckCharColors(m_currCharScript.m_actions);
        SetCharSlot(m_currButton, m_currCharScript.m_name, m_currCharScript.m_color);
        PlayerPrefScript.SaveChar(m_currButton.name, m_currCharScript);

        CloseLevelPanel(menuPans.NEW_ACTION_PANEL);
    }

    public void CloseLevelPanel(menuPans _pan)
    {
        EventSystem.current.currentSelectedGameObject.GetComponent<Button>().name = m_currButton.name;
        if (_pan == menuPans.NEW_ACTION_PANEL)
        {
            PanelScript.m_allPanels[(int)menuPans.CHAR_VIEW].m_panels[0].GetComponent<PanelScript>().PopulatePanel();
            m_saveButton = null;
        }
        else if (_pan == menuPans.NEW_STATS_PANEL)
            PanelScript.m_allPanels[(int)menuPans.CHAR_VIEW].m_panels[1].GetComponent<PanelScript>().PopulatePanel();

        PanelScript.m_allPanels[(int)_pan].m_inView = false;
        PanelScript.RemoveFromHistory("");

        if (!PanelScript.m_allPanels[(int)menuPans.NEW_ACTION_PANEL].m_inView && !PanelScript.m_allPanels[(int)menuPans.NEW_STATS_PANEL].m_inView)
        {
            for (int i = 0; i < PanelScript.m_allPanels[(int)menuPans.CHAR_VIEW].m_buttons.Length; i++)
            {
                if (PanelScript.m_allPanels[(int)menuPans.CHAR_VIEW].m_buttons[i].GetComponentInChildren<Text>().text == "REMOVE" ||
                    PanelScript.m_allPanels[(int)menuPans.CHAR_VIEW].m_buttons[i].GetComponentInChildren<Text>().text == "SAVE" ||
                    PanelScript.m_allPanels[(int)menuPans.CHAR_VIEW].m_buttons[i].GetComponentInChildren<Text>().text == "LEVEL UP" && m_currCharScript.m_exp >= 10)
                    PanelScript.m_allPanels[(int)menuPans.CHAR_VIEW].m_buttons[i].interactable = true;
            }

            PanelScript.m_locked = false;
        }
    }

    public void NewRandomStat(Button[] _buttons)
    {
        DatabaseScript db = GetComponent<DatabaseScript>();

        // Do this to ensure we don't throw away choices from the last time the panel was opened.
        for (int i = 0; i < _buttons.Length - 1; i++)
        {
            _buttons[i].image.color = Color.white;
            _buttons[i].GetComponentInChildren<Text>().text = "EMPTY";
        }

        for (int i = 0; i < _buttons.Length - 1; i++) // -1 just so we get only the action buttons
        {
            bool actOK = true;
            string statAlt = "";
            string buff = "";
            string debuff = "";

            do
            {
                statAlt = db.m_stat[Random.Range(0, db.m_stat.Length)];
                string[] stat = statAlt.Split('|');

                actOK = true;

                for (int j = 0; j < m_currCharScript.m_stats.Length; j++)
                {
                    string[] s = stat[j + 1].Split(':');
                    string value = ((CharacterScript.sts)j).ToString();
                    if (int.Parse(s[1]) > 0)
                        buff = value + "-";
                    else if (int.Parse(s[1]) < 0)
                        debuff = value;
                }

                for (int j = 0; j < _buttons.Length; j++)
                {
                    if (_buttons[j].GetComponentInChildren<Text>().text == buff + debuff)
                        actOK = false;
                    break;
                }

            } while (!actOK);

            Text t = _buttons[i].GetComponentInChildren<Text>();
            _buttons[i].GetComponentInChildren<Text>().text = buff + debuff;
            _buttons[i].name = statAlt;

            _buttons[i].onClick.RemoveAllListeners();
            Button[] conButtons = PanelScript.m_confirmPanel.m_buttons;
            _buttons[i].onClick.AddListener(() => conButtons[1].GetComponent<ButtonScript>().ConfirmationButton("New Stats"));
        }
    }

    public void AddStatAlteration()
    {
        PanelScript statPanScript = PanelScript.m_allPanels[(int)menuPans.CHAR_VIEW].m_panels[1].GetComponent<PanelScript>();
        string[] statSeparated = m_statButton.name.Split('|');

        for (int i = 0; i < 9; i++)
        {
            string[] s = statSeparated[i + 1].Split(':');
            m_currCharScript.m_stats[i] += int.Parse(s[1]);
            m_currCharScript.m_tempStats[i] += int.Parse(s[1]);
            statPanScript.m_text[i].color = Color.black;
        }

        PlayerPrefScript.SaveChar(m_currButton.name, m_currCharScript);
        CloseLevelPanel(menuPans.NEW_STATS_PANEL);
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
        PanelScript panScript = PanelScript.m_allPanels[(int)menuPans.CHAR_SLOTS].m_panels[int.Parse(button.transform.parent.name)].GetComponent<PanelScript>();
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
