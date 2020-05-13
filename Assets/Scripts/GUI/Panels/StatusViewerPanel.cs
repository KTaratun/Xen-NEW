using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StatusViewerPanel : SlidingPanelScript
{
    private RelativeSlidePanelScript m_imagePan;
    private BoardScript m_board;

    // Start is called before the first frame update
    new void Start()
    {
        base.Start();

        m_imagePan = transform.Find("Status Image").GetComponent<RelativeSlidePanelScript>();

        if (GameObject.Find("Board").GetComponent<BoardScript>())
            m_board = GameObject.Find("Board").GetComponent<BoardScript>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!m_hovered && !m_imagePan.m_inView && m_imagePan.m_state == state.CLOSED)
            m_inView = false;
        if (m_hovered && m_inView && m_state == state.OPEN)
            m_imagePan.PopulatePanel();
    }

    new private void FixedUpdate()
    {
        base.FixedUpdate();
    }

    override public void PopulatePanel()
    {
        base.PopulatePanel();
        m_hovered = true;

        if (m_board.m_highlightedTile && m_board.m_highlightedTile.m_holding && m_board.m_highlightedTile.m_holding.GetComponent<PowerupScript>())
        {
            PowerupScript pUP = m_board.m_highlightedTile.m_holding.GetComponent<PowerupScript>();

            transform.Find("Status Image/Status Image").GetComponent<Image>().sprite = pUP.m_sprite;
            transform.Find("Status Image/Status Image").GetComponent<Image>().color = pUP.m_color;

            transform.Find("Main View/Duration").GetComponent<Text>().text = "Duration: N/A";
            transform.Find("Main View/Effect").GetComponent<Text>().text = pUP.m_effect;
        }
        else if (ParameterIconScript.m_currParameter)
        {
            transform.Find("Status Image/Status Image").GetComponent<Image>().sprite = ParameterIconScript.m_currParameter.m_image.sprite;

            CharacterScript.sts param = (CharacterScript.sts)System.Enum.Parse(typeof(CharacterScript.sts), ParameterIconScript.m_currParameter.name);

            Color col = Color.white;
            int statsAdded = m_cScript.m_tempStats[(int)param] - m_cScript.m_stats[(int)param];

            if (statsAdded > 0)
            {
                col = StatusScript.c_buffColor;
                transform.Find("Main View/Duration").GetComponent<Text>().text = ParameterIconScript.m_currParameter.m_title + m_cScript.m_stats[(int)param] + " + " + statsAdded.ToString();
            }
            else if (statsAdded < 0)
            {
                col = StatusScript.c_debuffColor;
                transform.Find("Main View/Duration").GetComponent<Text>().text = ParameterIconScript.m_currParameter.m_title + m_cScript.m_stats[(int)param] + " - " + statsAdded.ToString();
            }
            else
                transform.Find("Main View/Duration").GetComponent<Text>().text = ParameterIconScript.m_currParameter.m_title + m_cScript.m_stats[(int)param];

            transform.Find("Status Image/Status Image").GetComponent<Image>().color = col;

            transform.Find("Main View/Effect").GetComponent<Text>().text = ParameterIconScript.m_currParameter.m_desc;
        }
        else if (m_cScript && m_cScript.m_currStatus > -1)
        {
            StatusScript[] statScripts = m_cScript.GetComponents<StatusScript>();

            transform.Find("Status Image/Status Image").GetComponent<Image>().sprite = statScripts[m_cScript.m_currStatus].m_sprite;
            transform.Find("Status Image/Status Image").GetComponent<Image>().color = statScripts[m_cScript.m_currStatus].m_color;

            transform.Find("Main View/Duration").GetComponent<Text>().text = "Duration: " + statScripts[m_cScript.m_currStatus].m_lifeSpan.ToString();
            transform.Find("Main View/Effect").GetComponent<Text>().text = statScripts[m_cScript.m_currStatus].m_effect;
        }
    }

    override public void ClosePanel()
    {
        m_hovered = false;
        m_imagePan.ClosePanel();
        
        if (m_cScript)
            m_cScript.m_currStatus = -1;
    }
}
