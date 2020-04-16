using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SlidingPanelScript : PanelScript 
{
    public enum dir { UP, DOWN, LEFT, RIGHT, NULL }
    public enum state { CLOSED, SLIDE, OPEN }

    public state m_state = state.CLOSED;
    public bool m_hovered = false;
    protected RectTransform m_rectT;

    public bool m_inView; // This starts the sliding process of the panel
    public dir m_direction;
    public float m_slideSpeed;
    public float m_inBoundryDis;
    public float m_outBoundryDis;
    public bool m_slideArray;

    // Use this for initialization
    new protected void Start ()
    {
        base.Start();

        m_rectT = GetComponent<RectTransform>();

        if (m_slideSpeed == 0)
            m_slideSpeed = 30.0f;

        if (m_inBoundryDis == 0)
            m_inBoundryDis = 20;

        if (m_outBoundryDis == 0)
        {
            if (m_direction == dir.UP)
            {
                m_inBoundryDis -= GetComponent<RectTransform>().rect.height * .5f;
                m_outBoundryDis += GetComponent<RectTransform>().rect.height;
            }
            else if (m_direction == dir.RIGHT)
            {
                m_inBoundryDis -= GetComponent<RectTransform>().rect.max.x;
                m_outBoundryDis += GetComponent<RectTransform>().rect.width;
            }
            else if (m_direction == dir.LEFT)
            {
                m_inBoundryDis += GetComponent<RectTransform>().rect.max.x;
                m_outBoundryDis -= GetComponent<RectTransform>().rect.width;
            }
            else if (m_direction == dir.DOWN)
            {
                m_inBoundryDis += GetComponent<RectTransform>().rect.height * .5f;
                m_outBoundryDis -=  GetComponent<RectTransform>().rect.height;
            }
        }
    }
	
	// Update is called once per frame
	protected void Update ()
    {
        //if (m_slideArray)
        //    SlideArray();
        //else

        if (m_rectT)
        {
            Slide();
            SlideState();
        }
    }

    public void OpenPanel()
    {

        if (m_direction == dir.UP && !m_inView)
        {
            if (m_panMan.m_history.Count > 0 && name != "Confirmation Panel")
                m_panMan.m_history[m_panMan.m_history.Count - 1].ClosePanel();

            m_panMan.m_history.Add(this);
        }

        if (m_direction != dir.NULL)
            m_inView = true;
    }

    virtual public void ClosePanel()
    {
        m_inView = false;
    }

    virtual protected void Slide()
    {
        if (m_direction == dir.UP)
        {
            if (m_inView)
            {
                if (m_rectT.anchoredPosition.y > m_inBoundryDis) //if (recTrans.offsetMax.y > m_inBoundryDis)
                {
                    m_rectT.anchoredPosition = new Vector2(m_rectT.anchoredPosition.x, m_rectT.anchoredPosition.y - m_slideSpeed);

                    if (m_rectT.anchoredPosition.y <= m_inBoundryDis)
                        m_rectT.anchoredPosition = new Vector2(m_rectT.anchoredPosition.x, m_inBoundryDis);
                }
            }
            else if (m_rectT.anchoredPosition.y < m_outBoundryDis)
            {
                m_rectT.anchoredPosition = new Vector2(m_rectT.anchoredPosition.x, m_rectT.anchoredPosition.y + m_slideSpeed);
                if (m_rectT.anchoredPosition.y >= m_outBoundryDis)
                    m_rectT.anchoredPosition = new Vector2(m_rectT.anchoredPosition.x, m_outBoundryDis);
            }
        }
        else if (m_direction == dir.RIGHT)
        {
            if (m_inView)
            {
                if (m_rectT.anchoredPosition.x > m_inBoundryDis)
                {
                    m_rectT.anchoredPosition = new Vector2(m_rectT.anchoredPosition.x - m_slideSpeed, m_rectT.anchoredPosition.y);
                    if (m_rectT.anchoredPosition.x <= m_inBoundryDis)
                        m_rectT.anchoredPosition = new Vector2(m_inBoundryDis, m_rectT.anchoredPosition.y);
                }
            }
            else if (m_rectT.anchoredPosition.x < m_outBoundryDis)
            {
                m_rectT.anchoredPosition = new Vector2(m_rectT.anchoredPosition.x + m_slideSpeed, m_rectT.anchoredPosition.y);
                if (m_rectT.anchoredPosition.x >= m_outBoundryDis)
                    m_rectT.anchoredPosition = new Vector2(m_outBoundryDis, m_rectT.anchoredPosition.y);
            }
        }
        else if (m_direction == dir.LEFT)
        {
            if (m_inView)
            {
                if (m_rectT.anchoredPosition.x < m_inBoundryDis)
                {
                    m_rectT.anchoredPosition = new Vector2(m_rectT.anchoredPosition.x + m_slideSpeed, m_rectT.anchoredPosition.y);

                    if (m_rectT.anchoredPosition.x >= m_inBoundryDis)
                        m_rectT.anchoredPosition = new Vector2(m_inBoundryDis, m_rectT.anchoredPosition.y);
                }
            }
            else if (m_rectT.anchoredPosition.x > m_outBoundryDis)
            {
                m_rectT.anchoredPosition = new Vector2(m_rectT.anchoredPosition.x - m_slideSpeed, m_rectT.anchoredPosition.y);
                if (m_rectT.anchoredPosition.x <= m_outBoundryDis)
                    m_rectT.anchoredPosition = new Vector2(m_outBoundryDis, m_rectT.anchoredPosition.y);
            }
        }
        else if (m_direction == dir.DOWN)
        {
            if (m_inView)
            {
                if (m_rectT.anchoredPosition.y < m_inBoundryDis)
                {
                    m_rectT.anchoredPosition = new Vector2(m_rectT.anchoredPosition.x, m_rectT.anchoredPosition.y + m_slideSpeed);

                    if (m_rectT.anchoredPosition.y >= m_inBoundryDis)
                        m_rectT.anchoredPosition = new Vector2(m_rectT.anchoredPosition.x, m_inBoundryDis);
                }
            }
            else
            {
                if (m_rectT.anchoredPosition.y > m_outBoundryDis && name != "DamagePreview")
                {
                    m_rectT.anchoredPosition = new Vector2(m_rectT.anchoredPosition.x, m_rectT.anchoredPosition.y - m_slideSpeed);
                    if (m_rectT.anchoredPosition.y <= m_outBoundryDis)
                        m_rectT.anchoredPosition = new Vector2(m_rectT.anchoredPosition.x, m_outBoundryDis);
                }
                else if (m_rectT.anchoredPosition.y > 400)
                {
                    m_rectT.anchoredPosition = new Vector2(m_rectT.anchoredPosition.x, m_rectT.anchoredPosition.y - m_slideSpeed);
                    if (m_rectT.anchoredPosition.y <= m_outBoundryDis)
                        m_rectT.anchoredPosition = new Vector2(m_rectT.anchoredPosition.x, m_outBoundryDis);
                }
            }
        }
    }

    protected void SlideState()
    {
        if (m_direction == dir.UP)
        {
            if (m_rectT.anchoredPosition.y <= m_inBoundryDis)
                m_state = state.OPEN;
            else if (m_rectT.anchoredPosition.y >= m_outBoundryDis)
                m_state = state.CLOSED;
            else
                m_state = state.SLIDE;


        }
        else if (m_direction == dir.RIGHT)
        {
            if (m_rectT.anchoredPosition.x <= m_inBoundryDis)
                m_state = state.OPEN;
            else if (m_rectT.anchoredPosition.x >= m_outBoundryDis)
                m_state = state.CLOSED;
            else
                m_state = state.SLIDE;
        }
        else if (m_direction == dir.LEFT)
        {
            if (m_rectT.anchoredPosition.x >= m_inBoundryDis)
                m_state = state.OPEN;
            else if (m_rectT.anchoredPosition.x <= m_outBoundryDis)
                m_state = state.CLOSED;
            else
                m_state = state.SLIDE;
        }
        else if (m_direction == dir.DOWN)
        {
            if (m_rectT.anchoredPosition.y >= m_inBoundryDis)
                m_state = state.OPEN;
            else if (m_rectT.anchoredPosition.y <= m_outBoundryDis)
                m_state = state.CLOSED;
            else
                m_state = state.SLIDE;
        }
    }

    //private void SlideArray()
    //{
    //    PanelScript[] panels = transform.parent.GetComponent<PanelScript>().m_panels;
    //    int distance = 500;

    //    int myPos = 0;

    //    for (int i = 0; i < panels.Length; i++)
    //    {
    //        if (panels[i] == GetComponent<PanelScript>())
    //            myPos = i;
    //    }

    //    if (myPos == 0)
    //        return;

    //    if (m_direction == dir.UP)
    //    {
    //        if (m_inView)
    //        {
    //            float myTop = GetComponent<RectTransform>().offsetMax.y - distance;
    //            float theirBottom = panels[myPos - 1].GetComponent<RectTransform>().offsetMin.y;

    //            if (myTop > theirBottom) //if (recTrans.offsetMax.y > m_inBoundryDis)
    //                transform.SetPositionAndRotation(new Vector3(transform.position.x, transform.position.y - m_slideSpeed, transform.position.z), transform.rotation);
    //        }
    //        else if (transform.position.y < m_outBoundryDis)
    //        {
    //            transform.SetPositionAndRotation(new Vector3(transform.position.x, transform.position.y + m_slideSpeed, transform.position.z), transform.rotation);
    //            if (transform.position.y > m_outBoundryDis)
    //                transform.SetPositionAndRotation(new Vector3(transform.position.x, m_outBoundryDis, transform.position.z), transform.rotation);
    //        }
    //    }
    //}
}
