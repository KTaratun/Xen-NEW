using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RelativeSlidePanelScript : SlidingPanelScript 
{
    // Use this for initialization
	new void Start () 
    {
        m_rectT = GetComponent<RectTransform>();

        if (m_slideSpeed == 0)
            m_slideSpeed = 30.0f;

        if (m_direction == dir.UP)
        {
            m_inBoundryDis += transform.parent.GetComponent<RectTransform>().rect.min.y;
            m_outBoundryDis += transform.parent.GetComponent<RectTransform>().rect.max.y;
        }
        else if (m_direction == dir.RIGHT)
        {
            m_inBoundryDis -= transform.parent.GetComponent<RectTransform>().rect.min.x;
            m_outBoundryDis += transform.parent.GetComponent<RectTransform>().rect.max.x;
        }
        else if (m_direction == dir.LEFT)
        {
            m_inBoundryDis += transform.parent.GetComponent<RectTransform>().rect.max.x;
            m_outBoundryDis += transform.parent.GetComponent<RectTransform>().rect.min.x;
        }
        else if (m_direction == dir.DOWN)
        {
            m_inBoundryDis += transform.parent.GetComponent<RectTransform>().rect.min.y;
            m_outBoundryDis += transform.parent.GetComponent<RectTransform>().rect.max.y;
        }

    }
	
	// Update is called once per frame
	new void FixedUpdate () 
    {
        Slide();
        SlideState();
    }

    new public void ClosePanel()
    {
        m_inView = false;

        //m_panels[3].GetComponent<SlidingPanelScript>().m_inView = false;
    }

    override protected void Slide()
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
                if (m_rectT.anchoredPosition.y > m_outBoundryDis)
                {
                    m_rectT.anchoredPosition = new Vector2(m_rectT.anchoredPosition.x, m_rectT.anchoredPosition.y - m_slideSpeed);
                    if (m_rectT.anchoredPosition.y <= m_outBoundryDis)
                        m_rectT.anchoredPosition = new Vector2(m_rectT.anchoredPosition.x, m_outBoundryDis);
                }
            }
        }
    }
}
