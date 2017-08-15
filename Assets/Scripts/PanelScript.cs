using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PanelScript : MonoBehaviour {

    public enum butts { MOV_BUTT, ACT_BUTT, PASS_BUTT, STATUS_BUTT }
    public enum dir { UP, DOWN, LEFT, RIGHT, NULL}

    static public Color b_isFree = new Color(.5f, 1, .5f, 1);
    static public Color b_isDisallowed = new Color(1, .5f, .5f, 1);

    public bool m_inView;
    public dir m_direction;
    public int m_boundryDis;
    public CharacterScript m_cScript;
    public GameObject m_main;
    public GameObject m_parent;
    public Button[] m_buttons;
    public GameObject[] m_panels;
    public Text[] m_text;
    public Image[] m_images;

    // Use this for initialization
    void Start ()
    {

        m_buttons = GetComponentsInChildren<Button>();

        if (m_boundryDis == 0)
        {
            if (m_direction == dir.UP)
                m_boundryDis = 180;
            else if (m_direction == dir.RIGHT)
                m_boundryDis = 480;
            else if (m_direction == dir.LEFT)
                m_boundryDis = -571;
            else if (m_direction == dir.DOWN)
                m_boundryDis = -180;
        }

        if (GetComponentsInChildren<Text>().Length > 0)
            m_text = GetComponentsInChildren<Text>();

        if (name == "Turn Panel")
            for (int i = 0; i < m_panels.Length; i++)
                m_panels[i].SetActive(false);

        //BPlayerPrefs.DeleteAll();
    }
	
	// Update is called once per frame
	void Update ()
    {
        RectTransform recTrans = GetComponent<RectTransform>();

        if (Input.GetMouseButtonDown(1) && m_inView && gameObject.tag != "Selector")
        {
            if (m_direction == dir.UP && recTrans.offsetMax.y < 200)
            {
                //PanelScript hudPanScript = hudPanel.GetComponent<PanelScript>();
                //hudPanScript.inView = false;
                m_inView = false;
          
                if (m_cScript && m_cScript.m_boardScript)
                    m_cScript.m_boardScript.m_selected = null;
            }
            if (m_parent && recTrans.offsetMax.y < 200)
            {
                PanelScript parentScript = m_parent.GetComponent<PanelScript>();
                parentScript.m_inView = true;
                m_inView = false;
            }
        }

        Slide();

        if (name == "Turn Panel")
            TurnSlide();
    }

    public void SetButtons()
    {
        for (int i = 0; i < m_buttons.Length; i++)
            m_buttons[i].onClick.RemoveAllListeners();

        if (name == "Main Panel")
        {
            if (!m_cScript.m_effects[(int)StatusScript.effects.IMMOBILE])
            {
                if (m_buttons[(int)butts.ACT_BUTT].GetComponent<Image>().color == Color.yellow)
                    m_buttons[(int)butts.MOV_BUTT].GetComponent<Image>().color = Color.yellow;

                if (m_buttons[(int)butts.MOV_BUTT].interactable == true)
                    m_buttons[(int)butts.MOV_BUTT].interactable = true;
                else
                    m_buttons[(int)butts.MOV_BUTT].interactable = false;
                m_buttons[(int)butts.MOV_BUTT].onClick.AddListener(() => m_cScript.MovementSelection(0));
            }
            else
            {
                m_buttons[(int)butts.MOV_BUTT].GetComponent<Image>().color = new Color(1, .5f, .5f, 1);
                m_buttons[(int)butts.MOV_BUTT].interactable = false;
            }
            m_buttons[(int)butts.ACT_BUTT].onClick.AddListener(() => m_cScript.ActionSelection());
            m_buttons[(int)butts.PASS_BUTT].onClick.AddListener(() => m_cScript.Pass());
            m_buttons[(int)butts.STATUS_BUTT].onClick.AddListener(() => m_cScript.ViewStatus());
        }
        else if (name == "Character Panel")
        {
            TeamMenuScript menuScript = m_main.GetComponent<TeamMenuScript>();
            m_buttons[0].onClick.AddListener(() => menuScript.NewCharacter());
            m_buttons[1].onClick.AddListener(() => menuScript.LoadCharacter());
            m_buttons[2].onClick.AddListener(() => menuScript.PresetCharacter());
            m_buttons[3].onClick.AddListener(() => menuScript.RandomCharacter());
        }
        else if (name == "Auxiliary Panel")
            m_buttons[0].onClick.AddListener(() => m_cScript.ViewStatus());
    }

    // General Panel
    public void PopulatePanel()
    {
        if (name == "Round Panel")
        {
            BoardScript bScript = m_main.GetComponent<BoardScript>();
            if (m_text != null)
                m_text[0].text = "Round: " + bScript.m_roundCount;
        }
        else if (name == "Action Panel")
        {
            ResetButtons();

            for (int i = 0; i < m_cScript.m_actions.Length; i++)
            {
                string[] actsSeparated = m_cScript.m_actions[i].Split('|');
                string[] eng = actsSeparated[(int)DatabaseScript.actions.ENERGY].Split(':');

                if (eng[1][0] == 'g' || eng[1][0] == 'r' || eng[1][0] == 'w' || eng[1][0] == 'b')
                    FirstActionAvailable(m_panels[0], m_cScript.m_actions[i]);
                else if (eng[1].Length == 1)
                    FirstActionAvailable(m_panels[1], m_cScript.m_actions[i]);
                else if (eng[1].Length == 2)
                    FirstActionAvailable(m_panels[2], m_cScript.m_actions[i]);
                else if (eng[1].Length == 3)
                    FirstActionAvailable(m_panels[3], m_cScript.m_actions[i]);
            }
        }
        else if (name == "Status Panel")
        {
            m_text[(int)CharacterScript.sts.HP].text = "HP: " + m_cScript.m_tempStats[(int)CharacterScript.sts.HP] + "/" + m_cScript.m_stats[(int)CharacterScript.sts.HP];
            m_text[(int)CharacterScript.sts.SPD].text = "SPD: " + m_cScript.m_tempStats[(int)CharacterScript.sts.SPD];

            m_text[(int)CharacterScript.sts.HIT].text = "HIT: " + m_cScript.m_stats[(int)CharacterScript.sts.HIT];
            CheckIfModded((int)CharacterScript.sts.HIT, m_cScript.m_stats[(int)CharacterScript.sts.HIT], m_cScript.m_tempStats[(int)CharacterScript.sts.HIT]);

            m_text[(int)CharacterScript.sts.EVA].text = "EVA: " + m_cScript.m_stats[(int)CharacterScript.sts.EVA];
            CheckIfModded((int)CharacterScript.sts.EVA, m_cScript.m_stats[(int)CharacterScript.sts.EVA], m_cScript.m_tempStats[(int)CharacterScript.sts.EVA]);

            m_text[(int)CharacterScript.sts.CRT].text = "CRT: " + m_cScript.m_stats[(int)CharacterScript.sts.CRT];
            CheckIfModded((int)CharacterScript.sts.CRT, m_cScript.m_stats[(int)CharacterScript.sts.CRT], m_cScript.m_tempStats[(int)CharacterScript.sts.CRT]);

            m_text[(int)CharacterScript.sts.DMG].text = "DMG: " + m_cScript.m_stats[(int)CharacterScript.sts.DMG];
            CheckIfModded((int)CharacterScript.sts.DMG, m_cScript.m_stats[(int)CharacterScript.sts.DMG], m_cScript.m_tempStats[(int)CharacterScript.sts.DMG]);

            m_text[(int)CharacterScript.sts.DEF].text = "DEF: " + m_cScript.m_stats[(int)CharacterScript.sts.DEF];
            CheckIfModded((int)CharacterScript.sts.DEF, m_cScript.m_stats[(int)CharacterScript.sts.DEF], m_cScript.m_tempStats[(int)CharacterScript.sts.DEF]);

            m_text[(int)CharacterScript.sts.MOV].text = "MOV: " + m_cScript.m_stats[(int)CharacterScript.sts.MOV];
            CheckIfModded((int)CharacterScript.sts.MOV, m_cScript.m_stats[(int)CharacterScript.sts.MOV], m_cScript.m_tempStats[(int)CharacterScript.sts.MOV]);

            m_text[(int)CharacterScript.sts.RNG].text = "RNG: " + m_cScript.m_stats[(int)CharacterScript.sts.RNG];
            CheckIfModded((int)CharacterScript.sts.RNG, m_cScript.m_stats[(int)CharacterScript.sts.RNG], m_cScript.m_tempStats[(int)CharacterScript.sts.RNG]);

            if (m_cScript.m_accessories[0] != null)
                m_text[9].text = "ACC: " + m_cScript.m_accessories[0];
            if (m_cScript.m_accessories[1] != null)
                m_text[10].text = "ACC: " + m_cScript.m_accessories[1];

            StatusSymbolSetup();
        }
        else if (name == "ActionViewer Panel")
        {
            string[] actStats = m_cScript.m_currAction.Split('|');
            string[] currStat = actStats[1].Split(':');
            m_text[0].text = currStat[1];

            currStat = actStats[2].Split(':');
            Button energy = GetComponentInChildren<Button>();
            ButtonScript engScript = energy.GetComponent<ButtonScript>();
            engScript.SetTotalEnergy(currStat[1]);

            currStat = actStats[3].Split(':');
            m_text[2].text = "HIT: " + currStat[1];
            if (m_cScript.m_stats[(int)CharacterScript.sts.HIT] != 0)
                m_text[2].text += " + " + m_cScript.m_stats[(int)CharacterScript.sts.HIT];

            currStat = actStats[4].Split(':');
            m_text[3].text = "DMG: " + currStat[1];
            if (m_cScript.m_stats[(int)CharacterScript.sts.DMG] != 0)
                m_text[3].text += " + " + m_cScript.m_stats[(int)CharacterScript.sts.DMG];

            currStat = actStats[5].Split(':');
            m_text[4].text = "RNG: " + currStat[1];
            if (m_cScript.m_stats[(int)CharacterScript.sts.RNG] != 0)
                m_text[4].text += "+" + m_cScript.m_stats[(int)CharacterScript.sts.RNG];

            currStat = actStats[7].Split(':');
            m_text[5].text = "CRT: " + currStat[1];
            if (m_cScript.m_stats[(int)CharacterScript.sts.CRT] != 0)
                m_text[5].text += " + " + m_cScript.m_stats[(int)CharacterScript.sts.CRT];

            currStat = actStats[8].Split(':');
            m_text[6].text = currStat[1];

            currStat = actStats[6].Split(':');
            m_text[7].text = "RAD: " + currStat[1];
        }
        else if (name == "HUD Panel LEFT" || name == "HUD Panel RIGHT")
        {
            m_panels[0].GetComponent<Image>().color = new Color(m_cScript.m_teamColor.r + 0.3f, m_cScript.m_teamColor.g + 0.3f, m_cScript.m_teamColor.b + 0.3f, 1);
            m_text[0].text = m_cScript.name;
            m_text[1].text = "HP: " + m_cScript.m_tempStats[(int)CharacterScript.sts.HP] + "/" + m_cScript.m_stats[(int)CharacterScript.sts.HP];
        }
        else if (name == "StatusViewer Panel")
        {
            StatusScript[] statScripts = m_cScript.GetComponents<StatusScript>();
            
            m_images[0].sprite = statScripts[m_cScript.m_currStatus].m_sprite;
            m_images[0].color = statScripts[m_cScript.m_currStatus].m_color;

            m_text[0].text = "Duration: " + statScripts[m_cScript.m_currStatus].m_lifeSpan.ToString();
            m_text[1].text = statScripts[m_cScript.m_currStatus].m_effect;

        }
        else if (name == "Status Selector")
            StatusSymbolSetup();
        else if (name == "ActionPreview")
        {
            BoardScript bScript = m_main.GetComponent<BoardScript>();
            CharacterScript currScript = bScript.m_currPlayer.GetComponent<CharacterScript>();
            m_panels[0].GetComponent<Image>().color = new Color(currScript.m_teamColor.r + 0.3f, currScript.m_teamColor.g + 0.3f, currScript.m_teamColor.b + 0.3f, 1);
            m_panels[1].GetComponent<Image>().color = new Color(m_cScript.m_teamColor.r + 0.3f, m_cScript.m_teamColor.g + 0.3f, m_cScript.m_teamColor.b + 0.3f, 1);
            int hit = int.Parse(DatabaseScript.GetActionData(currScript.m_currAction, DatabaseScript.actions.HIT)) * 5;
            int dmg = int.Parse(DatabaseScript.GetActionData(currScript.m_currAction, DatabaseScript.actions.DMG)) + currScript.m_tempStats[(int)CharacterScript.sts.DMG];
            m_text[0].text = "Hit %: " + (100 - hit + (currScript.m_tempStats[(int)CharacterScript.sts.HIT] * 5) - (m_cScript.m_tempStats[(int)CharacterScript.sts.EVA] * 5)).ToString();
            m_text[1].text = "HP: " + m_cScript.m_tempStats[(int)CharacterScript.sts.HP].ToString() + " -> " + (m_cScript.m_tempStats[(int)CharacterScript.sts.HP] - dmg).ToString();
        }
    }

    private void CheckIfModded(int _ind, int _origStat, int _tempStat)
    {
        if (_tempStat > _origStat)
            m_text[_ind].text += " +" + (_tempStat - _origStat).ToString();
        else if (_tempStat < _origStat)
            m_text[_ind].text += " -" + (_tempStat - _origStat).ToString();
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
                CharacterScript currCharScript = m_main.GetComponent<BoardScript>().m_currPlayer.GetComponent<CharacterScript>();
                string[] currCharAct = currCharScript.m_currAction.Split('|');
                string currCharActName = currCharAct[0];
                if (currCharScript.m_currAction.Length > 0)
                {
                    currCharAct = currCharAct[1].Split(':');
                    currCharActName = currCharAct[1];
                }

                // ACTION PREVENTION
                // REFACTOR
                if (m_cScript.m_effects[(int)StatusScript.effects.WARD] && CharacterScript.CheckIfAttack(name[1]) ||
                    m_cScript.m_effects[(int)StatusScript.effects.DELAY] && PlayerScript.CheckIfGains(eng[1]) && eng[1].Length == 2 ||
                    currCharActName == "Redirect ATK" && currCharScript != m_cScript && CharacterScript.CheckIfAttack(name[1]) ||
                    currCharActName == "Redirect ATK" && currCharScript != m_cScript && eng[1].Length > 2 ||
                    currCharActName == "Copy ATK" && currCharScript != m_cScript && !CharacterScript.CheckIfAttack(name[1]) ||
                    currCharActName == "Copy ATK" && currCharScript != m_cScript && CharacterScript.CheckIfAttack(name[1]) && eng[1].Length > 2)
                    buttons[i].GetComponent<Image>().color = b_isDisallowed;
                else if (currCharActName == "Redirect ATK" && currCharScript != m_cScript && !CharacterScript.CheckIfAttack(name[1]) && eng[1].Length <= 2 ||
                    currCharActName == "Copy ATK" && currCharScript != m_cScript && CharacterScript.CheckIfAttack(name[1]) && eng[1].Length <= 2)
                {
                    buttons[i].GetComponent<Image>().color = b_isFree;
                    buttons[i].onClick.AddListener(() => currCharScript.ActionTargeting());
                    buttons[i].interactable = true;
                }
                else
                {
                    buttons[i].GetComponent<Image>().color = new Color(1, 1, 1, 1);
                    PlayerScript playScript = m_cScript.m_player.GetComponent<PlayerScript>();
                    if (playScript.CheckEnergy(eng[1]))
                    {
                        if (m_cScript)
                            buttons[i].onClick.AddListener(() => currCharScript.ActionTargeting());
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
                if (recTrans.offsetMax.y >  m_boundryDis)
                    transform.SetPositionAndRotation(new Vector3(transform.position.x, transform.position.y - 25.0f, transform.position.z), transform.rotation);
            }
            else
                if (recTrans.offsetMax.y < 750)
                    transform.SetPositionAndRotation(new Vector3(transform.position.x, transform.position.y + 25.0f, transform.position.z), transform.rotation);
        }
        else if (m_direction == dir.RIGHT)
        {
            if (m_inView)
            {
                if (recTrans.offsetMax.x > m_boundryDis)
                    transform.SetPositionAndRotation(new Vector3(transform.position.x - 25.0f, transform.position.y, transform.position.z), transform.rotation);
            }
            else
                if (recTrans.offsetMax.x < 877)
                    transform.SetPositionAndRotation(new Vector3(transform.position.x + 25.0f, transform.position.y, transform.position.z), transform.rotation);
        }
        else if (m_direction == dir.LEFT)
        {
            if (m_inView)
            {
                if (recTrans.offsetMax.x > m_boundryDis)
                    transform.SetPositionAndRotation(new Vector3(transform.position.x - 25.0f, transform.position.y, transform.position.z), transform.rotation);
            }
            else
                if (recTrans.offsetMax.x < -877)
                    transform.SetPositionAndRotation(new Vector3(transform.position.x + 25.0f, transform.position.y, transform.position.z), transform.rotation);
        }
        else if (m_direction == dir.DOWN)
        {
            if (m_inView)
            {
                if (recTrans.offsetMax.y < m_boundryDis)
                    transform.SetPositionAndRotation(new Vector3(transform.position.x, transform.position.y + 25.0f, transform.position.z), transform.rotation);
            }
            else
                if (recTrans.offsetMax.y > -500)
                    transform.SetPositionAndRotation(new Vector3(transform.position.x, transform.position.y - 25.0f, transform.position.z), transform.rotation);
        }
    }
    
    // Turn Panel
    private void TurnSlide()
    {
        BoardScript bScript = m_main.GetComponent<BoardScript>();
        GameObject pan = null;
        int count = 0; // Count is used to determine how much to lower each panel

        // Set current panel and count actives
        for (int i = 0; i < m_panels.Length; i++)
        {
            if (m_panels[i].activeSelf)
            {
                if (pan == null)
                    pan = m_panels[i];
                count++;
            }
        }

        // If there are no actives, reset panels
        if (!pan)
        {
            count = bScript.m_currRound.Count;

            for (int i = 0; i < count; i++)
            {
                m_panels[i].SetActive(true);
                m_panels[i].transform.SetPositionAndRotation(new Vector3(m_panels[i].transform.position.x, 166 + 61.2f * i, m_panels[i].transform.position.z), m_panels[i].transform.rotation);
                CharacterScript charScript = bScript.m_currRound[i].GetComponent<CharacterScript>();
                charScript.m_turnPanel = m_panels[i];

                if (charScript.m_effects[(int)StatusScript.effects.STUN] && i == 0)
                    m_panels[i].GetComponent<Image>().color = new Color(1, .5f, .5f, 1);
                else
                    m_panels[i].GetComponent<Image>().color = new Color(charScript.m_teamColor.r + 0.3f, charScript.m_teamColor.g + 0.3f, charScript.m_teamColor.b + 0.3f, 1);

                Text t = m_panels[i].GetComponentInChildren<Text>();
                t.text = bScript.m_currRound[i].name;

                if (m_panels[i].GetComponentInChildren<Button>())
                {
                    Button button = m_panels[i].GetComponentInChildren<Button>();
                    ButtonScript buttScript = button.GetComponent<ButtonScript>();
                    buttScript.SetTotalEnergy(charScript.m_color);
                    buttScript.m_character = bScript.m_currRound[i];
                }
            }
            pan = m_panels[0];
        }

        RectTransform panRect = pan.GetComponent<RectTransform>();

        if (count != bScript.m_currRound.Count) // if one of the panels is removed, move it every frame unitl it slides off to the left
        {
            if (panRect.offsetMax.x > -200)
                pan.transform.SetPositionAndRotation(new Vector3(pan.transform.position.x - 10.0f, pan.transform.position.y, pan.transform.position.z), pan.transform.rotation);
            else if (panRect.offsetMax.x <= -200)
                pan.SetActive(false);
        }
        else
        {
            // if was removed, check all of the panels that are still left to see if they are in their new position. If not, slide them down each frame until they are
            for (int i = 0; i < m_panels.Length; i++)
            {
                pan = m_panels[i];
                panRect = pan.GetComponent<RectTransform>();
                float height = 61.2f; // The differnce in y each panel is

                if (pan.activeSelf && panRect.offsetMax.y > 180 + height * i - height * (bScript.m_livingPlayersInRound - bScript.m_currRound.Count))
                    pan.transform.SetPositionAndRotation(new Vector3(pan.transform.position.x, pan.transform.position.y - 10.0f, pan.transform.position.z), pan.transform.rotation);

                if (pan.activeSelf && panRect.offsetMax.x < 45)
                    pan.transform.SetPositionAndRotation(new Vector3(pan.transform.position.x + 10.0f, pan.transform.position.y, pan.transform.position.z), pan.transform.rotation);
            }
        }
    }

    public void PopulateHUD()
    {
        PopulatePanel();

        if (GetComponentInChildren<Button>())
        {
            Button button = GetComponentInChildren<Button>();
            ButtonScript buttScript = button.GetComponent<ButtonScript>();
            buttScript.SetTotalEnergy(m_cScript.m_color);

            StatusSymbolSetup();
        }
    }

    private void StatusSymbolSetup()
    {
        StatusScript[] statScripts = m_cScript.GetComponents<StatusScript>();

        for (int i = 0; i < m_images.Length; i++)
            m_images[i].enabled = false;

        for (int i = 0; i < statScripts.Length; i++)
        {
            m_images[i].enabled = true;
            ButtonScript buttScript = m_images[i].GetComponentInChildren<ButtonScript>();
            buttScript.m_parent = this;
            m_images[i].name = i.ToString();
            m_images[i].sprite = statScripts[i].m_sprite;
            m_images[i].color = statScripts[i].m_color;
        }
    }
}
