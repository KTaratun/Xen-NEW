using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MenuScript : MonoBehaviour {

    public Button m_currButton;
    public GameObject m_currCharacter;
    public GameObject m_character;
    public GameObject m_characterPanel;
    public GameObject m_characterViewer;
    public GameObject m_presetSelect;

	// Use this for initialization
	void Start ()
    {
        GameObject newChar = Instantiate(m_character);
        m_currCharacter = newChar;

        PlayerPrefs.DeleteAll();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void CharacterAssignment()
    {
        PanelScript charScript = m_characterPanel.GetComponent<PanelScript>();
        PanelScript viewScript = m_characterViewer.GetComponent<PanelScript>();
        PanelScript presetScript = m_presetSelect.GetComponent<PanelScript>();

        if (charScript.m_inView == true || viewScript.m_inView == true || presetScript.m_inView == true)
            return;

        charScript.m_inView = true;
        charScript.SetButtons();
        m_currButton = EventSystem.current.currentSelectedGameObject.GetComponent<Button>();
    }

    public void NewCharacter()
    {

    }

    public void LoadCharacter()
    {

    }

    public void PresetCharacter()
    {
        PanelScript charPanel = m_characterPanel.GetComponent<PanelScript>();
        charPanel.m_inView = false;

        PanelScript presetSelectScript = m_presetSelect.GetComponent<PanelScript>();
        presetSelectScript.m_inView = true;

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

    }

    public void PopulateCharacterViewer()
    {
        // If another panel is open, don't open character viewer for already loaded character
        PanelScript presetSelectScript = m_presetSelect.GetComponent<PanelScript>();
        PanelScript charViewScript = m_characterViewer.GetComponent<PanelScript>();
        PanelScript charScript = m_characterPanel.GetComponent<PanelScript>();

        Button currB = EventSystem.current.currentSelectedGameObject.GetComponent<Button>();
        int res = 0;

        if (charScript.m_inView == true || charViewScript.m_inView == true || presetSelectScript.m_inView == true && !int.TryParse(currB.name, out res))
            return;

        presetSelectScript.m_inView = false;
        charViewScript.m_inView = true;

        Button[] buttons = charViewScript.GetComponentsInChildren<Button>();
        PanelScript actionScript = charViewScript.m_panels[0].GetComponent<PanelScript>();
        PanelScript statPan = charViewScript.m_panels[1].GetComponent<PanelScript>();

        Text[] name = charViewScript.GetComponentsInChildren<Text>();

        CharacterScript currCharScript = m_currCharacter.GetComponent<CharacterScript>();
        actionScript.m_character = m_currCharacter;
        actionScript.m_cScript = currCharScript;

        res = 0;
        if (int.TryParse(currB.name, out res)) // If last pressed button is an int, it's a preset character panel button. Character slot buttons names are in the x,x format
        {
            charViewScript.m_parent = m_presetSelect;
            DatabaseScript dbScript = gameObject.GetComponent<DatabaseScript>();

            string[] presetDataSeparated = dbScript.m_presets[int.Parse(currB.name)].Split('|');

            // Fill out energy
            string[] presetColor = presetDataSeparated[(int)DatabaseScript.presets.COLORS].Split(':');
            ButtonScript buttScript = buttons[0].GetComponent<ButtonScript>(); // button[0] == energy
            buttons[0].name = presetColor[1];
            buttScript.SetTotalEnergy(presetColor[1]);
            currCharScript.m_color = presetColor[1];

            // Fill out name
            string[] presetName = presetDataSeparated[(int)DatabaseScript.presets.NAME].Split(':');
            name[1].text = presetName[1];
            
            // Fill out current character data
            currCharScript.name = presetName[1];
            currCharScript.m_actions = dbScript.GetActions(dbScript.m_presets[int.Parse(currB.name)]);

            // Fill out status
            statPan.m_character = m_currCharacter;
            statPan.m_cScript = currCharScript;
            statPan.PopulateText();

            // Fill out actions
            actionScript.PopulateActionButtons(currCharScript.m_actions);

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
            charViewScript.m_parent = null;
            ButtonScript buttScript = buttons[0].GetComponent<ButtonScript>(); // button[0] == energy

            // Fill out name
            name[1].text = PlayerPrefs.GetString(currB.name + ",name");
            
            // Fill out energy
            buttScript.SetTotalEnergy(PlayerPrefs.GetString(m_currButton.name + ",stats"));

            // Fill out actions
            string str = PlayerPrefs.GetString(currB.name + ",actions");
            string[] acts = str.Split(';');
            actionScript.PopulateActionButtons(acts);

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

    public void Select()
    {
        PanelScript charViewScript = m_characterViewer.GetComponent<PanelScript>();
        charViewScript.m_inView = false;

        CharacterScript cScript = m_currCharacter.GetComponent<CharacterScript>();

        // Save out pertinent character data
        string key = m_currButton.name + ",actions";
        string combo = string.Join(";", cScript.m_actions);
        PlayerPrefs.SetString(key, combo);
        key = m_currButton.name + ",name";
        PlayerPrefs.SetString(key, cScript.name);
        key = m_currButton.name + ",color";
        PlayerPrefs.SetString(key, cScript.m_color);

        // Change text to reflect the new character
        Text t = m_currButton.GetComponentInChildren<Text>();
        t.text = cScript.name;

        // Set energy for button
        Button[] buttons = charViewScript.GetComponentsInChildren<Button>();
        ButtonScript buttScript = m_currButton.GetComponent<ButtonScript>();
        key = m_currButton.name + ",stats";
        string stats = buttons[0].name;
        for (int i = 0; i < cScript.m_stats.Length; i++)
            stats += "," + cScript.m_stats[i];
        PlayerPrefs.SetString(key, stats);
        buttScript.SetTotalEnergy(buttons[0].name);

        // Change color of the button
        m_currButton.GetComponent<Image>().color = new Color(.85f, .85f, .85f, 1);

        // Set up button to show current character rather than setting up a new one
        m_currButton.onClick = new Button.ButtonClickedEvent();
        m_currButton.onClick.AddListener(() => PopulateCharacterViewer());
    }

    public void Remove()
    {
        // Change color of button back
        m_currButton.GetComponent<Image>().color = new Color(.6f, .6f, .6f, 1);

        // Reset energy
        ButtonScript buttScript = m_currButton.GetComponent<ButtonScript>();
        for (int k = 0; k < buttScript.m_energyPanel.Length; k++)
            buttScript.m_energyPanel[k].SetActive(false);

        // Close panel
        PanelScript charViewPan = m_characterViewer.GetComponent<PanelScript>();
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

    public void StartGame()
    {
        Application.LoadLevel("Scene1");
    }
}
