using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PanelScript : MonoBehaviour {

    public enum butts { MOV_BUTT, ACT_BUTT, PASS_BUTT, STATUS_BUTT }
    public enum dir { UP, DOWN, LEFT, RIGHT, NULL }

    static public Color b_isFree = new Color(.5f, 1, .5f, 1);
    static public Color b_isDisallowed = new Color(1, .5f, .5f, 1);
    static public Color b_isHalf = new Color(1, 1, .5f, 1);
    static public Color b_isSpecial = new Color(1, .2f, 1, 1);

    static public List<PanelScript> m_history;
    static public List<PanelScript> m_allPanels;
    public bool m_inView;
    public dir m_direction;
    public float m_slideSpeed;
    public int m_boundryDis;
    public CharacterScript m_cScript;
    public GameObject m_main;
    public Button[] m_buttons;
    public PanelScript[] m_panels;
    public Text[] m_text;
    public Image[] m_images;
    static public bool m_locked;
    static public PanelScript m_confirmPanel;
    public AudioSource m_audio;

    // Use this for initialization
    void Start()
    {
        m_audio = gameObject.AddComponent<AudioSource>();
        m_locked = false;

        if (m_slideSpeed == 0)
            m_slideSpeed = 30.0f;
        if (GameObject.Find("Confirmation Panel"))
            m_confirmPanel = GameObject.Find("Confirmation Panel").GetComponent<PanelScript>();

        if (m_history == null)
            m_history = new List<PanelScript>();

        m_buttons = GetComponentsInChildren<Button>();

        if (m_boundryDis == 0)
        {
            if (m_direction == dir.UP)
                m_boundryDis = 180;
            else if (m_direction == dir.RIGHT)
                m_boundryDis = 475;
            else if (m_direction == dir.LEFT)
                m_boundryDis = -480;
            else if (m_direction == dir.DOWN)
                m_boundryDis = -180;
        }

        if (GetComponentsInChildren<Text>().Length > 0)
            m_text = GetComponentsInChildren<Text>();

        if (name == "Turn Panel")
            for (int i = 0; i < m_panels.Length; i++)
                m_panels[i].gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        RectTransform recTrans = GetComponent<RectTransform>();
        // REFACTOR
        if (Input.GetMouseButtonDown(1) && m_inView && m_direction == dir.UP && recTrans.offsetMax.y < 200 &&
            gameObject.tag != "Selector" && gameObject.name != "Confirmation Panel" && !m_locked ||
            Input.GetMouseButtonDown(1) && m_history.Count > 0 && GetRecentHistory().name == "Confirmation Panel" && m_locked ||
            Input.GetMouseButtonDown(1) && m_history.Count == 1 && GetRecentHistory().name == "Confirmation Panel")
            OnRightClick();

        Slide();

        if (name == "Turn Panel")
            TurnSlide();
    }

    public void OnRightClick()
    {
        AudioClip a = Resources.Load<AudioClip>("Sounds/Menu Sound 2");
        m_audio.PlayOneShot(a);
        if (m_cScript && m_cScript.m_boardScript)
            m_cScript.m_boardScript.m_selected = null;

        if (m_history.Count > 0)
        {
            RemoveFromHistory("");
            if (m_history.Count > 0)
                m_history[m_history.Count - 1].m_inView = true;
        }

        if (m_main && m_main.name == "Board" && name == "Action Panel")
        {
            BoardScript bScript = m_main.GetComponent<BoardScript>();
            CharacterScript cScript = bScript.m_currCharScript;
        }
    }

    static public void MenuPanelInit(string _canvasName)
    {
        m_history.Clear();

        string canvasName = _canvasName;
        Canvas can = GameObject.Find(canvasName).GetComponent<Canvas>();
        PanelScript[] pans = can.GetComponentsInChildren<PanelScript>();
        m_allPanels = new List<PanelScript>();

        for (int i = 0; i < pans.Length; i++)
        {
            if (pans[i].transform.parent.name == _canvasName)
                m_allPanels.Add(pans[i]);
        }
    }

    // General Panel
    public void PopulatePanel()
    {
        if (m_audio && m_direction == dir.UP)
            m_audio.PlayOneShot(Resources.Load<AudioClip>("Sounds/Menu Sound 1"));

        if (name == "Action Panel")
        {
            ResetButtons();

            if (m_main && m_main.name == "Board")
                ActionsAvailable();
            else
            {
                for (int i = 0; i < m_cScript.m_actions.Length; i++)
                {
                    string[] actsSeparated = m_cScript.m_actions[i].Split('|');
                    string[] eng = actsSeparated[(int)DatabaseScript.actions.ENERGY].Split(':');
                
                    if (eng[1][0] == 'g' || eng[1][0] == 'r' || eng[1][0] == 'w' || eng[1][0] == 'b')
                        FirstActionAvailable(m_panels[0].gameObject, m_cScript.m_actions[i]);
                    else if (eng[1].Length == 1)
                        FirstActionAvailable(m_panels[1].gameObject, m_cScript.m_actions[i]);
                    else if (eng[1].Length == 2)
                        FirstActionAvailable(m_panels[2].gameObject, m_cScript.m_actions[i]);
                    else if (eng[1].Length == 3)
                        FirstActionAvailable(m_panels[3].gameObject, m_cScript.m_actions[i]);
                }
            }
        }
        else if (name == "ActionPreview")
        {
            BoardScript bScript = m_main.GetComponent<BoardScript>();
            CharacterScript currScript = bScript.m_currCharScript;

            int def = m_cScript.m_tempStats[(int)CharacterScript.sts.DEF];
            int dmg = int.Parse(DatabaseScript.GetActionData(currScript.m_currAction, DatabaseScript.actions.DMG)) + currScript.m_tempStats[(int)CharacterScript.sts.DMG];
            string actName = DatabaseScript.GetActionData(currScript.m_currAction, DatabaseScript.actions.NAME);

            if (actName == "Bypass ATK" || actName == "Forceful ATK")
            {
                if (actName == "Bypass ATK")
                    def -= 2 + currScript.m_tempStats[(int)CharacterScript.sts.TEC];
                else if (actName == "Forceful ATK")
                    def -= 1 + currScript.m_tempStats[(int)CharacterScript.sts.TEC];
                if (def < 0)
                    def = 0;
            }
            else if (actName == "Burst ATK" || actName == "Power ATK" || actName == "Devastating ATK")
                dmg += currScript.m_tempStats[(int)CharacterScript.sts.TEC];

            dmg -= def;

            if (m_cScript.m_tempStats[(int)CharacterScript.sts.HP] >= m_cScript.m_tempStats[(int)CharacterScript.sts.HP] - dmg)
                m_text[0].text = "HP: " + m_cScript.m_tempStats[(int)CharacterScript.sts.HP].ToString() + " -> " + (m_cScript.m_tempStats[(int)CharacterScript.sts.HP] - dmg).ToString();
            else
                m_text[0].text = "HP: " + m_cScript.m_tempStats[(int)CharacterScript.sts.HP].ToString() + " -> " + m_cScript.m_tempStats[(int)CharacterScript.sts.HP].ToString();
        }
        else if (name == "ActionViewer Panel")
        {
            string actName = DatabaseScript.GetActionData(m_cScript.m_currAction, DatabaseScript.actions.NAME);
            string[] actStats = m_cScript.m_currAction.Split('|');
            string[] currStat = actStats[1].Split(':');
            m_text[0].text = currStat[1];

            currStat = actStats[2].Split(':');
            Button energy = GetComponentInChildren<Button>();
            ButtonScript engScript = energy.GetComponent<ButtonScript>();
            engScript.SetTotalEnergy(currStat[1]);

            currStat = actStats[3].Split(':');
            int finalDmg = (int.Parse(currStat[1]) + m_cScript.m_tempStats[(int)CharacterScript.sts.DMG]);

            if (actName == "Burst ATK" || actName == "Power ATK" || actName == "Devastating ATK")
                finalDmg += m_cScript.m_tempStats[(int)CharacterScript.sts.TEC];

            if (finalDmg > 0)
                m_text[2].text = "DMG: " + finalDmg;
            else
                m_text[2].text = "DMG: 0";

            currStat = actStats[4].Split(':');
            int finalRng = (int.Parse(currStat[1]) + m_cScript.m_tempStats[(int)CharacterScript.sts.RNG]);

            if (actName == "Pull ATK" || actName == "Snipe ATK" || actName == "Lunge ATK" || actName == "Redirect ATK" || actName == "Thrust ATK")
                finalRng += m_cScript.m_tempStats[(int)CharacterScript.sts.TEC];

            if (finalRng > 0)
                m_text[3].text = "RNG: " + finalRng;
            else
                m_text[3].text = "RNG: 0";

            currStat = actStats[5].Split(':');
            int finalRad = (int.Parse(currStat[1]) + m_cScript.m_tempStats[(int)CharacterScript.sts.RAD]);

            if (actName == "Magnet ATK")
                finalRad += m_cScript.m_tempStats[(int)CharacterScript.sts.TEC];

            if (finalRad > 0)
                m_text[4].text = "RAD: " + finalRad;
            else
                m_text[4].text = "RAD: 0";

            currStat = actStats[6].Split(':');
            m_text[6].text = DatabaseScript.ModifyActions(m_cScript, currStat[1]);
        }
        else if (name == "Character Panel")
        {
            for (int i = 0; i < m_buttons.Length; i++)
                m_buttons[i].onClick.RemoveAllListeners();
            // REFACTOR: Move all these functions to populate Panel except random character
            TeamMenuScript menuScript = m_main.GetComponent<TeamMenuScript>();
            m_buttons[0].onClick.AddListener(() => menuScript.NewCharacter());
            m_buttons[1].onClick.AddListener(() => GetPanel("Save/Load Panel").PopulatePanel());
            m_buttons[2].onClick.AddListener(() => GetPanel("PresetSelect Panel").PopulatePanel());
            m_buttons[3].onClick.AddListener(() => menuScript.RandomCharacter());
        }
        else if (name == "CharacterViewer Panel")
        {
            // If another panel is open, don't open character viewer for already loaded character

            Button currB = null;
            int res = 0;

            PanelScript actionScript = m_panels[0].GetComponent<PanelScript>();
            PanelScript statPan = m_panels[1].GetComponent<PanelScript>();

            if (m_main.name == "Menu")
            {
                TeamMenuScript tMenu = m_main.GetComponent<TeamMenuScript>();
                m_cScript = tMenu.m_currCharScript;
                m_cScript.InitializeStats();

                if (EventSystem.current.currentSelectedGameObject)
                    currB = EventSystem.current.currentSelectedGameObject.GetComponent<Button>();
                else
                    currB = tMenu.m_currButton;

                if (int.TryParse(currB.name, out res)) // If last pressed button is an int, it's a preset character panel button. Character slot buttons names are in the x,x format
                {
                    DatabaseScript dbScript = m_main.GetComponent<DatabaseScript>();

                    string[] presetDataSeparated = dbScript.m_presets[int.Parse(currB.name)].Split('|');
                    string[] presetName = presetDataSeparated[(int)DatabaseScript.presets.NAME].Split(':');
                    string[] presetColor = presetDataSeparated[(int)DatabaseScript.presets.COLORS].Split(':');

                    int gen = Random.Range(0, 2);
                    tMenu.FillOutCharacterData(presetName[1], presetColor[1], dbScript.GetActions(dbScript.m_presets[int.Parse(currB.name)]), "", 0, 1, gen);

                    // Fill out name
                    GetComponentInChildren<InputField>().text = m_cScript.m_name;

                    // Determine if select or remove will be visible
                    for (int i = 0; i < m_buttons.Length; i++)
                    {
                        if (m_buttons[i].name == "Select Button" && m_buttons[i].gameObject.transform.position.x > 1000)
                            m_buttons[i].gameObject.transform.SetPositionAndRotation(new Vector3(m_buttons[i].gameObject.transform.position.x - 1000, m_buttons[i].gameObject.transform.position.y, m_buttons[i].gameObject.transform.position.z), m_buttons[i].gameObject.transform.rotation);
                        if (m_buttons[i].name == "Remove Button" && m_buttons[i].gameObject.transform.position.x < 1000)
                            m_buttons[i].gameObject.transform.SetPositionAndRotation(new Vector3(m_buttons[i].gameObject.transform.position.x + 1000, m_buttons[i].gameObject.transform.position.y, m_buttons[i].gameObject.transform.position.z), m_buttons[i].gameObject.transform.rotation);
                    }
                }
                else
                {
                    if (!m_inView)
                        tMenu.m_currButton = currB;

                    m_cScript = PlayerPrefScript.LoadChar(currB.name, m_cScript);
                    tMenu.m_currCharScript = PlayerPrefScript.LoadChar(currB.name, tMenu.m_currCharScript);

                    // Fill out name
                    m_text[36].text = m_cScript.m_name;
                    GetComponentInChildren<InputField>().text = m_cScript.m_name;

                    m_cScript.m_exp = 10;
                    // Determine if select or remove will be visible
                    for (int i = 0; i < m_buttons.Length; i++)
                    {
                        if (m_buttons[i].name == "Level up" && m_cScript.m_exp >= 10 && !GetPanel("New Action Panel").m_inView && !GetPanel("New Status Panel").m_inView)
                            m_buttons[i].interactable = true;
                        else if (m_buttons[i].name == "Level up" && m_cScript.m_exp < 10)
                            m_buttons[i].interactable = false;

                        if (m_buttons[i].name == "Select Button" && m_buttons[i].gameObject.transform.position.x < 1000)
                            m_buttons[i].gameObject.transform.SetPositionAndRotation(new Vector3(m_buttons[i].gameObject.transform.position.x + 1000, m_buttons[i].gameObject.transform.position.y, m_buttons[i].gameObject.transform.position.z), m_buttons[i].gameObject.transform.rotation);
                        if (m_buttons[i].name == "Remove Button" && m_buttons[i].gameObject.transform.position.x > 1000)
                            m_buttons[i].gameObject.transform.SetPositionAndRotation(new Vector3(m_buttons[i].gameObject.transform.position.x - 1000, m_buttons[i].gameObject.transform.position.y, m_buttons[i].gameObject.transform.position.z), m_buttons[i].gameObject.transform.rotation);
                    }
                }
            }
            else if (m_main.name == "Board")
                m_cScript = m_main.GetComponent<BoardScript>().m_selected.GetComponent<TileScript>().m_holding.GetComponent<CharacterScript>();

            // Fill out Action Panel
            actionScript.m_cScript = m_cScript;
            actionScript.PopulatePanel();
            // Fill out Status Panel
            statPan.m_cScript = m_cScript;
            statPan.PopulatePanel();
        }
        else if (name == "HUD Panel LEFT" || name == "HUD Panel RIGHT")
        {
            m_panels[0].GetComponent<Image>().color = new Color(m_cScript.m_teamColor.r + 0.3f, m_cScript.m_teamColor.g + 0.3f, m_cScript.m_teamColor.b + 0.3f, 1);
            GetComponentsInChildren<Text>()[0].text = m_cScript.m_name;
            GetComponentsInChildren<Text>()[1].text = "HP: " + m_cScript.m_tempStats[(int)CharacterScript.sts.HP] + "/" + m_cScript.m_stats[(int)CharacterScript.sts.HP];

            if (m_cScript.m_tempStats[(int)CharacterScript.sts.SPD].ToString().Length > 1)
                GetComponentsInChildren<Text>()[2].text = "   " + m_cScript.m_tempStats[(int)CharacterScript.sts.SPD];
            else
                GetComponentsInChildren<Text>()[2].text = "    " + m_cScript.m_tempStats[(int)CharacterScript.sts.SPD];

            if (m_cScript.m_tempStats[(int)CharacterScript.sts.DMG].ToString().Length > 1)
                GetComponentsInChildren<Text>()[3].text = "   " + m_cScript.m_tempStats[(int)CharacterScript.sts.DMG];
            else
                GetComponentsInChildren<Text>()[3].text = "    " + m_cScript.m_tempStats[(int)CharacterScript.sts.DMG];

            if (m_cScript.m_tempStats[(int)CharacterScript.sts.DEF].ToString().Length > 1)
                GetComponentsInChildren<Text>()[4].text = "   " + m_cScript.m_tempStats[(int)CharacterScript.sts.DEF];
            else
                GetComponentsInChildren<Text>()[4].text = "    " + m_cScript.m_tempStats[(int)CharacterScript.sts.DEF];

            if (m_cScript.m_tempStats[(int)CharacterScript.sts.MOV].ToString().Length > 1)
                GetComponentsInChildren<Text>()[5].text = "   " + m_cScript.m_tempStats[(int)CharacterScript.sts.MOV];
            else
                GetComponentsInChildren<Text>()[5].text = "    " + m_cScript.m_tempStats[(int)CharacterScript.sts.MOV];

            if (m_cScript.m_tempStats[(int)CharacterScript.sts.RNG].ToString().Length > 1)
                GetComponentsInChildren<Text>()[6].text = "   " + m_cScript.m_tempStats[(int)CharacterScript.sts.RNG];
            else
                GetComponentsInChildren<Text>()[6].text = "    " + m_cScript.m_tempStats[(int)CharacterScript.sts.RNG];

            if (m_cScript.m_tempStats[(int)CharacterScript.sts.TEC].ToString().Length > 1)
                GetComponentsInChildren<Text>()[7].text = "   " + m_cScript.m_tempStats[(int)CharacterScript.sts.TEC];
            else
                GetComponentsInChildren<Text>()[7].text = "    " + m_cScript.m_tempStats[(int)CharacterScript.sts.TEC];

            if (GetComponentInChildren<Button>())
            {
                Button button = GetComponentInChildren<Button>();
                ButtonScript buttScript = button.GetComponent<ButtonScript>();
                buttScript.SetTotalEnergy(m_cScript.m_color);

                StatusSymbolSetup();
            }

            m_panels[(int)CharacterScript.HUDPan.ACT_PAN].GetComponent<PanelScript>().m_cScript = m_cScript;
            m_panels[(int)CharacterScript.HUDPan.ACT_PAN].GetComponent<PanelScript>().PopulatePanel();

            m_cScript.m_player.SetEnergyPanel(m_cScript);

            if (name == "HUD Panel LEFT")
            {
                m_panels[(int)CharacterScript.HUDPan.MOV_PASS].GetComponent<PanelScript>().m_buttons[0].GetComponent<ButtonScript>().m_cScript = m_cScript;
                if (m_cScript.m_effects[(int)StatusScript.effects.IMMOBILE])
                {
                    m_panels[(int)CharacterScript.HUDPan.MOV_PASS].GetComponent<PanelScript>().m_buttons[0].GetComponent<Image>().color = b_isDisallowed;
                    m_panels[(int)CharacterScript.HUDPan.MOV_PASS].GetComponent<PanelScript>().m_buttons[0].interactable = false;
                }
                else if (m_cScript.m_hasActed[(int)CharacterScript.trn.ACT] == 2)
                    m_panels[(int)CharacterScript.HUDPan.MOV_PASS].GetComponent<PanelScript>().m_buttons[0].GetComponent<Image>().color = b_isHalf;
                else
                    m_panels[(int)CharacterScript.HUDPan.MOV_PASS].GetComponent<PanelScript>().m_buttons[0].GetComponent<Image>().color = Color.white;
            }
        }
        else if (name == "New Action Panel")
        {
            m_locked = true;
            for (int i = 0; i < m_buttons.Length - 1; i++) // -1 just so we get only the action buttons
            {
                string action = m_main.GetComponent<TeamMenuScript>().NewRandomAction(m_buttons);
                m_buttons[i].GetComponent<ButtonScript>().m_action = action;
                m_buttons[i].GetComponentInChildren<Text>().text = DatabaseScript.GetActionData(action, DatabaseScript.actions.NAME);
                m_buttons[i].GetComponent<ButtonScript>().SetTotalEnergy(DatabaseScript.GetActionData(action, DatabaseScript.actions.ENERGY));

                m_buttons[i].onClick.RemoveAllListeners();
                m_buttons[i].onClick.AddListener(() => m_main.GetComponent<TeamMenuScript>().NewActionSelection());
            }
        }
        else if (name == "New Status Panel")
        {
            m_locked = true;
            m_main.GetComponent<TeamMenuScript>().NewRandomStat(m_buttons);
        }
        else if (name == "PresetSelect Panel")
        {
            DatabaseScript dbScript = m_main.GetComponent<DatabaseScript>();

            for (int i = 0; i < m_buttons.Length; i++)
            {
                Button butt = m_buttons[i];
                Text t = butt.GetComponentInChildren<Text>();
                t.text = dbScript.GetDataValue(dbScript.m_presets[i], "Name:");
                butt.name = i.ToString();
                butt.onClick.RemoveAllListeners();
                butt.onClick.AddListener(() => m_panels[0].GetComponent<PanelScript>().PopulatePanel());
                ButtonScript buttScript = butt.GetComponent<ButtonScript>();
                buttScript.SetTotalEnergy(dbScript.GetDataValue(dbScript.m_presets[i], "Colors:"));
            }
        }
        else if (name == "Round Panel")
        {
            BoardScript bScript = m_main.GetComponent<BoardScript>();
            GetComponentInChildren<Text>().text = "Round: " + bScript.m_roundCount;
        }
        else if (name == "Save/Load Panel")
        {
            if (EventSystem.current.currentSelectedGameObject.GetComponent<Button>().name == "Save Button")
                m_text[0].text = "SAVE";
            else if (EventSystem.current.currentSelectedGameObject.GetComponent<Button>().name == "Load")
                m_text[0].text = "LOAD";

            Button[] button = m_buttons;
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    string key = i.ToString() + ',' + j.ToString() + "SAVE" + ",name";
                    string name = PlayerPrefs.GetString(key);
                    if (name.Length > 0)
                    {
                        key = i.ToString() + ',' + j.ToString() + "SAVE" + ",color";
                        string color = PlayerPrefs.GetString(key);

                        m_main.GetComponent<TeamMenuScript>().SetCharSlot(button[i * 4 + j], name, color);
                    }
                    button[i * 4 + j].onClick.RemoveAllListeners();
                    if (EventSystem.current.currentSelectedGameObject.GetComponent<Button>().name == "Save Button")
                    {
                        Button[] childButtons = m_confirmPanel.GetComponentsInChildren<Button>();
                        button[i * 4 + j].onClick.AddListener(() => childButtons[1].GetComponent<ButtonScript>().ConfirmationButton("Save"));
                    }
                    else if (EventSystem.current.currentSelectedGameObject.GetComponent<Button>().name == "Load")
                        button[i * 4 + j].onClick.AddListener(() => m_main.GetComponent<TeamMenuScript>().Load());
                }
            }
        }
        else if (name == "Status Panel")
        {
            if (m_main && m_main.name == "CharacterViewer Panel")
                m_text[0].text = "";
            else
                m_text[0].text = m_cScript.m_name;

            if (m_cScript.m_gender == 0)
                m_images[8].sprite = Resources.Load<Sprite>("Symbols/Male Symbol");
            else if (m_cScript.m_gender == 1)
                m_images[8].sprite = Resources.Load<Sprite>("Symbols/Female Symbol");

            m_text[1].text = "HP: " + m_cScript.m_tempStats[(int)CharacterScript.sts.HP] + "/" + m_cScript.m_stats[(int)CharacterScript.sts.HP];
            m_text[2].text = "SPD: " + m_cScript.m_tempStats[(int)CharacterScript.sts.SPD];
            m_text[3].text = "DMG: " + m_cScript.m_tempStats[(int)CharacterScript.sts.DMG];
            m_text[4].text = "DEF: " + m_cScript.m_tempStats[(int)CharacterScript.sts.DEF];
            m_text[5].text = "MOV: " + m_cScript.m_tempStats[(int)CharacterScript.sts.MOV];
            m_text[6].text = "RNG: " + m_cScript.m_tempStats[(int)CharacterScript.sts.RNG];
            m_text[7].text = "TEC: " + m_cScript.m_tempStats[(int)CharacterScript.sts.TEC];

            if (m_cScript.m_accessories[0] != null)
                m_text[8].text = "ACC: " + m_cScript.m_accessories[0];
            if (m_cScript.m_accessories[1] != null)
                m_text[9].text = "ACC: " + m_cScript.m_accessories[1];

            m_buttons[0].GetComponent<ButtonScript>().SetTotalEnergy(m_cScript.m_color);
            StatusSymbolSetup();
            
        }
        else if (name == "Status Selector")
        {
            StatusSymbolSetup();
        }
        else if (name == "StatusViewer Panel")
        {
            if (m_cScript)
            {
                StatusScript[] statScripts = m_cScript.GetComponents<StatusScript>();

                m_images[0].sprite = statScripts[m_cScript.m_currStatus].m_sprite;
                m_images[0].color = statScripts[m_cScript.m_currStatus].m_color;

                m_text[0].text = "Duration: " + statScripts[m_cScript.m_currStatus].m_lifeSpan.ToString();
                m_text[1].text = statScripts[m_cScript.m_currStatus].m_effect;
            }
            else
            {
                PowerupScript pUP = m_main.GetComponent<BoardScript>().m_highlightedTile.m_holding.GetComponent<PowerupScript>();

                m_images[0].sprite = pUP.m_sprite;
                m_images[0].color = pUP.m_color;

                m_text[0].text = "Duration: N/A";
                m_text[1].text = pUP.m_effect;
            }
        }

        if (m_direction == dir.UP && !m_inView)
        {
            if (m_history.Count > 0 && name != "Confirmation Panel")
                m_history[m_history.Count - 1].GetComponent<PanelScript>().m_inView = false;

            m_history.Add(this);
        }

        if (m_direction != dir.NULL)
            m_inView = true;

        if (name == "Confirmation Panel")
        {
            if ((int)Input.mousePosition.x < 120)
                transform.SetPositionAndRotation(new Vector3(120, 1000, transform.position.z), transform.rotation);
            else if ((int)Input.mousePosition.x > 1160)
                transform.SetPositionAndRotation(new Vector3(1160, 1000, transform.position.z), transform.rotation);
            else
                transform.SetPositionAndRotation(new Vector3(Input.mousePosition.x, 1000, transform.position.z), transform.rotation);


            if ((int)Input.mousePosition.y > 550)
                m_boundryDis = 300;
            else
                m_boundryDis = (int)Input.mousePosition.y - 250;
        }
    }

    public void ActionsAvailable()
    {
        int activeCount = 0;
        for (int i = 0; i < m_cScript.m_actions.Length; i++)
        {
            string name = DatabaseScript.GetActionData(m_cScript.m_actions[i], DatabaseScript.actions.NAME);
            string eng = DatabaseScript.GetActionData(m_cScript.m_actions[i], DatabaseScript.actions.ENERGY);

            Text text = m_buttons[i].GetComponentInChildren<Text>();
            ButtonScript buttScript = m_buttons[i].GetComponent<ButtonScript>();
            buttScript.m_action = m_cScript.m_actions[i];
            buttScript.SetTotalEnergy(eng);
            buttScript.m_cScript = m_cScript;
            Text t = m_buttons[i].GetComponentInChildren<Text>();
            t.text = name;

            // REFACTOR: Obnoxious check to see if there is a current action and if i
            CharacterScript currCharScript = m_main.GetComponent<BoardScript>().m_currCharScript;
            string currCharActName = "";
            if (currCharScript.m_currAction.Length > 0)
                currCharActName = DatabaseScript.GetActionData(currCharScript.m_currAction, DatabaseScript.actions.NAME);

            bool disabled = false;
            int ind = 0;
            for (int j = 0; j < m_cScript.m_actions.Length; j++)
            {
                if (name == DatabaseScript.GetActionData(m_cScript.m_actions[j], DatabaseScript.actions.NAME))
                {
                    ind = j;
                    if (m_cScript.m_isDiabled[j] != 0)
                        disabled = true;
                }
            }

            // ACTION PREVENTION
            // REFACTOR
            if (m_cScript.m_effects[(int)StatusScript.effects.WARD] && CharacterScript.CheckIfAttack(name) ||
                m_cScript.m_effects[(int)StatusScript.effects.DELAY] && PlayerScript.CheckIfGains(eng) && eng.Length == 2 ||
                currCharActName == "Redirect ATK" && currCharScript != m_cScript && CharacterScript.CheckIfAttack(name) ||
                currCharActName == "Redirect ATK" && currCharScript != m_cScript && eng.Length > 2 ||
                currCharActName == "Copy ATK" && currCharScript != m_cScript && !CharacterScript.CheckIfAttack(name) ||
                currCharActName == "Copy ATK" && currCharScript != m_cScript && CharacterScript.CheckIfAttack(name) && CharacterScript.CheckActionLevel(eng) > currCharScript.m_tempStats[(int)CharacterScript.sts.TEC] ||
                currCharActName == "Hack ATK" && currCharScript != m_cScript && PlayerScript.CheckIfGains(eng) ||
                m_cScript.m_effects[(int)StatusScript.effects.HINDER] && !CharacterScript.CheckIfAttack(name) ||
                disabled)
                m_buttons[i].GetComponent<Image>().color = b_isDisallowed;
            else if (currCharActName == "Redirect ATK" && currCharScript != m_cScript && !CharacterScript.CheckIfAttack(name) && eng.Length <= 2 ||
                currCharActName == "Copy ATK" && currCharScript != m_cScript && CharacterScript.CheckIfAttack(name) && CharacterScript.CheckActionLevel(eng) <= currCharScript.m_tempStats[(int)CharacterScript.sts.TEC])
            {
                m_buttons[i].GetComponent<Image>().color = b_isFree;
                m_buttons[i].onClick.AddListener(() => currCharScript.ActionTargeting(currCharScript.m_tile));
                if (GetPanel("Choose Panel").m_inView == true)
                    m_buttons[i].interactable = true;
                else
                    m_buttons[i].interactable = false;
            }
            else if (currCharActName == "Hack ATK" && currCharScript != m_cScript && !PlayerScript.CheckIfGains(eng))
            {
                    m_buttons[i].GetComponent<Image>().color = b_isSpecial;
                    m_buttons[i].onClick.AddListener(() => m_cScript.DisableSelectedAction(ind));
                    m_buttons[i].interactable = true;
            }
            else
            {
                m_buttons[i].GetComponent<Image>().color = new Color(1, 1, 1, 1);
                if (m_cScript.m_player.CheckEnergy(eng) && m_cScript.m_hasActed[(int)CharacterScript.trn.ACT] < 3)
                {
                    m_buttons[i].interactable = true;
                    if (m_cScript.m_player.CheckEnergy(eng) && m_cScript.m_hasActed[(int)CharacterScript.trn.ACT] == 2)
                        m_buttons[i].GetComponent<Image>().color = b_isHalf;
                }
                else
                    m_buttons[i].interactable = false;
            }
            activeCount++;
        }

        for (int i = activeCount; i < m_buttons.Length; i++)
            m_buttons[i].interactable = false;
    }

    public void FirstActionAvailable(GameObject panel, string action)
    {
        Button[] buttons = panel.GetComponentsInChildren<Button>();

        for (int i = 0; i < buttons.Length; i++)
        {
            Text text = buttons[i].GetComponentInChildren<Text>();

            if (text.text != "EMPTY")
                continue;

            buttons[i].name = action;
            ButtonScript buttScript = buttons[i].GetComponent<ButtonScript>();
            buttScript.m_action = action;

            Text t = buttons[i].GetComponentInChildren<Text>();
            string[] actsSeparated = action.Split('|');
            string[] name = actsSeparated[(int)DatabaseScript.actions.NAME].Split(':');
            string[] eng = actsSeparated[(int)DatabaseScript.actions.ENERGY].Split(':');

            t.text = name[1];
            buttScript.SetTotalEnergy(eng[1]);


            if (m_main && m_main.name == "Board")
            {
                // REFACTOR: Obnoxious check to see if there is a current action and if i
                CharacterScript currCharScript = m_main.GetComponent<BoardScript>().m_currCharScript;
                string[] currCharAct = currCharScript.m_currAction.Split('|');
                string currCharActName = currCharAct[0];
                if (currCharScript.m_currAction.Length > 0)
                {
                    currCharAct = currCharAct[1].Split(':');
                    currCharActName = currCharAct[1];
                }

                bool disabled = false;
                int ind = 0;
                for (int j = 0; j < m_cScript.m_actions.Length; j++)
                {
                    if (name[1] == DatabaseScript.GetActionData(m_cScript.m_actions[j], DatabaseScript.actions.NAME))
                    {
                        ind = j;
                        if (m_cScript.m_isDiabled[j] != 0)
                            disabled = true;
                    }

                }

                // ACTION PREVENTION
                // REFACTOR
                if (m_cScript.m_effects[(int)StatusScript.effects.WARD] && CharacterScript.CheckIfAttack(name[1]) ||
                    m_cScript.m_effects[(int)StatusScript.effects.DELAY] && PlayerScript.CheckIfGains(eng[1]) && eng[1].Length == 2 ||
                    currCharActName == "Redirect ATK" && currCharScript != m_cScript && CharacterScript.CheckIfAttack(name[1]) ||
                    currCharActName == "Redirect ATK" && currCharScript != m_cScript && eng[1].Length > 2 ||
                    currCharActName == "Copy ATK" && currCharScript != m_cScript && !CharacterScript.CheckIfAttack(name[1]) ||
                    currCharActName == "Copy ATK" && currCharScript != m_cScript && CharacterScript.CheckIfAttack(name[1]) && CharacterScript.CheckActionLevel(eng[1]) > currCharScript.m_tempStats[(int)CharacterScript.sts.TEC] ||
                    m_cScript.m_effects[(int)StatusScript.effects.HINDER] && !CharacterScript.CheckIfAttack(name[1]) ||
                    disabled)
                    buttons[i].GetComponent<Image>().color = b_isDisallowed;
                else if (currCharActName == "Redirect ATK" && currCharScript != m_cScript && !CharacterScript.CheckIfAttack(name[1]) && eng[1].Length <= 2 ||
                    currCharActName == "Copy ATK" && currCharScript != m_cScript && CharacterScript.CheckIfAttack(name[1]) && CharacterScript.CheckActionLevel(eng[1]) <= currCharScript.m_tempStats[(int)CharacterScript.sts.TEC])
                {
                    buttons[i].GetComponent<Image>().color = b_isFree;
                    buttons[i].onClick.AddListener(() => currCharScript.ActionTargeting(currCharScript.m_tile));
                    buttons[i].interactable = true;
                }
                else if (currCharActName == "Hack ATK" && currCharScript != m_cScript)
                {
                    if (!PlayerScript.CheckIfGains(eng[1]) && !disabled)
                    {
                        buttons[i].GetComponent<Image>().color = new Color(1, 1, 1, 1);
                        buttons[i].onClick.AddListener(() => m_cScript.DisableSelectedAction(ind));
                        buttons[i].interactable = true;
                    }
                    else
                    {
                        buttons[i].GetComponent<Image>().color = new Color(1, 1, 1, 1);
                        buttons[i].interactable = false;
                    }
                }
                else
                {
                    buttons[i].GetComponent<Image>().color = new Color(1, 1, 1, 1);
                    if (m_cScript.m_player.CheckEnergy(eng[1]))
                    {
                        if (m_cScript)
                            buttons[i].onClick.AddListener(() => currCharScript.ActionTargeting(currCharScript.m_tile));
                        buttons[i].interactable = true;
                    }
                }
            }
            else
                buttons[i].interactable = true;

            return;
        }
    }

    public void ResetButtons()
    {
        for (int i = 0; i < m_panels.Length; i++)
        {
            Button[] buttons = m_panels[i].GetComponentsInChildren<Button>();

            for (int j = 0; j < buttons.Length; j++)
            {
                buttons[j].onClick.RemoveAllListeners();
                buttons[j].interactable = false;
                buttons[j].GetComponent<Image>().color = new Color(1, 1, 1, 1);
                Text t = buttons[j].GetComponentInChildren<Text>();
                t.text = "EMPTY";
                ButtonScript buttScript = buttons[j].GetComponent<ButtonScript>();

                for (int k = 0; k < buttScript.m_energyPanel.Length; k++)
                    buttScript.m_energyPanel[k].SetActive(false);
            }
        }
    }

    private void Slide()
    {
        RectTransform recTrans = GetComponent<RectTransform>();

        if (m_direction == dir.UP)
        {
            if (m_inView)
            {
                if (recTrans.offsetMax.y > m_boundryDis)
                    transform.SetPositionAndRotation(new Vector3(transform.position.x, transform.position.y - m_slideSpeed, transform.position.z), transform.rotation);
            }
            else
                if (recTrans.offsetMax.y < 750)
                transform.SetPositionAndRotation(new Vector3(transform.position.x, transform.position.y + m_slideSpeed, transform.position.z), transform.rotation);
        }
        else if (m_direction == dir.RIGHT)
        {
            if (m_inView)
            {
                if (recTrans.offsetMax.x > m_boundryDis)
                    transform.SetPositionAndRotation(new Vector3(transform.position.x - m_slideSpeed, transform.position.y, transform.position.z), transform.rotation);
            }
            else
                if (recTrans.offsetMax.x < 877)
                transform.SetPositionAndRotation(new Vector3(transform.position.x + m_slideSpeed, transform.position.y, transform.position.z), transform.rotation);
        }
        else if (m_direction == dir.LEFT)
        {
            if (m_inView)
            {
                if (recTrans.offsetMax.x < m_boundryDis)
                    transform.SetPositionAndRotation(new Vector3(transform.position.x + m_slideSpeed, transform.position.y, transform.position.z), transform.rotation);
            }
            else
                if (recTrans.offsetMax.x > -877)
                transform.SetPositionAndRotation(new Vector3(transform.position.x - m_slideSpeed, transform.position.y, transform.position.z), transform.rotation);
        }
        else if (m_direction == dir.DOWN)
        {
            if (m_inView)
            {
                if (recTrans.offsetMax.y < m_boundryDis)
                    transform.SetPositionAndRotation(new Vector3(transform.position.x, transform.position.y + m_slideSpeed, transform.position.z), transform.rotation);
            }
            else
                if (recTrans.offsetMax.y > -500)
                transform.SetPositionAndRotation(new Vector3(transform.position.x, transform.position.y - m_slideSpeed, transform.position.z), transform.rotation);
        }
    }

    // Turn Panel
    private void TurnSlide()
    {
        BoardScript bScript = m_main.GetComponent<BoardScript>();
        GameObject pan = null;
        float width = 65.0f;
        float start = 190.0f;

        // Set current panel and count 
        for (int i = 0; i < m_panels.Length; i++)
        {
            if (m_panels[i].gameObject.activeSelf)
            {
                pan = m_panels[i].gameObject;
                break;
            }
        }

        if (pan == null)
            return;

        CharacterScript c = pan.GetComponentInChildren<ButtonScript>().m_cScript;

        //if (bScript.m_currRound.Count >= 8 && bScript.m_newTurn) //notListed.Count > 0 && notListed.Count + count > bScript.m_currRound.Count
        //{
        //    for (int i = 0; i < 7; i++)
        //    {
        //        ButtonScript b = m_panels[i + 1].GetComponentInChildren<ButtonScript>();
        //        TurnPanelInit(i, b.m_cScript);
        //    }
        //
        //    TurnPanelInit(7, bScript.m_currRound[7].GetComponent<CharacterScript>());
        //
        //    if (bScript.m_currRound.Count == 8)
        //        m_panels[8].SetActive(false);
        //    else
        //    {
        //        Text t = m_panels[8].GetComponentInChildren<Text>();
        //        t.text = "x" + (bScript.m_currRound.Count - 8).ToString();
        //    }
        //
        //    bScript.m_newTurn = false;
        //}
        if (bScript.m_currRound.Count < m_panels.Length - 1 && bScript.m_newTurn) // if one of the panels is removed, move it every frame unitl it slides off to the left
        {
            if (pan.transform.position.y < 750)
                pan.transform.SetPositionAndRotation(new Vector3(pan.transform.position.x, pan.transform.position.y + 10.0f, pan.transform.position.z), pan.transform.rotation);
            else if (pan.transform.position.y >= 750)
            {
                pan.SetActive(false);
                if (pan.GetComponent<Image>().color != new Color(1, .5f, .5f, 1))
                    bScript.m_newTurn = false;
            }
        }
        else
        {
            // if was removed, check all of the panels that are still left to see if they are in their new position. If not, slide them down each frame until they are

            int widthCount = 0;

            for (int i = 0; i < m_panels.Length - 1; i++)
            {
                pan = m_panels[i].gameObject;
                if (!pan.activeSelf)
                    continue;

                float widthMod = start + width * widthCount;
                widthCount++;

                if (pan.transform.position.x > widthMod)
                {
                    pan.transform.SetPositionAndRotation(new Vector3(pan.transform.position.x - 10.0f, pan.transform.position.y, pan.transform.position.z), pan.transform.rotation);

                    if (pan.transform.position.x < widthMod)
                        pan.transform.SetPositionAndRotation(new Vector3(widthMod, pan.transform.position.y, pan.transform.position.z), pan.transform.rotation);
                }

                if (pan.transform.position.y > 705)
                {
                    pan.transform.SetPositionAndRotation(new Vector3(pan.transform.position.x, pan.transform.position.y - 5.0f, pan.transform.position.z), pan.transform.rotation);

                    if (pan.transform.position.y < 705)
                        pan.transform.SetPositionAndRotation(new Vector3(pan.transform.position.x, 705, pan.transform.position.z), pan.transform.rotation);
                }
            }
        }
    }

    private void TurnPanelInit(int _ind, CharacterScript _charScript)
    {
        m_panels[_ind].gameObject.SetActive(true);

        if (_charScript.m_turnPanels.Count > 0)
            _charScript.m_turnPanels[0] = m_panels[_ind].gameObject;
        else
            _charScript.m_turnPanels.Add(m_panels[_ind].gameObject);

        if (_charScript.m_effects[(int)StatusScript.effects.STUN] && _ind == 0)
            m_panels[_ind].GetComponent<Image>().color = new Color(1, .5f, .5f, 1);
        else
            m_panels[_ind].GetComponent<Image>().color = new Color(_charScript.m_teamColor.r + 0.3f, _charScript.m_teamColor.g + 0.3f, _charScript.m_teamColor.b + 0.3f, 1);

        Text t = m_panels[_ind].GetComponentInChildren<Text>();
        t.text = _charScript.m_name;

        if (m_panels[_ind].GetComponentInChildren<Button>())
        {
            Button button = m_panels[_ind].GetComponentInChildren<Button>();
            ButtonScript buttScript = button.GetComponent<ButtonScript>();
            buttScript.SetTotalEnergy(_charScript.m_color);
            buttScript.m_cScript = _charScript;
        }
    }

    public void NewTurnOrder()
    {
        BoardScript bScript = m_main.GetComponent<BoardScript>();
        float width = 65.0f;
        float start = 190.0f;
        int roundCountModded = bScript.m_currRound.Count - 1;

        if (roundCountModded == 0)
            return;

        //if (roundCountModded > 8)
        //{
        //    m_panels[8].SetActive(true);
        //    Text t = m_panels[8].GetComponentInChildren<Text>();
        //    t.text = "x" + (roundCountModded - 8).ToString();
        //    roundCountModded = 8;
        //}

        for (int i = 0; i < roundCountModded; i++)
        {
            m_panels[i].transform.SetPositionAndRotation(new Vector3(start + width * i, m_panels[i].transform.position.y + i * 10, m_panels[i].transform.position.z), m_panels[i].transform.rotation);
            TurnPanelInit(i, bScript.m_currRound[i + 1].GetComponent<CharacterScript>()); // +1 because we are avoiding the first index
        }
    }

    // Maybe move
    private void StatusSymbolSetup()
    {
        StatusScript[] statScripts = m_cScript.GetComponents<StatusScript>();

        for (int i = 0; i < 8; i++)
            m_images[i].enabled = false;

        for (int i = 0; i < statScripts.Length; i++)
        {
            if (statScripts[i].m_lifeSpan <= 0)
                continue;

            m_images[i].enabled = true;
            ButtonScript buttScript = m_images[i].GetComponentInChildren<ButtonScript>();
            buttScript.m_parent = this;
            m_images[i].name = i.ToString();
            m_images[i].sprite = statScripts[i].m_sprite;
            m_images[i].color = statScripts[i].m_color;
        }
    }

    static public void CloseHistory()
    {
        while(m_history.Count > 0)
        {
            m_history[0].m_inView = false;
            m_history.RemoveAt(0);
        }
    }

    static public bool CheckIfPanelOpen()
    {
        for (int i = 0; i < m_allPanels.Count; i++)
            if (m_allPanels[i].m_inView && m_allPanels[i].m_direction == dir.UP)
                return true;

        return false;
    }

    static public PanelScript GetRecentHistory()
    {
        if (m_history.Count > 0)
            return m_history[m_history.Count - 1];

        return null;
    }

    static public void RemoveFromHistory(string _name)
    {
        if (_name == "")
        {
            m_history[m_history.Count - 1].m_inView = false;
            m_history.RemoveAt(m_history.Count - 1);
            return;
        }

        for (int i = 0; i < m_history.Count; i++)
        {
            if (m_history[i].name == _name)
            {
                m_history[i].m_inView = false;
                m_history.RemoveAt(i);
            }           
        }
    }

    static public PanelScript GetPanel(string _name)
    {
        for (int i = 0; i < m_allPanels.Count; i++)
            if (m_allPanels[i].name == _name)
                return m_allPanels[i].GetComponent<PanelScript>();

        return null;
    }
}
