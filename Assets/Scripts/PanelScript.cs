using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PanelScript : MonoBehaviour {

    public enum butts { MOV_BUTT, ACT_BUTT, PASS_BUTT, STATUS_BUTT }
    public enum dir { UP, DOWN, LEFT, RIGHT, NULL}

    public bool m_inView;
    public dir m_direction;
    public int m_boundryDis;
    public GameObject m_character;
    public CharacterScript m_cScript;
    public GameObject m_main;
    public GameObject m_parent;
    public Button[] m_buttons;
    public GameObject[] m_panels;
    public Text[] m_text;

    // Use this for initialization
    void Start ()
    {

        m_buttons = GetComponentsInChildren<Button>();

        if (m_boundryDis == 0)
        {
            if (m_direction == dir.UP)
                m_boundryDis = 180;
            else if (m_direction == dir.RIGHT)
                m_boundryDis = 470;
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
    }
	
	// Update is called once per frame
	void Update ()
    {
        RectTransform recTrans = GetComponent<RectTransform>();

        if (Input.GetMouseButtonDown(1) && m_inView)
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

    // MainPanel
    public void SetButtons()
    {
        for (int i = 0; i < m_buttons.Length; i++)
            m_buttons[i].onClick.RemoveAllListeners();

        if (name == "Main Panel")
        {
            m_buttons[(int)butts.MOV_BUTT].onClick.AddListener(() => m_cScript.MovementSelection());
            m_buttons[(int)butts.ACT_BUTT].onClick.AddListener(() => m_cScript.ActionSelection());
            m_buttons[(int)butts.PASS_BUTT].onClick.AddListener(() => m_cScript.Pass());
            m_buttons[(int)butts.STATUS_BUTT].onClick.AddListener(() => m_cScript.Status());
        }
        else if (name == "Character Panel")
        {
            MenuScript menuScript = m_main.GetComponent<MenuScript>();
            m_buttons[0].onClick.AddListener(() => menuScript.NewCharacter());
            m_buttons[1].onClick.AddListener(() => menuScript.LoadCharacter());
            m_buttons[2].onClick.AddListener(() => menuScript.PresetCharacter());
            m_buttons[2].onClick.AddListener(() => menuScript.RandomCharacter());
        }
        else if (name == "Auxiliary Panel")
            m_buttons[0].onClick.AddListener(() => m_cScript.Status());
    }

    // Action Panel
    public void PopulateActionButtons(string[] actions)
    {
        ResetButtons();

        for (int i = 0; i < actions.Length; i++)
        {
            string[] actsSeparated = actions[i].Split('|');
            string[] eng = actsSeparated[2].Split(':');

            if (eng[1][0] == 'g' || eng[1][0] == 'r' || eng[1][0] == 'w' || eng[1][0] == 'b')
                FirstActionAvailable(m_panels[0], actions[i]);
            else if (eng[1].Length == 1)
                FirstActionAvailable(m_panels[1], actions[i]);
            else if (eng[1].Length == 2)
                FirstActionAvailable(m_panels[2], actions[i]);
            else if (eng[1].Length == 3)
                FirstActionAvailable(m_panels[3], actions[i]);
        }
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
            string[] name = actsSeparated[1].Split(':');
            string[] eng = actsSeparated[2].Split(':');

            t.text = name[1];
            buttScript.SetTotalEnergy(eng[1]);

            PlayerScript playScript = m_cScript.m_player.GetComponent<PlayerScript>();
            if (playScript.CheckEnergy(eng[1]))
            {
                if (m_cScript)
                    buttons[i].onClick.AddListener(() => m_cScript.ActionTargeting());
                buttons[i].interactable = true;
            }

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
                Text t = buttons[j].GetComponentInChildren<Text>();
                t.text = "EMPTY";
                ButtonScript buttScript = buttons[j].GetComponent<ButtonScript>();

                for (int k = 0; k < buttScript.m_energyPanel.Length; k++)
                    buttScript.m_energyPanel[k].SetActive(false);
            }
        }
    }

    // General Panel
    public void PopulateText()
    {
        if (name == "Round Panel")
        {
            BoardScript bScript = m_main.GetComponent<BoardScript>();
            if (m_text != null)
                m_text[0].text = "Round: " + bScript.m_roundCount;
        }
        else if (name == "Status Panel")
        {
            m_text[(int)CharacterScript.sts.HP].text = "HP: " + m_cScript.m_tempStats[(int)CharacterScript.sts.HP] + "/" + m_cScript.m_stats[(int)CharacterScript.sts.HP];
            m_text[(int)CharacterScript.sts.SPD].text = "SPD: " + m_cScript.m_stats[(int)CharacterScript.sts.SPD];
            m_text[(int)CharacterScript.sts.HIT].text = "HIT: " + m_cScript.m_stats[(int)CharacterScript.sts.HIT];
            m_text[(int)CharacterScript.sts.EVA].text = "EVA: " + m_cScript.m_stats[(int)CharacterScript.sts.EVA];
            m_text[(int)CharacterScript.sts.CRT].text = "CRT: " + m_cScript.m_stats[(int)CharacterScript.sts.CRT];
            m_text[(int)CharacterScript.sts.DMG].text = "DMG: " + m_cScript.m_stats[(int)CharacterScript.sts.DMG];
            m_text[(int)CharacterScript.sts.DEF].text = "DEF: " + m_cScript.m_stats[(int)CharacterScript.sts.DEF];
            m_text[(int)CharacterScript.sts.MOV].text = "MOV: " + m_cScript.m_stats[(int)CharacterScript.sts.MOV];
            m_text[(int)CharacterScript.sts.RNG].text = "RNG: " + m_cScript.m_stats[(int)CharacterScript.sts.RNG];
            if (m_cScript.m_accessories[0] != null)
                m_text[9].text = "ACC: " + m_cScript.m_accessories[0];
            if (m_cScript.m_accessories[1] != null)
                m_text[10].text = "ACC: " + m_cScript.m_accessories[1];
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

            currStat = actStats[6].Split(':');
            m_text[5].text = "CRT: " + currStat[1];
            if (m_cScript.m_stats[(int)CharacterScript.sts.CRT] != 0)
                m_text[5].text += " + " + m_cScript.m_stats[(int)CharacterScript.sts.CRT];

            currStat = actStats[7].Split(':');
            m_text[6].text = currStat[1];
        }

        else if (name == "HUD Panel LEFT" || name == "HUD Panel RIGHT")
        {
            m_text[0].text = m_cScript.name;
            m_text[1].text = "HP: " + m_cScript.m_tempStats[(int)CharacterScript.sts.HP] + "/" + m_cScript.m_stats[(int)CharacterScript.sts.HP];
            
            //Image[] colorsUsed = panels[1].GetComponentsInChildren<Image>();

            //for (int i = 0; i < 4; i++)
            //    colorsUsed[i].gameObject.SetActive(cScript.colorsUsed[i]);
            //
            //Image[] colorsGained = panels[2].GetComponentsInChildren<Image>();
            //
            //for (int i = 0; i < 4; i++)
            //    colorsGained[i].gameObject.SetActive(cScript.colorsGained[i]);
        }
    }

    private void Slide()
    {
        RectTransform recTrans = GetComponent<RectTransform>();

        if (m_parent)
        {
            PanelScript parentScript = m_parent.GetComponent<PanelScript>();
            m_character = parentScript.m_character;
        }

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

                if (pan.activeSelf && panRect.offsetMax.y > 180 + height * i - height * (bScript.m_characters.Count - bScript.m_currRound.Count))
                    pan.transform.SetPositionAndRotation(new Vector3(pan.transform.position.x, pan.transform.position.y - 10.0f, pan.transform.position.z), pan.transform.rotation);

                if (pan.activeSelf && panRect.offsetMax.x < 45)
                    pan.transform.SetPositionAndRotation(new Vector3(pan.transform.position.x + 10.0f, pan.transform.position.y, pan.transform.position.z), pan.transform.rotation);
            }
        }
    }
}
