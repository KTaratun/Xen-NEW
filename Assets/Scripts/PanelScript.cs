using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PanelScript : MonoBehaviour {

    public enum butts { MOV_BUTT, ACT_BUTT, PASS_BUTT, STATUS_BUTT }
    public enum dir { UP, DOWN, LEFT, RIGHT, NULL}

    public bool inView;
    public dir direction;
    public int boundryDis;
    public GameObject character;
    public CharacterScript cScript;
    public GameObject main;
    public GameObject parent;
    public Button[] buttons;
    public GameObject[] panels;
    public Text[] text;

    // Use this for initialization
    void Start ()
    {

        buttons = GetComponentsInChildren<Button>();

        if (boundryDis == 0)
        {
            if (direction == dir.UP)
                boundryDis = 180;
            else if (direction == dir.RIGHT)
                boundryDis = 470;
            else if (direction == dir.LEFT)
                boundryDis = -571;
            else if (direction == dir.DOWN)
                boundryDis = -180;
        }

        if (GetComponentsInChildren<Text>().Length > 0)
            text = GetComponentsInChildren<Text>();

        if (name == "Turn Panel")
            for (int i = 0; i < panels.Length; i++)
                panels[i].SetActive(false);
    }
	
	// Update is called once per frame
	void Update ()
    {
        RectTransform recTrans = GetComponent<RectTransform>();

        if (Input.GetMouseButtonDown(1) && inView)
        {
            if (direction == dir.UP && recTrans.offsetMax.y < 200)
            {
                //PanelScript hudPanScript = hudPanel.GetComponent<PanelScript>();
                //hudPanScript.inView = false;
                inView = false;
          
                if (cScript && cScript.boardScript)
                    cScript.boardScript.selected = null;
            }
            if (parent && recTrans.offsetMax.y < 200)
            {
                PanelScript parentScript = parent.GetComponent<PanelScript>();
                parentScript.inView = true;
                inView = false;
            }
        }

        Slide();

        if (name == "Turn Panel")
            TurnSlide();
    }

    // MainPanel
    public void SetButtons()
    {
        for (int i = 0; i < buttons.Length; i++)
            buttons[i].onClick.RemoveAllListeners();

        if (name == "Main Panel")
        {
            buttons[(int)butts.MOV_BUTT].onClick.AddListener(() => cScript.MovementSelection());
            buttons[(int)butts.ACT_BUTT].onClick.AddListener(() => cScript.ActionSelection());
            buttons[(int)butts.PASS_BUTT].onClick.AddListener(() => cScript.Pass());
            buttons[(int)butts.STATUS_BUTT].onClick.AddListener(() => cScript.Status());
        }
        else if (name == "Character Panel")
        {
            MenuScript menuScript = main.GetComponent<MenuScript>();
            buttons[0].onClick.AddListener(() => menuScript.NewCharacter());
            buttons[1].onClick.AddListener(() => menuScript.LoadCharacter());
            buttons[2].onClick.AddListener(() => menuScript.PresetCharacter());
            buttons[2].onClick.AddListener(() => menuScript.RandomCharacter());
        }
        else if (name == "Auxiliary Panel")
            buttons[0].onClick.AddListener(() => cScript.Status());
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
                FirstActionAvailable(panels[0], actions[i]);
            else if (eng[1].Length == 1)
                FirstActionAvailable(panels[1], actions[i]);
            else if (eng[1].Length == 2)
                FirstActionAvailable(panels[2], actions[i]);
            else if (eng[1].Length == 3)
                FirstActionAvailable(panels[3], actions[i]);
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
            buttScript.action = action;

            Text t = buttons[i].GetComponentInChildren<Text>();
            string[] actsSeparated = action.Split('|');
            string[] name = actsSeparated[1].Split(':');
            string[] eng = actsSeparated[2].Split(':');

            t.text = name[1];
            buttScript.SetTotalEnergy(eng[1]);

            PlayerScript playScript = cScript.player.GetComponent<PlayerScript>();
            if (playScript.CheckEnergy(eng[1]))
            {
                if (cScript)
                    buttons[i].onClick.AddListener(() => cScript.ActionTargeting());
                buttons[i].interactable = true;
            }

            return;
        }
    }

    public void ResetButtons()
    {
        for (int i = 0; i < panels.Length; i++)
        {
            Button[] buttons = panels[i].GetComponentsInChildren<Button>();

            for (int j = 0; j < buttons.Length; j++)
            {
                buttons[j].onClick.RemoveAllListeners();
                buttons[j].interactable = false;
                Text t = buttons[j].GetComponentInChildren<Text>();
                t.text = "EMPTY";
                ButtonScript buttScript = buttons[j].GetComponent<ButtonScript>();

                for (int k = 0; k < buttScript.energyPanel.Length; k++)
                    buttScript.energyPanel[k].SetActive(false);
            }
        }
    }

    // General Panel
    public void PopulateText()
    {
        if (name == "Round Panel")
        {
            BoardScript bScript = main.GetComponent<BoardScript>();
            if (text != null)
                text[0].text = "Round: " + bScript.roundCount;
        }
        else if (name == "Status Panel")
        {
            text[(int)CharacterScript.sts.HP].text = "HP: " + cScript.tempStats[(int)CharacterScript.sts.HP] + "/" + cScript.stats[(int)CharacterScript.sts.HP];
            text[(int)CharacterScript.sts.SPD].text = "SPD: " + cScript.stats[(int)CharacterScript.sts.SPD];
            text[(int)CharacterScript.sts.HIT].text = "HIT: " + cScript.stats[(int)CharacterScript.sts.HIT];
            text[(int)CharacterScript.sts.EVA].text = "EVA: " + cScript.stats[(int)CharacterScript.sts.EVA];
            text[(int)CharacterScript.sts.CRT].text = "CRT: " + cScript.stats[(int)CharacterScript.sts.CRT];
            text[(int)CharacterScript.sts.DMG].text = "DMG: " + cScript.stats[(int)CharacterScript.sts.DMG];
            text[(int)CharacterScript.sts.DEF].text = "DEF: " + cScript.stats[(int)CharacterScript.sts.DEF];
            text[(int)CharacterScript.sts.MOV].text = "MOV: " + cScript.stats[(int)CharacterScript.sts.MOV];
            text[(int)CharacterScript.sts.RNG].text = "RNG: " + cScript.stats[(int)CharacterScript.sts.RNG];
            if (cScript.accessories[0] != null)
                text[9].text = "ACC: " + cScript.accessories[0];
            if (cScript.accessories[1] != null)
                text[10].text = "ACC: " + cScript.accessories[1];
        }
        else if (name == "ActionViewer Panel")
        {
            string[] actStats = cScript.currAction.Split('|');
            string[] currStat = actStats[1].Split(':');
            text[0].text = currStat[1];

            currStat = actStats[2].Split(':');
            Button energy = GetComponentInChildren<Button>();
            ButtonScript engScript = energy.GetComponent<ButtonScript>();
            engScript.SetTotalEnergy(currStat[1]);

            currStat = actStats[3].Split(':');
            text[2].text = "HIT: " + currStat[1];
            if (cScript.stats[(int)CharacterScript.sts.HIT] != 0)
                text[2].text += " + " + cScript.stats[(int)CharacterScript.sts.HIT];

            currStat = actStats[4].Split(':');
            text[3].text = "DMG: " + currStat[1];
            if (cScript.stats[(int)CharacterScript.sts.DMG] != 0)
                text[3].text += " + " + cScript.stats[(int)CharacterScript.sts.DMG];

            currStat = actStats[5].Split(':');
            text[4].text = "RNG: " + currStat[1];
            if (cScript.stats[(int)CharacterScript.sts.RNG] != 0)
                text[4].text += "+" + cScript.stats[(int)CharacterScript.sts.RNG];

            currStat = actStats[6].Split(':');
            text[5].text = "CRT: " + currStat[1];
            if (cScript.stats[(int)CharacterScript.sts.CRT] != 0)
                text[5].text += " + " + cScript.stats[(int)CharacterScript.sts.CRT];

            currStat = actStats[7].Split(':');
            text[6].text = currStat[1];
        }

        else if (name == "HUD Panel LEFT" || name == "HUD Panel RIGHT")
        {
            text[0].text = cScript.name;
            text[1].text = "HP: " + cScript.tempStats[(int)CharacterScript.sts.HP] + "/" + cScript.stats[(int)CharacterScript.sts.HP];
            
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

        if (parent)
        {
            PanelScript parentScript = parent.GetComponent<PanelScript>();
            character = parentScript.character;
        }

        if (direction == dir.UP)
        {
            if (inView)
            {
                if (recTrans.offsetMax.y >  boundryDis)
                    transform.SetPositionAndRotation(new Vector3(transform.position.x, transform.position.y - 25.0f, transform.position.z), transform.rotation);
            }
            else
                if (recTrans.offsetMax.y < 750)
                    transform.SetPositionAndRotation(new Vector3(transform.position.x, transform.position.y + 25.0f, transform.position.z), transform.rotation);
        }
        else if (direction == dir.RIGHT)
        {
            if (inView)
            {
                if (recTrans.offsetMax.x > boundryDis)
                    transform.SetPositionAndRotation(new Vector3(transform.position.x - 25.0f, transform.position.y, transform.position.z), transform.rotation);
            }
            else
                if (recTrans.offsetMax.x < 877)
                    transform.SetPositionAndRotation(new Vector3(transform.position.x + 25.0f, transform.position.y, transform.position.z), transform.rotation);
        }
        else if (direction == dir.LEFT)
        {
            if (inView)
            {
                if (recTrans.offsetMax.x > boundryDis)
                    transform.SetPositionAndRotation(new Vector3(transform.position.x - 25.0f, transform.position.y, transform.position.z), transform.rotation);
            }
            else
                if (recTrans.offsetMax.x < -877)
                    transform.SetPositionAndRotation(new Vector3(transform.position.x + 25.0f, transform.position.y, transform.position.z), transform.rotation);
        }
        else if (direction == dir.DOWN)
        {
            if (inView)
            {
                if (recTrans.offsetMax.y < boundryDis)
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
        BoardScript bScript = main.GetComponent<BoardScript>();
        GameObject pan = null;
        int count = 0; // Count is used to determine how much to lower each panel

        // Set current panel and count actives
        for (int i = 0; i < panels.Length; i++)
        {
            if (panels[i].activeSelf)
            {
                if (pan == null)
                    pan = panels[i];
                count++;
            }
        }

        // If there are no actives, reset panels
        if (!pan)
        {
            count = bScript.currRound.Count;

            for (int i = 0; i < count; i++)
            {
                panels[i].SetActive(true);
                panels[i].transform.SetPositionAndRotation(new Vector3(panels[i].transform.position.x, 166 + 61.2f * i, panels[i].transform.position.z), panels[i].transform.rotation);
                CharacterScript charScript = bScript.currRound[i].GetComponent<CharacterScript>();
                charScript.turnPanel = panels[i];
                Text t = panels[i].GetComponentInChildren<Text>();
                t.text = bScript.currRound[i].name;

                if (panels[i].GetComponentInChildren<Button>())
                {
                    Button button = panels[i].GetComponentInChildren<Button>();
                    ButtonScript buttScript = button.GetComponent<ButtonScript>();
                    buttScript.SetTotalEnergy(charScript.color);
                    buttScript.character = bScript.currRound[i];
                }
            }
            pan = panels[0];
        }

        RectTransform panRect = pan.GetComponent<RectTransform>();

        if (count != bScript.currRound.Count) // if one of the panels is removed, move it every frame unitl it slides off to the left
        {
            if (panRect.offsetMax.x > -200)
                pan.transform.SetPositionAndRotation(new Vector3(pan.transform.position.x - 10.0f, pan.transform.position.y, pan.transform.position.z), pan.transform.rotation);
            else if (panRect.offsetMax.x <= -200)
                pan.SetActive(false);
        }
        else
        {
            // if was removed, check all of the panels that are still left to see if they are in their new position. If not, slide them down each frame until they are
            for (int i = 0; i < panels.Length; i++)
            {
                pan = panels[i];
                panRect = pan.GetComponent<RectTransform>();
                float height = 61.2f; // The differnce in y each panel is

                if (pan.activeSelf && panRect.offsetMax.y > 180 + height * i - height * (bScript.characters.Count - bScript.currRound.Count))
                    pan.transform.SetPositionAndRotation(new Vector3(pan.transform.position.x, pan.transform.position.y - 10.0f, pan.transform.position.z), pan.transform.rotation);

                if (pan.activeSelf && panRect.offsetMax.x < 45)
                    pan.transform.SetPositionAndRotation(new Vector3(pan.transform.position.x + 10.0f, pan.transform.position.y, pan.transform.position.z), pan.transform.rotation);
            }
        }
    }
}
