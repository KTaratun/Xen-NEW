using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MenuScript : MonoBehaviour {

    public Button currButton;
    public GameObject currCharacter;
    public GameObject character;
    public GameObject characterPanel;
    public GameObject characterViewer;
    public GameObject presetSelect;

	// Use this for initialization
	void Start ()
    {
        GameObject newChar = Instantiate(character);
        currCharacter = newChar;

        PlayerPrefs.DeleteAll();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void CharacterAssignment()
    {
        PanelScript charScript = characterPanel.GetComponent<PanelScript>();
        PanelScript viewScript = characterViewer.GetComponent<PanelScript>();
        PanelScript presetScript = presetSelect.GetComponent<PanelScript>();

        if (charScript.inView == true || viewScript.inView == true || presetScript.inView == true)
            return;

        charScript.inView = true;
        charScript.SetButtons();
        currButton = EventSystem.current.currentSelectedGameObject.GetComponent<Button>();
    }

    public void NewCharacter()
    {

    }

    public void LoadCharacter()
    {

    }

    public void PresetCharacter()
    {
        PanelScript charPanel = characterPanel.GetComponent<PanelScript>();
        charPanel.inView = false;

        PanelScript presetSelectScript = presetSelect.GetComponent<PanelScript>();
        presetSelectScript.inView = true;

        DatabaseScript dbScript = gameObject.GetComponent<DatabaseScript>();

        for (int i = 0; i < presetSelectScript.buttons.Length; i++)
        {
            Button butt = presetSelectScript.buttons[i];
            Text t = butt.GetComponentInChildren<Text>();
            t.text = dbScript.GetDataValue(dbScript.presets[i], "Name:");
            butt.name = i.ToString();
            butt.onClick.RemoveAllListeners();
            butt.onClick.AddListener(() => PopulateCharacterViewer());
            ButtonScript buttScript = butt.GetComponent<ButtonScript>();
            buttScript.SetTotalEnergy(dbScript.GetDataValue(dbScript.presets[i], "Colors:"));
        }
    }

    public void RandomCharacter()
    {

    }

    public void PopulateCharacterViewer()
    {
        // If another panel is open, don't open character viewer for already loaded character
        PanelScript presetSelectScript = presetSelect.GetComponent<PanelScript>();
        PanelScript charViewScript = characterViewer.GetComponent<PanelScript>();
        PanelScript charScript = characterPanel.GetComponent<PanelScript>();

        Button currB = EventSystem.current.currentSelectedGameObject.GetComponent<Button>();
        int res = 0;

        if (charScript.inView == true || charViewScript.inView == true || presetSelectScript.inView == true && !int.TryParse(currB.name, out res))
            return;

        presetSelectScript.inView = false;
        charViewScript.inView = true;

        Button[] buttons = charViewScript.GetComponentsInChildren<Button>();
        PanelScript actionScript = charViewScript.panels[0].GetComponent<PanelScript>();
        PanelScript statPan = charViewScript.panels[1].GetComponent<PanelScript>();

        Text[] name = charViewScript.GetComponentsInChildren<Text>();

        CharacterScript currCharScript = currCharacter.GetComponent<CharacterScript>();
        actionScript.character = currCharacter;
        actionScript.cScript = currCharScript;

        res = 0;
        if (int.TryParse(currB.name, out res)) // If last pressed button is an int, it's a preset character panel button. Character slot buttons names are in the x,x format
        {
            charViewScript.parent = presetSelect;
            DatabaseScript dbScript = gameObject.GetComponent<DatabaseScript>();

            string[] presetDataSeparated = dbScript.presets[int.Parse(currB.name)].Split('|');

            // Fill out energy
            string[] presetColor = presetDataSeparated[2].Split(':');
            ButtonScript buttScript = buttons[0].GetComponent<ButtonScript>(); // button[0] == energy
            buttons[0].name = presetColor[1];
            buttScript.SetTotalEnergy(presetColor[1]);
            currCharScript.color = presetColor[1];

            // Fill out name
            string[] presetName = presetDataSeparated[1].Split(':');
            name[1].text = presetName[1];
            
            // Fill out current character data
            currCharScript.name = presetName[1];
            currCharScript.actions = dbScript.GetActions(dbScript.presets[int.Parse(currB.name)]);

            // Fill out status
            statPan.character = currCharacter;
            statPan.cScript = currCharScript;
            statPan.PopulateText();

            // Fill out actions
            actionScript.PopulateActionButtons(currCharScript.actions);

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
            charViewScript.parent = null;
            ButtonScript buttScript = buttons[0].GetComponent<ButtonScript>(); // button[0] == energy

            // Fill out name
            name[1].text = PlayerPrefs.GetString(currB.name + ",name");
            
            // Fill out energy
            buttScript.SetTotalEnergy(PlayerPrefs.GetString(currButton.name + ",stats"));

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
        PanelScript charViewScript = characterViewer.GetComponent<PanelScript>();
        charViewScript.inView = false;

        CharacterScript cScript = currCharacter.GetComponent<CharacterScript>();

        // Save out pertinent character data
        string key = currButton.name + ",actions";
        string combo = string.Join(";", cScript.actions);
        PlayerPrefs.SetString(key, combo);
        key = currButton.name + ",name";
        PlayerPrefs.SetString(key, cScript.name);
        key = currButton.name + ",color";
        PlayerPrefs.SetString(key, cScript.color);

        // Change text to reflect the new character
        Text t = currButton.GetComponentInChildren<Text>();
        t.text = cScript.name;

        // Set energy for button
        Button[] buttons = charViewScript.GetComponentsInChildren<Button>();
        ButtonScript buttScript = currButton.GetComponent<ButtonScript>();
        key = currButton.name + ",stats";
        string stats = buttons[0].name;
        for (int i = 0; i < cScript.stats.Length; i++)
            stats += "," + cScript.stats[i];
        PlayerPrefs.SetString(key, stats);
        buttScript.SetTotalEnergy(buttons[0].name);

        // Change color of the button
        currButton.GetComponent<Image>().color = new Color(.85f, .85f, .85f, 1);

        // Set up button to show current character rather than setting up a new one
        currButton.onClick = new Button.ButtonClickedEvent();
        currButton.onClick.AddListener(() => PopulateCharacterViewer());
    }

    public void Remove()
    {
        // Change color of button back
        currButton.GetComponent<Image>().color = new Color(.6f, .6f, .6f, 1);

        // Reset energy
        ButtonScript buttScript = currButton.GetComponent<ButtonScript>();
        for (int k = 0; k < buttScript.energyPanel.Length; k++)
            buttScript.energyPanel[k].SetActive(false);

        // Close panel
        PanelScript charViewPan = characterViewer.GetComponent<PanelScript>();
        charViewPan.inView = false;

        // Change text back
        Text t = currButton.GetComponentInChildren<Text>();
        t.text = "EMPTY";

        // Remove character from playerprefs
        string key = currButton.name + ",actions";
        PlayerPrefs.DeleteKey(key);
        key = currButton.name + ",name";
        PlayerPrefs.DeleteKey(key);
        key = currButton.name + ",stats";
        PlayerPrefs.DeleteKey(key);

        currButton.onClick = new Button.ButtonClickedEvent();
        currButton.onClick.AddListener(() => CharacterAssignment());
    }

    public void StartGame()
    {
        Application.LoadLevel("Scene1");
    }
}
