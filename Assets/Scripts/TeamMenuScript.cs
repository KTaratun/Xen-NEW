using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class TeamMenuScript : MonoBehaviour {

    public Button m_currButton;
    public Button m_saveButton;
    public Button m_oldButton;
    public Button m_statButton;
    public CharacterScript m_currCharScript;
    public GameObject m_character;
    public AudioSource m_audio;

	// Use this for initialization
	void Start ()
    {
        m_audio = gameObject.AddComponent<AudioSource>();

        PanelScript.MenuPanelInit("Canvas", gameObject);
        TeamInit();

        GameObject newChar = Instantiate(m_character);
        newChar.transform.SetPositionAndRotation(new Vector3(2000, 2000, 2000), Quaternion.identity);
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
            PanelScript panScript = PanelScript.GetPanel("CharacterSlots").m_panels[i];
            Button[] team = panScript.m_buttons;
            for (int j = 0; j < 6; j++)
            {
                string key = i.ToString() + ',' + j.ToString() + ",name";
                string name = PlayerPrefs.GetString(key);
                if (name.Length > 0)
                {
                    key = i.ToString() + ',' + j.ToString() + ",color";
                    string color = PlayerPrefs.GetString(key);

                    SetCharSlot(team[j], name);
                    team[j].onClick = new Button.ButtonClickedEvent();
                    team[j].onClick.AddListener(() => PanelScript.GetPanel("CharacterViewer Panel").PopulatePanel());
                }
            }
        }
    }

    public void SetCharSlot(Button _button, string _name)
    {
        Text t = _button.GetComponentInChildren<Text>();
        t.text = _name;

        ButtonScript buttScript = _button.GetComponent<ButtonScript>();

        _button.GetComponent<Image>().color = new Color(.85f, .85f, .85f, 1);
    }

    public void CharacterAssignment()
    {
        if (PanelScript.CheckIfPanelOpen())
            return;

        PanelScript.GetPanel("Character Panel").PopulatePanel();
        m_currButton = EventSystem.current.currentSelectedGameObject.GetComponent<Button>();
    }

    public void NewCharacter()
    {
    }

    public void RandomCharacter(int _lvl)
    {
        if (!m_audio.isPlaying)
            m_audio.PlayOneShot(Resources.Load<AudioClip>("Sounds/Recruitment Sound 2"));

        m_currCharScript.m_name = RandomName();

        int numActions = 3;
        int statAlts = _lvl;
        int hPBonus = _lvl * 2;

        m_currCharScript.m_actions = new string[numActions];

        DatabaseScript dbScript = GameObject.Find("Database").GetComponent<DatabaseScript>();

        string newAct = "";
        for (int i = 0; i < 2; i++)
        {
            newAct = dbScript.m_actions[0];
            m_currCharScript.m_actions[i] = newAct;
        }

        m_currCharScript.m_actions[2] = dbScript.m_actions[1];

        m_currCharScript.InitializeStats();

        m_currCharScript.m_stats[(int)CharacterScript.sts.HP] = 10 + hPBonus;
        m_currCharScript.m_tempStats[(int)CharacterScript.sts.HP] = m_currCharScript.m_stats[(int)CharacterScript.sts.HP];

        StatAlterations(m_currCharScript);

        m_currCharScript.m_level = 1 + ((_lvl - 1) * 2);

        Select();

        // Choose the number of random characters
        // Choose level range
        // Choose color restraints
    }

    private void StatAlterations(CharacterScript _charScript)
    {
        int num = Random.Range(0, 15);

        switch (num)
        {
            case 0:
                _charScript.m_stats[(int)CharacterScript.sts.HP] += 2;
                _charScript.m_tempStats[(int)CharacterScript.sts.HP] += 2;
                _charScript.m_stats[(int)CharacterScript.sts.SPD] -= 2;
                _charScript.m_tempStats[(int)CharacterScript.sts.SPD] -= 2;
                break;
            case 1:
                _charScript.m_stats[(int)CharacterScript.sts.HP] += 3;
                _charScript.m_tempStats[(int)CharacterScript.sts.HP] += 3;
                _charScript.m_stats[(int)CharacterScript.sts.DMG] -= 1;
                _charScript.m_tempStats[(int)CharacterScript.sts.DMG] -= 1;
                break;
            case 2:
                _charScript.m_stats[(int)CharacterScript.sts.HP] += 3;
                _charScript.m_tempStats[(int)CharacterScript.sts.HP] += 3;
                _charScript.m_stats[(int)CharacterScript.sts.MOV] -= 1;
                _charScript.m_tempStats[(int)CharacterScript.sts.MOV] -= 1;
                break;
            case 3:
                _charScript.m_stats[(int)CharacterScript.sts.SPD] += 2;
                _charScript.m_tempStats[(int)CharacterScript.sts.SPD] += 2;
                _charScript.m_stats[(int)CharacterScript.sts.HP] -= 2;
                _charScript.m_tempStats[(int)CharacterScript.sts.HP] -= 2;
                break;
            case 4:
                _charScript.m_stats[(int)CharacterScript.sts.SPD] += 3;
                _charScript.m_tempStats[(int)CharacterScript.sts.SPD] += 3;
                _charScript.m_stats[(int)CharacterScript.sts.DMG] -= 1;
                _charScript.m_tempStats[(int)CharacterScript.sts.DMG] -= 1;
                break;
            case 5:
                _charScript.m_stats[(int)CharacterScript.sts.SPD] += 3;
                _charScript.m_tempStats[(int)CharacterScript.sts.SPD] += 3;
                _charScript.m_stats[(int)CharacterScript.sts.MOV] -= 1;
                _charScript.m_tempStats[(int)CharacterScript.sts.MOV] -= 1;
                break;
            case 6:
                _charScript.m_stats[(int)CharacterScript.sts.DMG] += 1;
                _charScript.m_tempStats[(int)CharacterScript.sts.DMG] += 1;
                _charScript.m_stats[(int)CharacterScript.sts.HP] -= 3;
                _charScript.m_tempStats[(int)CharacterScript.sts.HP] -= 3;
                break;
            case 7:
                _charScript.m_stats[(int)CharacterScript.sts.DMG] += 1;
                _charScript.m_tempStats[(int)CharacterScript.sts.DMG] += 1;
                _charScript.m_stats[(int)CharacterScript.sts.SPD] -= 3;
                _charScript.m_tempStats[(int)CharacterScript.sts.SPD] -= 3;
                break;
            case 8:
                _charScript.m_stats[(int)CharacterScript.sts.DMG] += 1;
                _charScript.m_tempStats[(int)CharacterScript.sts.DMG] += 1;
                _charScript.m_stats[(int)CharacterScript.sts.MOV] -= 1;
                _charScript.m_tempStats[(int)CharacterScript.sts.MOV] -= 1;
                break;
            case 9:
                _charScript.m_stats[(int)CharacterScript.sts.MOV] += 1;
                _charScript.m_tempStats[(int)CharacterScript.sts.MOV] += 1;
                _charScript.m_stats[(int)CharacterScript.sts.HP] -= 3;
                _charScript.m_tempStats[(int)CharacterScript.sts.HP] -= -3;
                break;
            case 10:
                _charScript.m_stats[(int)CharacterScript.sts.MOV] += 1;
                _charScript.m_tempStats[(int)CharacterScript.sts.MOV] += 1;
                _charScript.m_stats[(int)CharacterScript.sts.SPD] -= 3;
                _charScript.m_tempStats[(int)CharacterScript.sts.SPD] -= 3;
                break;
            case 11:
                _charScript.m_stats[(int)CharacterScript.sts.MOV] += 1;
                _charScript.m_tempStats[(int)CharacterScript.sts.MOV] += 1;
                _charScript.m_stats[(int)CharacterScript.sts.DMG] -= 1;
                _charScript.m_tempStats[(int)CharacterScript.sts.DMG] -= 1;
                break;
            case 12:
                _charScript.m_stats[(int)CharacterScript.sts.RNG] += 1;
                _charScript.m_tempStats[(int)CharacterScript.sts.RNG] += 1;
                _charScript.m_stats[(int)CharacterScript.sts.HP] -= 3;
                _charScript.m_tempStats[(int)CharacterScript.sts.HP] -= -3;
                break;
            case 13:
                _charScript.m_stats[(int)CharacterScript.sts.RNG] += 1;
                _charScript.m_tempStats[(int)CharacterScript.sts.RNG] += 1;
                _charScript.m_stats[(int)CharacterScript.sts.SPD] -= 3;
                _charScript.m_tempStats[(int)CharacterScript.sts.SPD] -= 3;
                break;
            case 14:
                _charScript.m_stats[(int)CharacterScript.sts.RNG] += 1;
                _charScript.m_tempStats[(int)CharacterScript.sts.RNG] += 1;
                _charScript.m_stats[(int)CharacterScript.sts.DMG] -= 1;
                _charScript.m_tempStats[(int)CharacterScript.sts.DMG] -= 1;
                break;
            case 15:
                _charScript.m_stats[(int)CharacterScript.sts.RNG] += 1;
                _charScript.m_tempStats[(int)CharacterScript.sts.RNG] += 1;
                _charScript.m_stats[(int)CharacterScript.sts.MOV] -= 1;
                _charScript.m_tempStats[(int)CharacterScript.sts.MOV] -= 1;
                break;
            default:
                break;
        }
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

    public void FillOutCharacterData(string _name, string[] _actions, string _stats, int _exp, int _level, int _gender)
    {
        m_currCharScript.m_name = _name;
        m_currCharScript.m_actions = _actions;
        m_currCharScript.m_gender = _gender;

        // Load in stats

        m_currCharScript.InitializeStats();
        m_currCharScript.m_tempStats = m_currCharScript.m_stats;

        if (_stats.Length > 0)
        {
            string[] stats = _stats.Split(',');
            for (int i = 0; i < stats.Length; i++)
            {
                m_currCharScript.m_stats[i] = int.Parse(stats[i]);
                m_currCharScript.m_tempStats[i] = int.Parse(stats[i]);
            }
        }

        m_currCharScript.m_exp = _exp;
        m_currCharScript.m_level = _level;
    }

    public void Select()
    {
        PlayerPrefScript.SaveChar(m_currButton.name, m_currCharScript);
        SetCharSlot(m_currButton, m_currCharScript.m_name);

        m_currButton.onClick = new Button.ButtonClickedEvent();
        m_currButton.onClick.AddListener(() => PanelScript.GetPanel("CharacterViewer Panel").PopulatePanel());

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

        if (!m_audio.isPlaying)
            m_audio.PlayOneShot(Resources.Load<AudioClip>("Sounds/Remove Character Sound 1 LOW"));
    }

    public void Load()
    {
        m_saveButton = EventSystem.current.currentSelectedGameObject.GetComponent<Button>();

        if (PlayerPrefs.GetString(m_saveButton.name + "SAVE" + ",name").Length == 0)
            return;

        FillOutCharacterData(PlayerPrefs.GetString(m_saveButton.name + "SAVE" + ",name"), PlayerPrefs.GetString(m_saveButton.name + "SAVE" + ",actions").Split(';'), 
            PlayerPrefs.GetString(m_saveButton.name + "SAVE" + ",stats"), int.Parse(PlayerPrefs.GetString(m_saveButton.name + "SAVE" + ",exp")), 
            int.Parse(PlayerPrefs.GetString(m_saveButton.name + "SAVE" + ",level")), int.Parse(PlayerPrefs.GetString(m_saveButton.name + "SAVE" + ",gender")));

        Select();
        PanelScript.GetPanel("Save/Load Panel").ClosePanel();
    }

    public void Save()
    {
        CharacterScript cScript = m_currCharScript;

        PlayerPrefScript.SaveChar(m_saveButton.name + "SAVE", cScript);
        SetCharSlot(m_saveButton, cScript.m_name);

        PanelScript.CloseHistory();
    }

    public void LevelUp()
    {
        PanelScript.m_locked = true;
        m_currCharScript.m_exp -= 10;
        m_currCharScript.m_level++;

        // Disable all buttons while leveling up
        for (int i = 0; i < PanelScript.GetPanel("CharacterViewer Panel").m_buttons.Length; i++)
        {
            if (PanelScript.GetPanel("CharacterViewer Panel").m_buttons[i].GetComponentInChildren<Text>())
                if (PanelScript.GetPanel("CharacterViewer Panel").m_buttons[i].GetComponentInChildren<Text>().text == "REMOVE" ||
                    PanelScript.GetPanel("CharacterViewer Panel").m_buttons[i].GetComponentInChildren<Text>().text == "SAVE" ||
                    PanelScript.GetPanel("CharacterViewer Panel").m_buttons[i].GetComponentInChildren<Text>().text == "LEVEL UP")
                    PanelScript.GetPanel("CharacterViewer Panel").m_buttons[i].interactable = false;
        }

        PanelScript.GetPanel("New Action Panel").PopulatePanel();

        if (m_currCharScript.m_level == 3 || m_currCharScript.m_level == 5)
            PanelScript.GetPanel("New Status Panel").PopulatePanel();

        if (m_currCharScript.m_level < 6)
        {
            m_currCharScript.m_stats[(int)CharacterScript.sts.HP] += 2;
            m_currCharScript.m_tempStats[(int)CharacterScript.sts.HP] += 2;
            PlayerPrefScript.SaveChar(m_currButton.name, m_currCharScript);
            PanelScript.GetPanel("CharacterViewer Panel").PopulatePanel();
        }
        else
            PlayerPrefScript.SaveChar(m_currButton.name, m_currCharScript);

        m_audio.volume = 0.7f;
        m_audio.PlayOneShot(Resources.Load<AudioClip>("Sounds/Save Game Sound 1"));
    }

    public void NewActionSelection()
    {
        PanelScript actPanScript = PanelScript.GetPanel("CharacterViewer Panel").m_panels[0].GetComponent<PanelScript>();
        Button newAct = EventSystem.current.currentSelectedGameObject.GetComponent<Button>();
        string action = newAct.GetComponent<ButtonScript>().m_action;

        if (m_saveButton)
            m_saveButton.image.color = Color.white;
        m_saveButton = newAct;
        m_saveButton.image.color = Color.cyan;

        for (int i = 0; i < actPanScript.m_panels.Length; i++)
        {
            Button[] buttons = actPanScript.m_panels[i].GetComponentsInChildren<Button>();
            ColorActionButtons(buttons, false);
        }
    }

    private void ColorActionButtons(Button[] _buttons, bool _color)
    {
        if (_color)
        {
            for (int j = 0; j < _buttons.Length; j++)
            {
                if (m_currCharScript.m_actions.Length >= 6 && _buttons[j].GetComponentInChildren<Text>().text == "EMPTY")
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
                    newActions[i] = m_saveButton.GetComponent<ButtonScript>().m_action;
            }

            m_currCharScript.m_actions = newActions;
        }
        else
        {
            for (int i = 0; i < m_currCharScript.m_actions.Length; i++)
            {
                if (oldActString == DatabaseScript.GetActionData(m_currCharScript.m_actions[i], DatabaseScript.actions.NAME))
                    m_currCharScript.m_actions[i] = m_saveButton.GetComponent<ButtonScript>().m_action;
            }
        }

        SetCharSlot(m_currButton, m_currCharScript.m_name);
        PlayerPrefScript.SaveChar(m_currButton.name, m_currCharScript);


        CloseLevelPanel(0);
    }

    public void CloseLevelPanel(int _pan)
    {
        m_audio.PlayOneShot(Resources.Load<AudioClip>("Sounds/Menu Sound 2"));

        EventSystem.current.currentSelectedGameObject.GetComponent<Button>().name = m_currButton.name;
        if (_pan == 0)
        {
            PanelScript.GetPanel("CharacterViewer Panel").m_panels[0].GetComponent<PanelScript>().PopulatePanel();
            PanelScript.GetPanel("New Action Panel").ClosePanel();
            m_saveButton = null;
        }
        else if (_pan == 1)
        {
            PanelScript.GetPanel("CharacterViewer Panel").m_panels[1].GetComponent<PanelScript>().PopulatePanel();
            PanelScript.GetPanel("New Status Panel").ClosePanel();
        }

        PanelScript.RemoveFromHistory("");
        
        if (!PanelScript.GetPanel("New Action Panel").m_inView && !PanelScript.GetPanel("New Status Panel").m_inView)
        {
            for (int i = 0; i < PanelScript.GetPanel("CharacterViewer Panel").m_buttons.Length; i++)
            {
                if (PanelScript.GetPanel("CharacterViewer Panel").m_buttons[i].GetComponentInChildren<Text>())
                    if (PanelScript.GetPanel("CharacterViewer Panel").m_buttons[i].GetComponentInChildren<Text>().text == "REMOVE" ||
                        PanelScript.GetPanel("CharacterViewer Panel").m_buttons[i].GetComponentInChildren<Text>().text == "SAVE" ||
                        PanelScript.GetPanel("CharacterViewer Panel").m_buttons[i].GetComponentInChildren<Text>().text == "LEVEL UP" && m_currCharScript.m_exp >= 10)
                        PanelScript.GetPanel("CharacterViewer Panel").m_buttons[i].interactable = true;
            }
        
            PanelScript.m_locked = false;
        }
    }

    public void NewRandomStat(Button[] _buttons)
    {
        DatabaseScript db = GameObject.Find("Database").GetComponent<DatabaseScript>();

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
                    {
                        actOK = false;
                        break;
                    }
                }

            } while (!actOK);

            _buttons[i].GetComponentInChildren<Text>().text = buff + debuff;
            _buttons[i].name = statAlt;

            _buttons[i].onClick.RemoveAllListeners();
            Button[] conButtons = PanelScript.m_confirmPanel.m_buttons;
            _buttons[i].onClick.AddListener(() => conButtons[1].GetComponent<ButtonScript>().ConfirmationButton("New Stats"));
        }
    }

    public void AddStatAlteration()
    {
        PanelScript statPanScript = PanelScript.GetPanel("CharacterViewer Panel").m_panels[1].GetComponent<PanelScript>();
        string[] statSeparated = m_statButton.name.Split('|');

        for (int i = 0; i < m_currCharScript.m_stats.Length; i++)
        {
            string[] s = statSeparated[i + 1].Split(':');
            m_currCharScript.m_stats[i] += int.Parse(s[1]);
            m_currCharScript.m_tempStats[i] += int.Parse(s[1]);
            statPanScript.m_text[i].color = Color.black;
        }

        PlayerPrefScript.SaveChar(m_currButton.name, m_currCharScript);
        CloseLevelPanel(1);
    }

    public void RandomTeam(Button _button)
    {
        PanelScript panScript = _button.transform.parent.GetComponent<PanelScript>();
        Button[] buttons = panScript.m_buttons;
        for (int i = 0; i < 6; i++)
        {
            m_currButton = buttons[i];
            RandomCharacter(1);
        }

        // Let the user decide how many random characters to add
        // What level the the characters will be
        // What colors
    }

    public void ClearTeam(Button _button)
    {
        PanelScript panScript = PanelScript.GetPanel("CharacterSlots").m_panels[int.Parse(_button.transform.parent.name)].GetComponent<PanelScript>();
        Button[] team = panScript.m_buttons;
        for (int i = 0; i < 6; i++)
        {
            m_currButton = team[i];
            Remove();
        }
    }

    public void StartGame()
    {
        CheckIfCompControlled();

        DontDestroyOnLoad(GameObject.Find("Database"));
        SceneManager.LoadScene("Scene1");
    }

    public void Rename()
    {
        Text text = PanelScript.GetPanel("CharacterViewer Panel").m_text[35];
        m_currCharScript.m_name = text.text;
        m_currButton.GetComponentInChildren<Text>().text = text.text;
        PlayerPrefScript.SaveChar(m_currButton.name, m_currCharScript);

    }

    public void ComputerControlled()
    {
        Button me = EventSystem.current.currentSelectedGameObject.GetComponent<Button>();

        if (me.image.color != Color.cyan)
        {
            m_audio.PlayOneShot(Resources.Load<AudioClip>("Sounds/Misc Sound 2"));
            me.GetComponent<ButtonScript>().m_oldColor = me.image.color;
            me.image.color = Color.cyan;
        }
        else
        {
            m_audio.PlayOneShot(Resources.Load<AudioClip>("Sounds/Misc Sound 2 OFF"));
            me.image.color = me.GetComponent<ButtonScript>().m_oldColor;
        }
    }

    private void CheckIfCompControlled()
    {
        PanelScript charSlots = PanelScript.GetPanel("CharacterSlots");

        for (int h = 0; h < charSlots.m_panels.Length; h++)
        {
            PanelScript currSlot = charSlots.m_panels[h].GetComponent<PanelScript>();

            Button[] buttons = currSlot.m_buttons;
            for (int i = 0; i < 6; i++)
            {
                m_currButton = buttons[i];
                string name = PlayerPrefs.GetString(m_currButton.name + ",name");

                if (name.Length == 0)
                    continue;

                PlayerPrefScript.LoadChar(m_currButton.name, m_currCharScript);

                if (currSlot.m_buttons[currSlot.m_buttons.Length - 1].image.color == Color.cyan)
                    m_currCharScript.m_isAI = true;
                else
                    m_currCharScript.m_isAI = false;

                Select();
            }
        }
    }
}
