using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PanelScript : MonoBehaviour {

    public enum HUDPan { CHAR_PORT, ACT_PAN, MOV_PASS, ENG_PAN, STS_BUTTS, PARA_PAN}

    static public Color b_isFree = new Color(.5f, 1, .5f, 1);
    static public Color b_isDisallowed = new Color(1, .5f, .5f, 1);
    static public Color b_isHalf = new Color(1, 1, .5f, 1);
    static public Color b_isSpecial = new Color(1, .2f, 1, 1);
    
    // Children
    public PanelScript[] m_panels;
    public Text[] m_text;
    public Image[] m_images;
    public Button[] m_buttons;
    public PanelSlideScript m_slideScript;
    public TurnPanelScript m_turnPanel;

    // References
    public GameObject m_main;
    public CharacterScript m_cScript;
    public PanelManagerScript m_pMan;


    // Use this for initialization
    void Start()
    {
        //m_audio = gameObject.AddComponent<AudioSource>();

        m_buttons = GetComponentsInChildren<Button>();

        if (GetComponentsInChildren<Text>().Length > 0)
            m_text = GetComponentsInChildren<Text>();

        if (GetComponent<PanelSlideScript>())
            m_slideScript = GetComponent<PanelSlideScript>();
        else if (GetComponent<TurnPanelScript>())
            m_turnPanel = GetComponent<TurnPanelScript>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // General Panel
    public void PopulatePanel()
    {
        //AudioManagerScript.Play

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
        else if (name == "ActionViewer Panel")
        {
            string actName = DatabaseScript.GetActionData(m_cScript.m_currAction, DatabaseScript.actions.NAME);
            string[] actStats = m_cScript.m_currAction.Split('|');
            string[] currStat = actStats[1].Split(':');
            PanelScript actView = m_panels[1];
            PanelScript main = m_panels[0];
            actView.m_text[0].text = currStat[1];

            currStat = actStats[2].Split(':');
            Button energy = GetComponentInChildren<Button>();
            ButtonScript engScript = energy.GetComponent<ButtonScript>();
            engScript.SetTotalEnergy(currStat[1]);

            currStat = actStats[3].Split(':');
            if (ActionScript.CheckIfAttack(actName))
            {
                int finalDmg = (int.Parse(currStat[1]) + m_cScript.m_tempStats[(int)CharacterScript.sts.DMG]);

                if (ActionScript.UniqueActionProperties(m_cScript.m_currAction, ActionScript.uniAct.DMG_MOD) >= 0)
                    finalDmg += m_cScript.m_tempStats[(int)CharacterScript.sts.TEC];

                if (finalDmg > 0)
                    main.m_text[0].text = "DMG: " + finalDmg;
                else
                    main.m_text[0].text = "DMG: 0";
            }
            else
                main.m_text[0].text = "DMG: 0";

            currStat = actStats[4].Split(':');
            int finalRng = (int.Parse(currStat[1]) + m_cScript.m_tempStats[(int)CharacterScript.sts.RNG]);

            if (ActionScript.UniqueActionProperties(m_cScript.m_currAction, ActionScript.uniAct.RNG_MOD) >= 0)
                finalRng += m_cScript.m_tempStats[(int)CharacterScript.sts.TEC];

            if (finalRng > 0)
                main.m_text[1].text = "RNG: " + finalRng;
            else
                main.m_text[1].text = "RNG: 1";

            currStat = actStats[5].Split(':');
            int finalRad = (int.Parse(currStat[1]) + m_cScript.m_tempStats[(int)CharacterScript.sts.RAD]);

            if (ActionScript.UniqueActionProperties(m_cScript.m_currAction, ActionScript.uniAct.RAD_MOD) >= 0)
                finalRad += m_cScript.m_tempStats[(int)CharacterScript.sts.TEC];

            if (finalRad > 0)
                main.m_text[2].text = "RAD: " + finalRad;
            else
                main.m_text[2].text = "RAD: 0";

            currStat = actStats[6].Split(':');
            main.m_text[3].text = DatabaseScript.ModifyActions(m_cScript.m_tempStats[(int)CharacterScript.sts.TEC], currStat[1]);
        }
        else if (name == "Character Panel")
        {
            for (int i = 0; i < m_buttons.Length; i++)
                m_buttons[i].onClick.RemoveAllListeners();
            // REFACTOR: Move all these functions to populate Panel except random character
            TeamMenuScript menuScript = m_main.GetComponent<TeamMenuScript>();
            m_buttons[0].onClick.AddListener(() => menuScript.NewCharacter());
            m_buttons[1].onClick.AddListener(() => PanelManagerScript.GetPanel("Save/Load Panel").PopulatePanel());
            m_buttons[2].onClick.AddListener(() => PanelManagerScript.GetPanel("PresetSelect Panel").PopulatePanel());
            //m_buttons[3].onClick.AddListener(() => menuScript.RandomCharacter());
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
                    if (!m_slideScript.m_inView)
                        tMenu.m_currButton = currB;

                    m_cScript = PlayerPrefScript.LoadChar(currB.name, m_cScript);
                    tMenu.m_currCharScript = PlayerPrefScript.LoadChar(currB.name, tMenu.m_currCharScript);

                    // Fill out name
                    m_text[32].text = m_cScript.m_name;
                    GetComponentInChildren<InputField>().text = m_cScript.m_name;

                    m_cScript.m_exp = 10;
                    // Determine if select or remove will be visible
                    for (int i = 0; i < m_buttons.Length; i++)
                    {
                        if (m_buttons[i].name == "Level up" && m_cScript.m_exp >= 10 && !PanelManagerScript.GetPanel("New Action Panel").GetComponent<PanelSlideScript>().m_inView && !PanelManagerScript.GetPanel("New Status Panel").GetComponent<PanelSlideScript>().m_inView)
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
        else if (name == "DamagePreview")
        {
            BoardScript bScript = m_main.GetComponent<BoardScript>();
            CharacterScript currScript = bScript.m_currCharScript;

            int def = m_cScript.m_tempStats[(int)CharacterScript.sts.DEF];
            int dmg = int.Parse(DatabaseScript.GetActionData(currScript.m_currAction, DatabaseScript.actions.DMG)) + currScript.m_tempStats[(int)CharacterScript.sts.DMG];

            if (ActionScript.UniqueActionProperties(currScript.m_currAction, ActionScript.uniAct.BYPASS) >= 0 && def > 0)
            {
                def -= ActionScript.UniqueActionProperties(currScript.m_currAction, ActionScript.uniAct.BYPASS) + currScript.m_tempStats[(int)CharacterScript.sts.TEC];
                if (def < 0)
                    def = 0;
            }
            else if (ActionScript.UniqueActionProperties(currScript.m_currAction, ActionScript.uniAct.DMG_MOD) >= 0)
                dmg += currScript.m_tempStats[(int)CharacterScript.sts.TEC];

            dmg -= def;

            if (m_cScript.m_tempStats[(int)CharacterScript.sts.HP] >= m_cScript.m_tempStats[(int)CharacterScript.sts.HP] - dmg)
                m_text[0].text = /*"HP: " + */m_cScript.m_tempStats[(int)CharacterScript.sts.HP].ToString() + "->" + (m_cScript.m_tempStats[(int)CharacterScript.sts.HP] - dmg).ToString();
            else
                m_text[0].text = /*"HP: " + */m_cScript.m_tempStats[(int)CharacterScript.sts.HP].ToString() + "->" + m_cScript.m_tempStats[(int)CharacterScript.sts.HP].ToString();
        }
        else if (name == "HUD Panel LEFT" || name == "HUD Panel RIGHT")
        {
            m_panels[0].GetComponent<Image>().color = new Color(m_cScript.m_teamColor.r + 0.3f, m_cScript.m_teamColor.g + 0.3f, m_cScript.m_teamColor.b + 0.3f, 1);
            GetComponentsInChildren<Text>()[0].text = m_cScript.m_name;
            GetComponentsInChildren<Text>()[1].text = "HP: " + m_cScript.m_tempStats[(int)CharacterScript.sts.HP] + "/" + m_cScript.m_stats[(int)CharacterScript.sts.HP];

            string spacing = "  ";

            Image[] statSyms = m_panels[(int)HUDPan.PARA_PAN].m_images;

            for (int i = 1; i < 7; i++)
            {
                //if (m_cScript.m_tempStats[i].ToString().Length > 1 && m_cScript.m_tempStats[i] != -1)
                    GetComponentsInChildren<Text>()[i + 1].text = spacing + m_cScript.m_tempStats[i];
                //else
                //    GetComponentsInChildren<Text>()[i + 1].text = spacing + " " + m_cScript.m_tempStats[i];

                if (i == (int)CharacterScript.sts.SPD && m_cScript.m_tempStats[i] == 10 ||
                    i == (int)CharacterScript.sts.MOV && m_cScript.m_tempStats[i] == 5 ||
                    i != (int)CharacterScript.sts.SPD && i != (int)CharacterScript.sts.MOV && 
                    m_cScript.m_tempStats[i] == 0)
                    statSyms[i - 1].color = Color.white;
                else if (i == (int)CharacterScript.sts.SPD && m_cScript.m_tempStats[i] > 10 ||
                    i == (int)CharacterScript.sts.MOV && m_cScript.m_tempStats[i] > 5 ||
                    i != (int)CharacterScript.sts.SPD && i != (int)CharacterScript.sts.MOV &&
                    m_cScript.m_tempStats[i] > 0)
                    statSyms[i - 1].color = StatusScript.c_buffColor;
                else if (i == (int)CharacterScript.sts.SPD && m_cScript.m_tempStats[i] < 10 ||
                    i == (int)CharacterScript.sts.MOV && m_cScript.m_tempStats[i] < 5 ||
                    i != (int)CharacterScript.sts.SPD && i != (int)CharacterScript.sts.MOV &&
                    m_cScript.m_tempStats[i] < 0)
                    statSyms[i - 1].color = StatusScript.c_debuffColor;
            }

            if (GetComponentInChildren<Button>())
            {
                Button button = GetComponentInChildren<Button>();
                ButtonScript buttScript = button.GetComponent<ButtonScript>();
                buttScript.SetTotalEnergy(m_cScript.m_color);

                m_panels[(int)HUDPan.STS_BUTTS].m_cScript = m_cScript;
                m_panels[(int)HUDPan.STS_BUTTS].StatusSymbolSetup(true);
            }

            m_panels[(int)HUDPan.ACT_PAN].m_cScript = m_cScript;
            m_panels[(int)HUDPan.ACT_PAN].PopulatePanel();

            m_cScript.m_player.SetEnergyPanel(m_cScript);

            if (name == "HUD Panel LEFT")
            {
                m_panels[(int)HUDPan.MOV_PASS].m_buttons[0].GetComponent<ButtonScript>().m_object = m_cScript.gameObject;
                if (m_cScript.m_effects[(int)StatusScript.effects.IMMOBILE])
                {
                    m_panels[(int)HUDPan.MOV_PASS].m_buttons[0].GetComponent<Image>().color = b_isDisallowed;
                    m_panels[(int)HUDPan.MOV_PASS].m_buttons[0].interactable = false;
                }
                else if (m_cScript.m_hasActed[(int)CharacterScript.trn.ACT] == 2)
                    m_panels[(int)HUDPan.MOV_PASS].m_buttons[0].GetComponent<Image>().color = b_isHalf;
                else
                    m_panels[(int)HUDPan.MOV_PASS].m_buttons[0].GetComponent<Image>().color = Color.white;
            }
        }
        else if (name == "New Action Panel")
        {
            m_cScript = m_main.GetComponent<TeamMenuScript>().m_currCharScript;
            PanelManagerScript.m_locked = true;
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
            PanelManagerScript.m_locked = true;
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
                        Button[] childButtons = PanelManagerScript.m_confirmPanel.GetComponentsInChildren<Button>();
                        button[i * 4 + j].onClick.AddListener(() => InputManagerScript.ConfirmationButton(childButtons[1], "Save"));
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
            //StatusSymbolSetup();
            
        }
        else if (name == "Status Selector")
        {
            StatusSymbolSetup(false);
        }
        else if (name == "StatusViewer Panel")
        {
            if (m_cScript)
            {
                StatusScript[] statScripts = m_cScript.GetComponents<StatusScript>();

                m_panels[0].m_images[0].sprite = statScripts[m_cScript.m_currStatus].m_sprite;
                m_panels[0].m_images[0].color = statScripts[m_cScript.m_currStatus].m_color;

                m_panels[1].m_text[0].text = "Duration: " + statScripts[m_cScript.m_currStatus].m_lifeSpan.ToString();
                m_panels[1].m_text[1].text = statScripts[m_cScript.m_currStatus].m_effect;
            }
            else
            {
                PowerupScript pUP = m_main.GetComponent<BoardScript>().m_highlightedTile.m_holding.GetComponent<PowerupScript>();

                m_panels[0].m_images[0].sprite = pUP.m_sprite;
                m_panels[0].m_images[0].color = pUP.m_color;

                m_panels[1].m_text[0].text = "Duration: N/A";
                m_panels[1].m_text[1].text = pUP.m_effect;
            }
        }

        if (m_slideScript)
            m_slideScript.OpenPanel();
    }

    public void ActionsAvailable()
    {
        int activeCount = 0;
        for (int i = 0; i < m_cScript.m_actions.Length; i++)
        {
            string name = DatabaseScript.GetActionData(m_cScript.m_actions[i], DatabaseScript.actions.NAME);
            string eng = DatabaseScript.GetActionData(m_cScript.m_actions[i], DatabaseScript.actions.ENERGY);

            ButtonScript buttScript = m_buttons[i].GetComponent<ButtonScript>();
            buttScript.m_action = m_cScript.m_actions[i];
            buttScript.SetTotalEnergy(eng);
            buttScript.m_object = m_cScript.gameObject;
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
            if (m_cScript.m_effects[(int)StatusScript.effects.WARD] && ActionScript.CheckIfAttack(name) ||
                m_cScript.m_effects[(int)StatusScript.effects.DELAY] && PlayerScript.CheckIfGains(eng) && eng.Length >= 2 ||
                currCharActName == "SUP(Redirect)" && currCharScript != m_cScript && ActionScript.CheckIfAttack(name) ||
                currCharActName == "SUP(Redirect)" && currCharScript != m_cScript && eng.Length > 1 ||
                currCharActName == "ATK(Copy)" && currCharScript != m_cScript && !ActionScript.CheckIfAttack(name) ||
                currCharActName == "ATK(Copy)" && currCharScript != m_cScript && ActionScript.CheckIfAttack(name) && ActionScript.CheckActionLevel(eng) > 2 ||
                currCharActName == "ATK(Hack)" && currCharScript != m_cScript && PlayerScript.CheckIfGains(eng) ||
                m_cScript.m_effects[(int)StatusScript.effects.HINDER] && !ActionScript.CheckIfAttack(name) ||
                disabled)
                m_buttons[i].GetComponent<Image>().color = b_isDisallowed;
            else if (currCharActName == "SUP(Redirect)" && currCharScript != m_cScript && !ActionScript.CheckIfAttack(name) && eng.Length <= 2 ||
                currCharActName == "ATK(Copy)" && currCharScript != m_cScript && ActionScript.CheckIfAttack(name) && ActionScript.CheckActionLevel(eng) <= 2)
            {
                m_buttons[i].GetComponent<Image>().color = b_isFree;
                m_buttons[i].onClick.AddListener(() => ActionScript.ActionTargeting(currCharScript, currCharScript.m_tile));
                if (PanelManagerScript.GetPanel("Choose Panel").m_slideScript.m_inView == true)
                    m_buttons[i].interactable = true;
                else
                    m_buttons[i].interactable = false;
            }
            else if (currCharActName == "ATK(Hack)" && currCharScript != m_cScript && !PlayerScript.CheckIfGains(eng))
            {
                    m_buttons[i].GetComponent<Image>().color = b_isSpecial;
                    m_buttons[i].onClick.AddListener(() => ActionScript.DisableSelectedAction(m_cScript, ind));
                    m_buttons[i].interactable = true;
            }
            else
            {
                m_buttons[i].GetComponent<Image>().color = new Color(1, 1, 1, 1);
                if (m_cScript.m_player.CheckEnergy(eng) && m_cScript.m_hasActed[(int)CharacterScript.trn.ACT] < 3 && !PanelManagerScript.GetPanel("Choose Panel").m_slideScript.m_inView)
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
                if (m_cScript.m_effects[(int)StatusScript.effects.WARD] && ActionScript.CheckIfAttack(name[1]) ||
                    m_cScript.m_effects[(int)StatusScript.effects.DELAY] && PlayerScript.CheckIfGains(eng[1]) && eng[1].Length == 2 ||
                    currCharActName == "SUP(Redirect)" && currCharScript != m_cScript && ActionScript.CheckIfAttack(name[1]) ||
                    currCharActName == "SUP(Redirect)" && currCharScript != m_cScript && eng[1].Length > 2 ||
                    currCharActName == "ATK(Copy)" && currCharScript != m_cScript && !ActionScript.CheckIfAttack(name[1]) ||
                    currCharActName == "ATK(Copy)" && currCharScript != m_cScript && ActionScript.CheckIfAttack(name[1]) && ActionScript.CheckActionLevel(eng[1]) > currCharScript.m_tempStats[(int)CharacterScript.sts.TEC] ||
                    m_cScript.m_effects[(int)StatusScript.effects.HINDER] && !ActionScript.CheckIfAttack(name[1]) ||
                    disabled)
                    buttons[i].GetComponent<Image>().color = b_isDisallowed;
                else if (currCharActName == "SUP(Redirect)" && currCharScript != m_cScript && !ActionScript.CheckIfAttack(name[1]) && eng[1].Length <= 2 ||
                    currCharActName == "ATK(Copy)" && currCharScript != m_cScript && ActionScript.CheckIfAttack(name[1]) && ActionScript.CheckActionLevel(eng[1]) <= currCharScript.m_tempStats[(int)CharacterScript.sts.TEC])
                {
                    buttons[i].GetComponent<Image>().color = b_isFree;
                    buttons[i].onClick.AddListener(() => ActionScript.ActionTargeting(currCharScript, currCharScript.m_tile));
                    buttons[i].interactable = true;
                }
                else if (currCharActName == "ATK(Hack)" && currCharScript != m_cScript)
                {
                    if (!PlayerScript.CheckIfGains(eng[1]) && !disabled)
                    {
                        buttons[i].GetComponent<Image>().color = new Color(1, 1, 1, 1);
                        buttons[i].onClick.AddListener(() => ActionScript.DisableSelectedAction(m_cScript, ind));
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
                            buttons[i].onClick.AddListener(() => ActionScript.ActionTargeting(currCharScript, currCharScript.m_tile));
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
        for (int i = 0; i < m_buttons.Length; i++)
        {
            m_buttons[i].onClick.RemoveAllListeners();
            m_buttons[i].interactable = false;
            m_buttons[i].GetComponent<Image>().color = new Color(1, 1, 1, 1);
            if (m_buttons[i].GetComponent<PanelScript>())
                m_buttons[i].GetComponent<PanelScript>().m_slideScript.m_inView = false;
            Text t = m_buttons[i].GetComponentInChildren<Text>();
            t.text = "EMPTY";
            ButtonScript buttScript = m_buttons[i].GetComponent<ButtonScript>();

            for (int k = 0; k < buttScript.m_energyPanel.Length; k++)
                buttScript.m_energyPanel[k].SetActive(false);
        }
    }

    // Maybe move
    private void StatusSymbolSetup(bool _isHUD)
    {
        StatusScript[] statScripts = m_cScript.GetComponents<StatusScript>();

        for (int i = 0; i < 8; i++)
        {
            if (_isHUD)
                m_images[i].GetComponent<PanelScript>().m_slideScript.m_inView = false;
            else
                m_images[i].enabled = false;
        }

        for (int i = 0; i < statScripts.Length; i++)
        {
            if (statScripts[i].m_lifeSpan <= 0)
                continue;

            ButtonScript buttScript = m_images[i].GetComponentInChildren<ButtonScript>();
            buttScript.m_parent = this;
            if (_isHUD)
                m_images[i].GetComponent<PanelScript>().m_slideScript.m_inView = true;
            else
                m_images[i].enabled = true;
            m_images[i].name = i.ToString();
            m_images[i].sprite = statScripts[i].m_sprite;
            m_images[i].color = statScripts[i].m_color;
        }
    }
}