using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TurnPanelScript : PanelScript {

    private PanelScript[] m_panels;
    private BoardScript m_board;

    // Use this for initialization
    void Start ()
    {
        int numTurnPanels = 10;

        m_panels = new PanelScript[numTurnPanels];

        for (int i = 0; i < numTurnPanels; i++)
        {
            GameObject turnPanel = Instantiate(Resources.Load<GameObject>("GUI/Next"));

            turnPanel.transform.SetParent(gameObject.transform);
            m_panels[i] = turnPanel.GetComponent<PanelScript>();
            //m_panels[i].GetComponent<EnergyButtonScript>().EnergyInit();
            //m_panels[i].gameObject.SetActive(false);
        }

        if (GameObject.Find("Board").GetComponent<BoardScript>())
            m_board = GameObject.Find("Board").GetComponent<BoardScript>();
    }
	
	// Update is called once per frame
	void Update ()
    {
        TurnSlide();
    }

    // Turn Panel
    private void TurnSlide()
    {
        GameObject pan = null;
        float width = 90.0f;
        float start = 190.0f;

        // Set current panel and count 
        for (int i = 0; i < m_panels.Length; i++)
        {
            if (m_panels[i].gameObject.activeSelf)
            {
                pan = m_panels[i].gameObject;
                break;
            }
        }

        if (pan == null)
            return;

        //if (bScript.m_currRound.Count >= 8 && bScript.m_newTurn) //notListed.Count > 0 && notListed.Count + count > bScript.m_currRound.Count
        //{
        //    for (int i = 0; i < 7; i++)
        //    {
        //        ButtonScript b = m_panels[i + 1].GetComponentInChildren<ButtonScript>();
        //        TurnPanelInit(i, b.m_cScript);
        //    }
        //
        //    TurnPanelInit(7, bScript.m_currRound[7].GetComponent<CharacterScript>());
        //
        //    if (bScript.m_currRound.Count == 8)
        //        m_panels[8].SetActive(false);
        //    else
        //    {
        //        Text t = m_panels[8].GetComponentInChildren<Text>();
        //        t.text = "x" + (bScript.m_currRound.Count - 8).ToString();
        //    }
        //
        //    bScript.m_newTurn = false;
        //}
        if (m_board.m_currRound.Count < m_panels.Length - 1 && m_board.m_newTurn) // if one of the panels is removed, move it every frame unitl it slides off to the left
        {
            if (pan.transform.position.y < 750)
                pan.transform.SetPositionAndRotation(new Vector3(pan.transform.position.x, pan.transform.position.y + 10.0f, pan.transform.position.z), pan.transform.rotation);
            else if (pan.transform.position.y >= 750)
            {
                pan.SetActive(false);
                if (pan.GetComponent<Image>().color != new Color(1, .5f, .5f, 1))
                    m_board.m_newTurn = false;
            }
        }
        else
        {
            // if was removed, check all of the panels that are still left to see if they are in their new position. If not, slide them down each frame until they are

            int widthCount = 0;

            for (int i = 0; i < m_panels.Length - 1; i++)
            {
                pan = m_panels[i].gameObject;
                if (!pan.activeSelf)
                    continue;

                float widthMod = start + width * widthCount;
                widthCount++;

                if (pan.transform.position.x > widthMod)
                {
                    pan.transform.SetPositionAndRotation(new Vector3(pan.transform.position.x - 10.0f, pan.transform.position.y, pan.transform.position.z), pan.transform.rotation);

                    if (pan.transform.position.x < widthMod)
                        pan.transform.SetPositionAndRotation(new Vector3(widthMod, pan.transform.position.y, pan.transform.position.z), pan.transform.rotation);
                }

                if (pan.transform.position.y > 705)
                {
                    pan.transform.SetPositionAndRotation(new Vector3(pan.transform.position.x, pan.transform.position.y - 5.0f, pan.transform.position.z), pan.transform.rotation);

                    if (pan.transform.position.y < 705)
                        pan.transform.SetPositionAndRotation(new Vector3(pan.transform.position.x, 705, pan.transform.position.z), pan.transform.rotation);
                }
            }
        }
    }

    private void TurnPanelInit(int _ind, CharacterScript _charScript)
    {
        PanelScript currPan = m_panels[_ind];
        currPan.gameObject.SetActive(true);

        _charScript.m_turnPanels.Add(currPan.gameObject);

        if (_charScript.m_effects[(int)StatusScript.effects.STUN] && _ind == 0)
            currPan.GetComponent<Image>().color = new Color(1, .5f, .5f, 1);
        else
            currPan.GetComponent<Image>().color = _charScript.m_teamColor;

        Text t = currPan.GetComponentInChildren<Text>();
        t.text = _charScript.m_name;

        if (currPan.GetComponentInChildren<Button>())
        {
            TurnButtonScript buttScript = currPan.GetComponentInChildren<Button>().GetComponent<TurnButtonScript>();
            buttScript.m_cScript = _charScript;
            buttScript.SetTotalEnergy(_charScript.m_color);
            buttScript.m_object = _charScript.gameObject; 
        }
    }

    public void NewTurnOrder()
    {
        BoardScript bScript = m_board.GetComponent<BoardScript>();
        float width = 75.0f;
        float start = -360.0f;
        int roundCountModded = bScript.m_currRound.Count - 1;

        if (roundCountModded == 0)
            return;

        //if (roundCountModded > 8)
        //{
        //    m_panels[8].SetActive(true);
        //    Text t = m_panels[8].GetComponentInChildren<Text>();
        //    t.text = "x" + (roundCountModded - 8).ToString();
        //    roundCountModded = 8;
        //}

        for (int i = 0; i < roundCountModded; i++)
        {
            m_panels[i].transform.position = new Vector3(start + transform.position.x + width * i, transform.position.y);
            TurnPanelInit(i, bScript.m_currRound[i + 1].GetComponent<CharacterScript>()); // +1 because we are avoiding the first index
        }
    }
}
