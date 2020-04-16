using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PanelScript : MonoBehaviour {

    static public Color b_isFree = new Color(.5f, 1, .5f, 1);
    static public Color b_isDisallowed = new Color(1, .5f, .5f, 1);
    static public Color b_isHalf = new Color(1, 1, .5f, 1);
    static public Color b_isSpecial = new Color(1, .2f, 1, 1);

    // References
    public CharacterScript m_cScript;
    public SlidingPanelManagerScript m_panMan;

    // Use this for initialization
    protected void Start()
    {
        if (GameObject.Find("Scene Manager"))
            m_panMan = GameObject.Find("Scene Manager").GetComponent<SlidingPanelManagerScript>();

        //m_audio = gameObject.AddComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // General Panel
    virtual public void PopulatePanel()
    {
        //AudioManagerScript.Play

        //if (name == "Character Panel")
        //{
        //    for (int i = 0; i < transform.childCount; i++)
        //        transform.GetChild(i).GetComponent<Button>().onClick.RemoveAllListeners();
        //    // REFACTOR: Move all these functions to populate Panel except random character
        //    TeamMenuScript menuScript = m_main.GetComponent<TeamMenuScript>();
        //    transform.Find("New").GetComponent<Button>().onClick.AddListener(() => menuScript.NewCharacter());
        //    transform.Find("Load").GetComponent<Button>().onClick.AddListener(() => m_panMan.GetPanel("Save/Load Panel").PopulatePanel());
        //    transform.Find("Preset").GetComponent<Button>().onClick.AddListener(() => m_panMan.GetPanel("PresetSelect Panel").PopulatePanel());
        //    //m_buttons[3].onClick.AddListener(() => menuScript.RandomCharacter());
        //}
        //if (name == "New Action Panel")
        //{
        //    m_cScript = m_main.GetComponent<TeamMenuScript>().m_currCharScript;
        //    m_panMan.m_locked = true;
        //    for (int i = 0; i < transform.Find("Action Buttons").childCount; i++)
        //    {
        //        //string action = m_main.GetComponent<TeamMenuScript>().NewRandomAction(m_buttons);
        //        //m_buttons[i].GetComponent<ButtonScript>().m_action = action;
        //        //m_buttons[i].GetComponentInChildren<Text>().text = DatabaseScript.GetActionData(action, DatabaseScript.actions.NAME);
        //        //m_buttons[i].GetComponent<ButtonScript>().SetTotalEnergy(DatabaseScript.GetActionData(action, DatabaseScript.actions.ENERGY));
        //        //
        //        //m_buttons[i].onClick.RemoveAllListeners();
        //        //m_buttons[i].onClick.AddListener(() => m_main.GetComponent<TeamMenuScript>().NewActionSelection());
        //    }
        //}
        //else if (name == "New Status Panel")
        //{
        //    m_panMan.m_locked = true;
        //    m_main.GetComponent<TeamMenuScript>().NewRandomStat(transform.Find("Status Buttons").GetComponentsInChildren<Button>());
        //}
        //else if (name == "PresetSelect Panel")
        //{
        //    DatabaseScript dbScript = m_main.GetComponent<DatabaseScript>();

        //    for (int i = 0; i < m_buttons.Length; i++)
        //    {
        //        Button butt = m_buttons[i];
        //        Text t = butt.GetComponentInChildren<Text>();
        //        t.text = dbScript.GetDataValue(dbScript.m_presets[i], "Name:");
        //        butt.name = i.ToString();
        //        butt.onClick.RemoveAllListeners();
        //        butt.onClick.AddListener(() => m_panels[0].GetComponent<PanelScript>().PopulatePanel());
        //        EnergyButtonScript buttScript = butt.GetComponent<EnergyButtonScript>();
        //        buttScript.SetTotalEnergy(dbScript.GetDataValue(dbScript.m_presets[i], "Colors:"));
        //    }
        //}
        if (name == "Round Panel")
        {
            BoardScript bScript = GameObject.Find("Board").GetComponent<BoardScript>();
            GetComponentInChildren<Text>().text = "Round: " + bScript.m_roundCount;
        }
        else if (name == "Save/Load Panel")
        {
            //if (EventSystem.current.currentSelectedGameObject.GetComponent<Button>().name == "Save Button")
            //    transform.Find("Text").GetComponent<Text>().text = "SAVE";
            //else if (EventSystem.current.currentSelectedGameObject.GetComponent<Button>().name == "Load")
            //    transform.Find("Text").GetComponent<Text>().text = "LOAD";
            //
            //Button[] button = m_buttons;
            //for (int i = 0; i < 4; i++)
            //{
            //    for (int j = 0; j < 9; j++)
            //    {
            //        string key = i.ToString() + ',' + j.ToString() + "SAVE" + ",name";
            //        string name = PlayerPrefs.GetString(key);
            //        if (name.Length > 0)
            //        {
            //            key = i.ToString() + ',' + j.ToString() + "SAVE" + ",color";
            //            string color = PlayerPrefs.GetString(key);
            //
            //            m_main.GetComponent<TeamMenuScript>().SetCharSlot(button[i * 4 + j], name, color);
            //        }
            //        button[i * 4 + j].onClick.RemoveAllListeners();
            //        if (EventSystem.current.currentSelectedGameObject.GetComponent<Button>().name == "Save Button")
            //        {
            //            Button[] childButtons = m_panMan.m_confirmPanel.GetComponentsInChildren<Button>();
            //            button[i * 4 + j].onClick.AddListener(() => m_panMan.GetPanel("Confirmation Panel").GetComponent<ConfirmationPanelScript>().ConfirmationButton("Save"));
            //        }
            //        else if (EventSystem.current.currentSelectedGameObject.GetComponent<Button>().name == "Load")
            //            button[i * 4 + j].onClick.AddListener(() => m_main.GetComponent<TeamMenuScript>().Load());
            //    }
            //}
        }
        //else if (name == "Status Panel")
        //{
        //    if (m_main && m_main.name == "CharacterViewer Panel")
        //        transform.Find("Name").GetComponent<Text>().text = "";
        //    else
        //        transform.Find("Name").GetComponent<Text>().text = m_cScript.m_name;
        //
        //    //if (m_cScript.m_gender == 0)
        //    //    m_images[8].sprite = Resources.Load<Sprite>("Symbols/Male Symbol");
        //    //else if (m_cScript.m_gender == 1)
        //    //    m_images[8].sprite = Resources.Load<Sprite>("Symbols/Female Symbol");
        //
        //    transform.Find("HP").GetComponent<Text>().text = "HP: " + m_cScript.m_tempStats[(int)CharacterScript.sts.HP] + "/" + m_cScript.m_stats[(int)CharacterScript.sts.HP];
        //    transform.Find("SPD").GetComponent<Text>().text = "SPD: " + m_cScript.m_tempStats[(int)CharacterScript.sts.SPD];
        //    transform.Find("DMG").GetComponent<Text>().text = "DMG: " + m_cScript.m_tempStats[(int)CharacterScript.sts.DMG];
        //    transform.Find("DEF").GetComponent<Text>().text = "DEF: " + m_cScript.m_tempStats[(int)CharacterScript.sts.DEF];
        //    transform.Find("MOV").GetComponent<Text>().text = "MOV: " + m_cScript.m_tempStats[(int)CharacterScript.sts.MOV];
        //    transform.Find("RNG").GetComponent<Text>().text = "RNG: " + m_cScript.m_tempStats[(int)CharacterScript.sts.RNG];
        //    transform.Find("TEC").GetComponent<Text>().text = "TEC: " + m_cScript.m_tempStats[(int)CharacterScript.sts.TEC];
        //
        //    //if (m_cScript.m_accessories[0] != null)
        //    //    m_text[8].text = "ACC: " + m_cScript.m_accessories[0];
        //    //if (m_cScript.m_accessories[1] != null)
        //    //    m_text[9].text = "ACC: " + m_cScript.m_accessories[1];
        //
        //    //transform.Find().GetComponent<EnergyButtonScript>().SetTotalEnergy(m_cScript.m_color);
        //    //StatusSymbolSetup();
        //    
        //}

        if (GetComponent<SlidingPanelScript>())
            GetComponent<SlidingPanelScript>().OpenPanel();
    }
}