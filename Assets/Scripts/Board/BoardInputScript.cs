using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BoardInputScript : InputManagerScript
{
    private BoardCamScript m_camera;
    private GameManagerScript m_gamMan;

    // Start is called before the first frame update
    new protected void Start()
    {
        base.Start();

        m_camera = GameObject.Find("BoardCam/Main Camera").GetComponent<BoardCamScript>();

        if (GameObject.Find("Scene Manager"))
            m_gamMan = GameObject.Find("Scene Manager").GetComponent<GameManagerScript>();
    }

    // Update is called once per frame
    override protected void Update()
    {
        base.Update();

        Inputs();
    }

    void Inputs()
    {
        // Select character
        //if (Input.GetMouseButtonDown(1))
        //{
        //    if (m_board.m_selected && m_board.m_selected.m_holding && m_board.m_selected.m_holding.tag == "Player")
        //        m_board.m_selected.m_holding.GetComponent<CharacterScript>().DeselectCharacter();
        //    m_board.m_selected = null;
        //}

        // Deselect button
        //if (Input.GetMouseButtonDown(1) && m_board.m_currButton)
        //{
        //    ButtonScript butt = m_board.m_currButton.GetComponent<ButtonScript>();
        //    butt.Select();
        //    butt.HoverFalse();
        //
        //    TileLinkScript.ClearRadius(m_board.m_highlightedTile);
        //}

        if (m_board.m_camIsFrozen || m_camera.m_rotate || m_board.m_isForcedMove)
            return;

        //if (m_selected && m_selected.m_holding && m_selected.m_holding.tag == "Player")
        //{
        //    // If character is not being viewed
        //    Renderer oldRend = m_selected.m_holding.transform.GetComponentInChildren<Renderer>();
        //    if (oldRend.materials[2].shader != oldRend.materials[0].shader)
        //        return;
        //}

        float w = Input.GetAxis("Mouse ScrollWheel");

        ActionPanelScript actPan = m_panMan.GetPanel("HUD Panel LEFT").transform.Find("Action Panel").GetComponent<ActionPanelScript>();

        if (Input.GetKeyDown(KeyCode.Q) && !m_panMan.GetPanel("Choose Panel").m_inView) //Formally getmousebuttondown(1);
            OnRightClick();
        else if (Input.GetKeyDown(KeyCode.Escape))
            m_panMan.GetPanel("Options Panel").PopulatePanel();
        else if (Input.GetKeyDown(KeyCode.LeftShift) && m_gamMan.m_hasActed[(int)GameManagerScript.trn.MOV] == false)
            m_panMan.GetPanel("HUD Panel LEFT").transform.Find("Move Pass Panel/Move").GetComponent<ButtonScript>().Select();
        //else if (Input.GetKeyDown(KeyCode.Space) && !m_panMan.GetPanel("Choose Panel").m_inView)
        //{
        //    if (m_currButton)
        //    {
        //        m_currButton.GetComponent<Image>().color = Color.white;
        //        m_currButton.GetComponent<ButtonScript>().m_main.ClosePanel();
        //        m_currButton = null;
        //        TileScript selectedTileScript = m_currCharScript.m_tile.GetComponent<TileScript>();
        //        if (selectedTileScript.m_radius.Count > 0)
        //            selectedTileScript.ClearRadius();
        //    }
        //    m_panMan.GetPanel("Confirmation Panel").GetComponent<ConfirmationPanelScript>().ConfirmationButton("Pass");
        //}
        else if (Input.GetKey(KeyCode.E) && w != 0)
        {
            if (!m_board.m_currButton)
            {
                actPan.transform.GetChild(0).GetComponent<ButtonScript>().Select();
                return;
            }

            int i = int.Parse(m_board.m_currButton.name);
            if (w < 0)
            {
                while (i + 1 < 8)
                {
                    Button currButt = actPan.transform.GetChild(i + 1).GetComponent<Button>();

                    if (currButt.GetComponentInChildren<Text>().text != "EMPTY" &&
                        m_gamMan.m_currCharScript.m_player.CheckEnergy(currButt.GetComponent<ActionButtonScript>().m_action.m_energy))
                    {
                        currButt.GetComponent<ButtonScript>().Select();
                        break;
                    }
                    i++;
                }
            }
            else if (w > 0)
            {
                while (i - 1 >= 0)
                {
                    Button currButt = actPan.transform.GetChild(i - 1).GetComponent<Button>();

                    if (i - 1 >= 0 && currButt.GetComponentInChildren<Text>().text != "EMPTY" &&
                         m_gamMan.m_currCharScript.m_player.CheckEnergy(currButt.GetComponent<ActionButtonScript>().m_action.m_energy))
                    {
                        currButt.GetComponent<ButtonScript>().Select();
                        break;
                    }
                    i--;
                }
            }
        }

        int num = -1;
        if (Input.GetKeyDown(KeyCode.Alpha1))
            num = 0;
        else if (Input.GetKeyDown(KeyCode.Alpha2))
            num = 1;
        else if (Input.GetKeyDown(KeyCode.Alpha3))
            num = 2;
        else if (Input.GetKeyDown(KeyCode.Alpha4))
            num = 3;
        //else if (Input.GetKeyDown(KeyCode.Alpha5))
        //    num = 4;
        //else if (Input.GetKeyDown(KeyCode.Alpha6))
        //    num = 5;
        //else if (Input.GetKeyDown(KeyCode.Alpha7))
        //    num = 6;
        //else if (Input.GetKeyDown(KeyCode.Alpha8))
        //    num = 7;

        Text t = null;
        Button butt = null;
        if (num >= 0)
        {
            butt = actPan.transform.GetChild(num).GetComponent<Button>();
            t = butt.GetComponentInChildren<Text>();
        }

        if (num >= 0 && t.text != "EMPTY" && butt.interactable == true)
        {
            TileLinkScript.ClearRadius(m_gamMan.m_currCharScript.m_tile);
            butt.GetComponent<ActionButtonScript>().Select();

            if (m_board.m_highlightedTile)
            {
                TileScript highlightTS = m_board.m_highlightedTile;
                if (highlightTS.m_holding && highlightTS.m_holding.tag == "Player")
                if (highlightTS.gameObject.GetComponent<Renderer>().material.color != TileLinkScript.c_attack &&
                    highlightTS.gameObject.GetComponent<Renderer>().material.color != TileLinkScript.c_radius)
                    m_panMan.GetPanel("ActionViewer Panel").transform.Find("ActionView").GetComponent<SlidingPanelScript>().m_inView = false;

            }
        }
    }

    static public void ResumeGame()
    {
        BoardScript board = GameObject.Find("Board").GetComponent<BoardScript>();

        board.m_isForcedMove = null;
        board.m_camIsFrozen = false;
        //panman.CloseHistory();
    }
}
