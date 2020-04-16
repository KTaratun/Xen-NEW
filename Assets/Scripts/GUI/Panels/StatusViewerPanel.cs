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
    new void Update()
    {
        base.Update();

        if (!m_hovered && !m_imagePan.m_inView && m_imagePan.m_state == state.CLOSED)
            m_inView = false;
        if (m_hovered && m_inView && m_state == state.OPEN)
            m_imagePan.PopulatePanel();
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
        else
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
    }
}
