using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TurnPanelScript : PanelScript {

    private PanelScript[] m_panels;
    private GameManagerScript m_gamMan;
    private float m_spacing; //.4f

    // Use this for initialization
    new void Start ()
    {
        int numTurnPanels = 10;
        float dif = 0;

        m_panels = new PanelScript[numTurnPanels];

        for (int i = 0; i < numTurnPanels; i++)
        {
            GameObject turnPanel = Instantiate(Resources.Load<GameObject>("GUI/Next"));

            turnPanel.transform.SetParent(gameObject.transform, false);
            float x = turnPanel.transform.localScale.x;
            //dif = x - turnPanel.transform.localScale.x;

            m_panels[i] = turnPanel.GetComponent<PanelScript>();
        }

        if (GameObject.Find("Scene Manager"))
            m_gamMan = GameObject.Find("Scene Manager").GetComponent<GameManagerScript>();

        RectTransform rectT = Resources.Load<GameObject>("GUI/Next").GetComponent<RectTransform>();
        //rectT.localScale = Vector3.one;
        m_spacing = rectT.rect.width; //*2 + (rectT.rect.width * dif);
    }
	
	// Update is called once per frame
	void Update ()
    {
    }

    private void FixedUpdate()
    {
        TurnSlide();
    }

    // Turn Panel
    private void TurnSlide()
    {
        GameObject pan = null;

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

        if (m_gamMan.m_currRound.Count < m_panels.Length - 1 && m_gamMan.m_newTurn) // if one of the panels is removed, move it every frame unitl it slides off to the left
        {
            if (pan.transform.localPosition.y < 60)
                pan.transform.localPosition = new Vector2(pan.transform.localPosition.x, pan.transform.localPosition.y + 8.0f);
            else if (pan.transform.localPosition.y >= 60)
            {
                pan.SetActive(false);
                if (pan.GetComponent<Image>().color != new Color(1, .5f, .5f, 1))
                    m_gamMan.m_newTurn = false;
            }
        }
        else
        {
            // if was removed, check all of the panels that are still left to see if they are in their new position. If not, slide them down each frame until they are

            int widthCount = 0;
            GameObject last = null;

            for (int i = 0; i < m_panels.Length - 1; i++)
            {
                pan = m_panels[i].gameObject;
                if (!pan.activeSelf)
                    continue;


                if (pan.transform.localPosition.x > m_spacing * widthCount)
                {
                    pan.transform.localPosition = new Vector2(pan.transform.localPosition.x - 10.0f, pan.transform.localPosition.y);

                    if (pan.transform.localPosition.x < m_spacing * widthCount)
                        pan.transform.localPosition = new Vector2(m_spacing * widthCount, pan.transform.localPosition.y);
                }

                if (pan.transform.localPosition.y > 0)
                {
                    if (!last || pan.transform.localPosition.y > last.transform.localPosition.y + 10)
                    {
                        pan.transform.localPosition = new Vector2(pan.transform.localPosition.x, pan.transform.localPosition.y - 5.0f);

                        if (pan.transform.localPosition.y <= 0)
                        {
                            pan.transform.localPosition = new Vector2(pan.transform.localPosition.x, 0);
                            widthCount++;
                            last = null;
                            continue;
                        }
                    }
                }
                else
                {
                    widthCount++;
                    last = null;
                    continue;
                }

                widthCount++;
                last = pan;
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
        int roundCountModded = m_gamMan.m_currRound.Count - 1;

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
            m_panels[i].transform.localPosition = new Vector3(m_spacing * i, 60);
            TurnPanelInit(i, m_gamMan.m_currRound[i + 1].GetComponent<CharacterScript>()); // +1 because we are avoiding the first index
        }
    }
}
