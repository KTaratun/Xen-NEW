using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterViewerPanel : PanelScript
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //override public void PopoulatePanel()
    //{
    //    base.PopulatePanel();
    //     // If another panel is open, don't open character viewer for already loaded character
    //
    //     Button currB = null;
    //     int res = 0;
    //
    //     PanelScript actionScript = transform.Find("Action Panel").GetComponent<PanelScript>();
    //     PanelScript statPan = transform.Find("Status Panel").GetComponent<PanelScript>();
    //
    //     if (m_main.name == "Menu")
    //     {
    //         TeamMenuScript tMenu = m_main.GetComponent<TeamMenuScript>();
    //         m_cScript = tMenu.m_currCharScript;
    //         m_cScript.InitializeStats();
    //
    //         if (EventSystem.current.currentSelectedGameObject)
    //             currB = EventSystem.current.currentSelectedGameObject.GetComponent<Button>();
    //         else
    //             currB = tMenu.m_currButton;
    //
    //         if (int.TryParse(currB.name, out res)) // If last pressed button is an int, it's a preset character panel button. Character slot buttons names are in the x,x format
    //         {
    //             DatabaseScript dbScript = m_main.GetComponent<DatabaseScript>();
    //
    //             string[] presetDataSeparated = dbScript.m_presets[int.Parse(currB.name)].Split('|');
    //             string[] presetName = presetDataSeparated[(int)DatabaseScript.presets.NAME].Split(':');
    //             string[] presetColor = presetDataSeparated[(int)DatabaseScript.presets.COLORS].Split(':');
    //
    //             int gen = Random.Range(0, 2);
    //             tMenu.FillOutCharacterData(presetName[1], presetColor[1], dbScript.GetActions(dbScript.m_presets[int.Parse(currB.name)]), "", 0, 1, gen);
    //
    //             // Fill out name
    //             GetComponentInChildren<InputField>().text = m_cScript.m_name;
    //
    //             // Determine if select or remove will be visible
    //             Transform selectButton = transform.Find("Main Buttons/Select Button");
    //             Transform removeButton = transform.Find("Main Buttons/Remove Button");
    //
    //             if (selectButton.position.x > 1000)
    //                 selectButton.position = new Vector3(selectButton.position.x - 1000, selectButton.position.y, selectButton.position.z);
    //             if (removeButton.position.x < 1000)
    //                 removeButton.position = new Vector3(removeButton.position.x + 1000, removeButton.position.y, removeButton.position.z);
    //         }
    //         else
    //         {
    //             if (!GetComponent<SlidingPanelScript>().m_inView)
    //                 tMenu.m_currButton = currB;
    //
    //             m_cScript = PlayerPrefScript.LoadChar(currB.name, m_cScript);
    //             tMenu.m_currCharScript = PlayerPrefScript.LoadChar(currB.name, tMenu.m_currCharScript);
    //
    //             // Fill out name
    //             transform.Find("InputField/Name").GetComponent<Text>().text = m_cScript.m_name;
    //             GetComponentInChildren<InputField>().text = m_cScript.m_name;
    //
    //             m_cScript.m_exp = 10;
    //             // Determine if select or remove will be visible
    //             Transform selectButton = transform.Find("Main Buttons/Select Button");
    //             Transform removeButton = transform.Find("Main Buttons/Remove Button");
    //
    //             if (selectButton.position.x > 1000)
    //                 selectButton.position = new Vector3(selectButton.position.x - 1000, selectButton.position.y, selectButton.position.z);
    //             if (removeButton.position.x < 1000)
    //                 removeButton.position = new Vector3(removeButton.position.x + 1000, removeButton.position.y, removeButton.position.z);
    //
    //             Button levelButton = transform.Find("Main Buttons/Select Button").GetComponent<Button>();
    //
    //             if (m_cScript.m_exp >= 10 && !m_panMan.GetPanel("New Action Panel").GetComponent<SlidingPanelScript>().m_inView && !m_panMan.GetPanel("New Status Panel").GetComponent<SlidingPanelScript>().m_inView)
    //                 levelButton.interactable = true;
    //             else if (m_cScript.m_exp < 10)
    //                 levelButton.interactable = false;
    //         }
    //     }
    //     else if (m_main.name == "Board")
    //         m_cScript = m_main.GetComponent<BoardScript>().m_selected.GetComponent<TileScript>().m_holding.GetComponent<CharacterScript>();
    //
    //     // Fill out Action Panel
    //     actionScript.m_cScript = m_cScript;
    //     actionScript.PopulatePanel();
    //     // Fill out Status Panel
    //     statPan.m_cScript = m_cScript;
    //     statPan.PopulatePanel();
    //}
}
