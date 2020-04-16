using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class ConfirmationPanelScript : SlidingPanelScript 
{
    private BoardScript m_board;
    private TeamMenuScript m_tMenu;

    // Use this for initialization
    new void Start ()
    {
        base.Start();

        if (GameObject.Find("Board"))
            m_board = GameObject.Find("Board").GetComponent<BoardScript>();

        if (GameObject.Find("Scene Manager"))
        {
            m_panMan = GameObject.Find("Scene Manager").GetComponent<SlidingPanelManagerScript>();
            if (GameObject.Find("Scene Manager").GetComponent<TeamMenuScript>())
                m_tMenu = GameObject.Find("Scene Manager").GetComponent<TeamMenuScript>();
        }

        if (m_slideSpeed == 0)
            m_slideSpeed = 30.0f;

        if (m_direction == dir.UP)
            m_outBoundryDis = GetComponent<RectTransform>().rect.height;

        Button butt = transform.Find("Cancel").gameObject.GetComponent<ButtonScript>().GetComponent<Button>();
        butt.onClick.AddListener(() => CancelButton());
    }
	
	// Update is called once per frame
	new void Update () 
    {
        base.Update();
	}

    new public void OpenPanel()
    {
        base.OpenPanel();

        if ((int)Input.mousePosition.x < 120)
            transform.position = new Vector3(120, transform.position.y, transform.position.z);
        else if ((int)Input.mousePosition.x > 1160)
            transform.position = new Vector3(1160, transform.position.y, transform.position.z);
        else
            transform.position = new Vector3(Input.mousePosition.x, transform.position.y, transform.position.z);

            m_inBoundryDis = -Screen.height + (int)Input.mousePosition.y + 100;

        if (m_inBoundryDis < -400)
            m_inBoundryDis = -400;
    }

    public void CancelButton()
    {
        GameObject gO = transform.Find("Cancel").gameObject;
        gO.GetComponent<Image>().color = Color.white;

        ClosePanel();
    }

    public void ConfirmationButton(string _confirm)
    {
        PanelScript parent = null;
        GameObject gO = transform.Find("Confirm").gameObject;
        ButtonScript buttScript = gO.GetComponent<ButtonScript>();
        Button butt = gO.GetComponent<Button>();

        //transform.Find("Cancel").GetComponent<ButtonScript>().m_object = 


        //if (!buttScript.m_main || SlidingPanelManagerScript.GetCurrentPanel() && SlidingPanelManagerScript.GetCurrentPanel().name != "Confirmation Panel")
        //{
        //    parent = SlidingPanelManagerScript.GetCurrentPanel();
        //    parent.PopulatePanel();
        //}

        OpenPanel();

        butt.onClick.RemoveAllListeners();

        if (_confirm == "Action")
        {
            butt.onClick.AddListener(() => m_board.m_currCharScript.m_currAction.ActionStart());
        }
        else if (_confirm == "Choose Panel")
        {
            butt.onClick.AddListener(() => m_panMan.CloseAll());
        }
        else if (_confirm == "Clear Team")
        {
            Button b = EventSystem.current.currentSelectedGameObject.GetComponent<Button>();
            butt.onClick.AddListener(() => m_tMenu.ClearTeam(b.transform.parent.GetComponent<PanelScript>()));
        }
        else if (_confirm == "Move")
        {
            bool isForcedMove = false;
            buttScript.m_object = m_cScript.gameObject;

            if (buttScript.m_boardScript.m_isForcedMove)
            {
                buttScript.m_object = buttScript.m_boardScript.m_isForcedMove;
                isForcedMove = true;
            }

            butt.onClick.AddListener(() => buttScript.m_object.GetComponent<ObjectScript>().MovingStart(buttScript.m_boardScript.m_selected, isForcedMove, false));
        }
        else if (_confirm == "New Action")
        {
            //TeamMenuScript tMenuScript = buttScript.m_main.m_main.GetComponent<TeamMenuScript>();
            //tMenuScript.m_oldButton = EventSystem.current.currentSelectedGameObject.GetComponent<Button>();
            //_button.GetComponent<Button>().onClick.AddListener(() => tMenuScript.ReplaceAction());
        }
        else if (_confirm == "New Stats")
        {
            Transform statText = m_panMan.GetPanel("CharacterViewer Panel").transform.Find("Status Panel/Text");
            Button newStat = EventSystem.current.currentSelectedGameObject.GetComponent<Button>();
            string[] statSeparated = newStat.name.Split('|');

            for (int i = 1; i < 8; i++)
            {
                Text currText = statText.transform.GetChild(i).GetComponent<Text>();

                string[] s = statSeparated[i].Split(':');
                string[] textSeparated = currText.text.Split(':');
                int newNumber = buttScript.m_main.m_cScript.m_stats[i - 1] + int.Parse(s[1]);
                if (i == 0)
                    currText.text = textSeparated[0] + ": " + newNumber.ToString() + "/" + newNumber.ToString();
                else
                    currText.text = textSeparated[0] + ": " + newNumber.ToString();

                if (int.Parse(s[1]) > 0)
                    currText.color = new Color(0, .7f, .7f, 1);
                else if (int.Parse(s[1]) < 0)
                    currText.color = new Color(.9f, .4f, 0, 1);
                else
                    currText.color = Color.black;
            }

            if (m_tMenu.m_statButton)
                m_tMenu.m_statButton.image.color = Color.white;

            m_tMenu.m_statButton = newStat;
            newStat.image.color = Color.cyan;
            butt.onClick.AddListener(() => m_tMenu.AddStatAlteration());
        }
        else if (_confirm == "None Action")
        {
            TeamMenuScript tMenuScript = buttScript.m_main.GetComponent<TeamMenuScript>();
            tMenuScript.m_oldButton = EventSystem.current.currentSelectedGameObject.GetComponent<Button>();
            butt.onClick.AddListener(() => tMenuScript.CloseLevelPanel(0));
        }
        else if (_confirm == "None Stats")
        {
            TeamMenuScript tMenuScript = buttScript.m_main.GetComponent<TeamMenuScript>();
            tMenuScript.m_statButton = null;
            butt.onClick.AddListener(() => tMenuScript.CloseLevelPanel(1));
        }
        else if (_confirm == "Pass")
        {
            m_panMan.GetPanel("HUD Panel LEFT").transform.Find("Move Pass Panel/Pass").GetComponent<ButtonScript>().Select();
            transform.Find("Confirm").GetComponent<Button>().onClick.AddListener(() => buttScript.m_boardScript.Pass());
        }
        else if (_confirm == "Random Team")
        {
            Button b = EventSystem.current.currentSelectedGameObject.GetComponent<Button>();
            butt.onClick.AddListener(() => m_tMenu.RandomTeam(b));
        }
        else if (_confirm == "Remove")
        {
            butt.onClick.AddListener(() => m_tMenu.Remove());
        }
        else if (_confirm == "Save")
        {
            m_tMenu.m_saveButton = EventSystem.current.currentSelectedGameObject.GetComponent<Button>();

            butt.onClick.AddListener(() => m_tMenu.Save());
        }
    }
}
