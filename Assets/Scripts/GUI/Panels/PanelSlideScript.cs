using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelSlideScript : MonoBehaviour {

    public enum dir { UP, DOWN, LEFT, RIGHT, NULL }

    private PanelScript m_pan;

    public bool m_inView; // This starts the sliding process of the panel
    public dir m_direction;
    public float m_slideSpeed;
    public float m_inBoundryDis;
    private float m_outBoundryDis;
    public bool m_slideArray;

    // Use this for initialization
    void Start ()
    {
        m_pan = GetComponent<PanelScript>();

        if (m_slideSpeed == 0)
            m_slideSpeed = 30.0f;

        if (m_inBoundryDis == 0)
            m_inBoundryDis = 20;

        if (m_direction == dir.UP)
            m_outBoundryDis = Screen.height + GetComponent<RectTransform>().rect.height;
        else if (m_direction == dir.RIGHT)
            m_outBoundryDis = Screen.width + GetComponent<RectTransform>().rect.width;
        else if (m_direction == dir.LEFT)
            m_outBoundryDis = 0 - GetComponent<RectTransform>().rect.width;
        else if (m_direction == dir.DOWN)
            m_outBoundryDis = 0 - GetComponent<RectTransform>().rect.height;
    }
	
	// Update is called once per frame
	void Update ()
    {
        if (m_slideArray)
            SlideArray();
        else
            Slide();
    }

    public void OpenPanel()
    {
        if (m_direction == dir.UP && !m_inView)
        {
            if (PanelManagerScript.m_history.Count > 0 && name != "Confirmation Panel")
                PanelManagerScript.m_history[PanelManagerScript.m_history.Count - 1].m_slideScript.ClosePanel();

            PanelManagerScript.m_history.Add(m_pan);
        }

        if (m_direction != dir.NULL)
            m_inView = true;

        if (name == "Confirmation Panel")
        {
            if ((int)Input.mousePosition.x < 120)
                transform.SetPositionAndRotation(new Vector3(120, 1000, transform.position.z), transform.rotation);
            else if ((int)Input.mousePosition.x > 1160)
                transform.SetPositionAndRotation(new Vector3(1160, 1000, transform.position.z), transform.rotation);
            else
                transform.SetPositionAndRotation(new Vector3(Input.mousePosition.x, 1000, transform.position.z), transform.rotation);


            if ((int)Input.mousePosition.y > 550)
                m_inBoundryDis = 550 + 80;
            else
                m_inBoundryDis = (int)Input.mousePosition.y + 80;
        }
    }

    public void ClosePanel()
    {
        m_inView = false;

        if (name == "ActionViewer Panel")
            m_pan.m_panels[3].m_slideScript.m_inView = false;
        if (name == "StatusViewer Panel")
            m_pan.m_panels[0].m_slideScript.m_inView = false;
    }

    private void Slide()
    {
        bool isOpen = false;

        if (name == "ActionView Slide")
        {
            PanelScript parent = PanelManagerScript.GetPanel("ActionViewer Panel");
            m_inBoundryDis = (int)parent.transform.position.y;
            m_outBoundryDis = (int)parent.transform.position.y - 50;
        }
        else if (tag == "Action Button" && m_pan.m_panels[0])
        {
            PanelScript parent = m_pan.m_panels[0];
            if (parent.m_panels[0].name == "HUD Panel LEFT")
                m_inBoundryDis = (int)parent.transform.position.x + 10;
            else if (parent.m_panels[0].name == "HUD Panel RIGHT")
                m_inBoundryDis = (int)parent.transform.position.x - 10;
            m_outBoundryDis = (int)parent.transform.position.x;
        }

        if (m_direction == dir.UP)
        {
            float magicNum = Screen.height - m_inBoundryDis - GetComponent<RectTransform>().rect.height * .5f; ;

            if (m_inView)
            {
                if (transform.position.y > magicNum) //if (recTrans.offsetMax.y > m_inBoundryDis)
                {
                    transform.SetPositionAndRotation(new Vector3(transform.position.x, transform.position.y - m_slideSpeed, transform.position.z), transform.rotation);
                    if (transform.position.y < magicNum)
                    {
                        transform.SetPositionAndRotation(new Vector3(transform.position.x, magicNum, transform.position.z), transform.rotation);
                        isOpen = true;
                    }
                }
                else
                    isOpen = true;
            }
            else if (transform.position.y < m_outBoundryDis)
            {
                transform.SetPositionAndRotation(new Vector3(transform.position.x, transform.position.y + m_slideSpeed, transform.position.z), transform.rotation);
                if (transform.position.y > m_outBoundryDis)
                    transform.SetPositionAndRotation(new Vector3(transform.position.x, m_outBoundryDis, transform.position.z), transform.rotation);
            }
        }
        else if (m_direction == dir.RIGHT)
        {
            float magicNum = Screen.width - m_inBoundryDis - GetComponent<RectTransform>().rect.width * .5f;

            if (m_inView)
            {
                if (transform.position.x > magicNum)
                {
                    transform.SetPositionAndRotation(new Vector3(transform.position.x - m_slideSpeed, transform.position.y, transform.position.z), transform.rotation);
                    if (transform.position.x < magicNum)
                    {
                        transform.SetPositionAndRotation(new Vector3(magicNum, transform.position.y, transform.position.z), transform.rotation);
                        isOpen = true;
                    }
                }
                else
                    isOpen = true;
            }
            else if (transform.position.x < m_outBoundryDis)
            {
                transform.SetPositionAndRotation(new Vector3(transform.position.x + m_slideSpeed, transform.position.y, transform.position.z), transform.rotation);
                if (transform.position.x > m_outBoundryDis)
                    transform.SetPositionAndRotation(new Vector3(m_outBoundryDis, transform.position.y, transform.position.z), transform.rotation);
            }
        }
        else if (m_direction == dir.LEFT)
        {
            float magicNum = m_inBoundryDis + GetComponent<RectTransform>().rect.width * .5f;

            if (m_inView)
            {
                if (transform.position.x < magicNum)
                {
                    transform.SetPositionAndRotation(new Vector3(transform.position.x + m_slideSpeed, transform.position.y, transform.position.z), transform.rotation);

                    if (transform.position.x > magicNum)
                    {
                        transform.SetPositionAndRotation(new Vector3(magicNum, transform.position.y, transform.position.z), transform.rotation);
                        isOpen = true;
                    }
                }
                else
                    isOpen = true;
            }
            else if (transform.position.x > m_outBoundryDis)
            {
                transform.SetPositionAndRotation(new Vector3(transform.position.x - m_slideSpeed, transform.position.y, transform.position.z), transform.rotation);
                if (transform.position.x < m_outBoundryDis)
                    transform.SetPositionAndRotation(new Vector3(m_outBoundryDis, transform.position.y, transform.position.z), transform.rotation);
            }
        }
        else if (m_direction == dir.DOWN)
        {
            float magicNum = m_inBoundryDis + GetComponent<RectTransform>().rect.height * .5f;

            if (m_inView)
            {
                if (transform.position.y < magicNum)
                {
                    transform.SetPositionAndRotation(new Vector3(transform.position.x, transform.position.y + m_slideSpeed, transform.position.z), transform.rotation);
                    if (transform.position.y > magicNum)
                    {
                        transform.SetPositionAndRotation(new Vector3(transform.position.x, magicNum, transform.position.z), transform.rotation);
                        isOpen = true;
                    }
                }
                else
                    isOpen = true;
            }
            else
            {
                if (transform.position.y > m_outBoundryDis && name != "DamagePreview")
                {
                    transform.SetPositionAndRotation(new Vector3(transform.position.x, transform.position.y - m_slideSpeed, transform.position.z), transform.rotation);
                    if (transform.position.y < m_outBoundryDis)
                        transform.SetPositionAndRotation(new Vector3(transform.position.x, m_outBoundryDis, transform.position.z), transform.rotation);
                }
                else if (transform.position.y > 400)
                {
                    transform.SetPositionAndRotation(new Vector3(transform.position.x, transform.position.y - m_slideSpeed, transform.position.z), transform.rotation);
                    if (transform.position.y < m_outBoundryDis)
                        transform.SetPositionAndRotation(new Vector3(transform.position.x, m_outBoundryDis, transform.position.z), transform.rotation);
                }
            }
        }

        if (name == "ActionViewer Panel" && isOpen)
            m_pan.m_panels[3].m_slideScript.m_inView = true;
        else if (name == "StatusViewer Panel" && isOpen)
            m_pan.m_panels[0].m_slideScript.m_inView = true;
    }

    private void SlideArray()
    {
        PanelScript[] panels = transform.parent.GetComponent<PanelScript>().m_panels;
        int distance = 500;

        int myPos = 0;

        for (int i = 0; i < panels.Length; i++)
        {
            if (panels[i] == GetComponent<PanelScript>())
                myPos = i;
        }

        if (myPos == 0)
            return;

        if (m_direction == dir.UP)
        {
            if (m_inView)
            {
                float myTop = GetComponent<RectTransform>().offsetMax.y - distance;
                float theirBottom = panels[myPos - 1].GetComponent<RectTransform>().offsetMin.y;

                if (myTop > theirBottom) //if (recTrans.offsetMax.y > m_inBoundryDis)
                    transform.SetPositionAndRotation(new Vector3(transform.position.x, transform.position.y - m_slideSpeed, transform.position.z), transform.rotation);
            }
            else if (transform.position.y < m_outBoundryDis)
            {
                transform.SetPositionAndRotation(new Vector3(transform.position.x, transform.position.y + m_slideSpeed, transform.position.z), transform.rotation);
                if (transform.position.y > m_outBoundryDis)
                    transform.SetPositionAndRotation(new Vector3(transform.position.x, m_outBoundryDis, transform.position.z), transform.rotation);
            }
        }
    }
}
